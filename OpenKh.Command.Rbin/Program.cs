using OpenKh.Common;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using OpenKh.Common.Exceptions;
using System.Collections.Generic;

namespace OpenKh.Command.Rbin
{
    [Command("OpenKh.Command.Rbin")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(ListCommand), typeof(ExtractCommand), typeof(ExtractAllCommand), typeof(UnpackCommand))]
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CommandLineApplication.Execute<Program>(args);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
            }
            catch (InvalidFileException e)
            {
                Console.WriteLine($"Invalid File Exception: {e.Message}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
            }
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }

        private class ListCommand
        {
            [Required]
            [Argument(0, "Rbin File", "The rbin file to list the contents of.")]
            public string FileName { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var fileStream = File.OpenRead(FileName);

                var rbin = Ddd.Rbin.Read(fileStream);

                Console.WriteLine($"Read version {rbin.Version} rbin containing {rbin.TOC.Count} files.");
                Console.WriteLine($"Mount point is {rbin.MountPath}");
                Console.WriteLine("Offset, Size, Compressed, Hash, Name");
                foreach(var entry in rbin.TOC)
                {
                    Console.WriteLine($"{entry.Offset:X8}, {entry.Size:D8}, {entry.IsCompressed, -5}, {entry.Hash:X8}, {entry.Name}");
                }

                return 0;
            }
        }

        private class ExtractCommand
        {
            [Required]
            [Argument(0, "Rbin File", "The rbin file to extract from")]
            public string RbinFilePath { get; set; }

            [Required]
            [Argument(1, "Target File", "The file to extract from the rbin")]
            public string Target { get; set; }

            [Argument(2, "Output Folder")]
            public string OutputFolder { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                if (string.IsNullOrWhiteSpace(OutputFolder))
                {
                    OutputFolder = Environment.CurrentDirectory;
                }

                var rbinStream = File.OpenRead(RbinFilePath);
                var rbin = Ddd.Rbin.Read(rbinStream);
                var tocEntry = rbin.TOC.Find(f => f.Name == Target);
                if (tocEntry == null)
                {
                    Console.WriteLine("Target not found in rbin");
                    return 1;
                }

                Directory.CreateDirectory(OutputFolder);
                ExtractFile(rbinStream, tocEntry, "", OutputFolder, out string wrotePath, true);

                return 0;
            }
        }

        private class ExtractAllCommand
        {
            [Required]
            [Argument(0, "Rbin File", "The rbin file to extract from")]
            public string RbinFilePath { get; set; }

            [Argument(2, "Output Folder")]
            public string OutputFolder { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                if (string.IsNullOrWhiteSpace(OutputFolder))
                {
                    OutputFolder = Environment.CurrentDirectory;
                }

                var rbinStream = File.OpenRead(RbinFilePath);
                var rbin = Ddd.Rbin.Read(rbinStream);
                foreach (var tocEntry in rbin.TOC)
                {
                    ExtractFile(rbinStream, tocEntry, "", OutputFolder, out string wrotePath);
                    Console.WriteLine($"Wrote {Path.Combine(OutputFolder, tocEntry.Name)}");
                }

                return 0;
            }
        }

        private class UnpackCommand
        {
            [Required]
            [Argument(0, "Rbin Folder")]
            public string SrcFolder { get; set; }

            [Required]
            [Argument(1, "Output Folder")]
            public string DstFolder { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                Directory.CreateDirectory(DstFolder);
                UnpackReport report = new UnpackReport();

                using (var vfsStream = File.CreateText(Path.Combine(DstFolder, "@vfs.txt")))
                {
                    var rbinList = Directory.EnumerateFiles(SrcFolder, "*.rbin", SearchOption.TopDirectoryOnly).ToList();
                    Console.WriteLine($"Found {rbinList.Count} rbins");
                    
                    System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
                    
                    int filecount = 0;
                    List<string> issueFiles = new List<string>();
                    foreach (var rbinPath in rbinList)
                    {
                        report.inputFiles.Add(rbinPath);
                        string rbinName = Path.GetFileName(rbinPath);
                        
                        using (var rbinStream = File.OpenRead(rbinPath))
                        {
                            var rbin = Ddd.Rbin.Read(rbinStream);

                            vfsStream.WriteLine($"{Path.GetFileName(rbinPath)} => {rbin.MountPath}");
                            report.rbinMountPaths.Add(rbinName, rbin.MountPath);
                            report.rbinFileCounts.Add(rbinName, 0);
                            report.rbinFileMap.Add(rbinName, new List<string>());
                            
                            foreach (var tocEntry in rbin.TOC)
                            {
                                if (ExtractFile(rbinStream, tocEntry, rbin.MountPath, DstFolder, out string wrotePath))
                                {
                                    Console.WriteLine($"\tWrote {wrotePath}");
                                    
                                    report.rbinFileCounts[rbinName]++;
                                    report.rbinFileMap[rbinName].Add(tocEntry.FullPath != string.Empty ? tocEntry.FullPath : tocEntry.Name);
                                    
                                    string fileType = Path.GetExtension(wrotePath).ToLower();
                                    if (!report.fileTypeCounts.ContainsKey(fileType))
                                    {
                                        report.fileTypeCounts.Add(fileType, 0);
                                        report.CompressedFileTypeCounts.Add(fileType, 0);
                                    }
                                    report.fileTypeCounts[fileType]++;
                                    if (tocEntry.IsCompressed)
                                        report.CompressedFileTypeCounts[fileType]++;
                                }
                                else
                                {
                                    Console.WriteLine($"\tFailed to write {Path.Combine(wrotePath, tocEntry.Name)}");
                                    issueFiles.Add(Path.Combine(Path.GetFileName(rbinPath), tocEntry.Name));
                                }
                            }
                            filecount += rbin.TOC.Count;
                        }
                    }
                    stopwatch.Stop();
                    Console.WriteLine($"Extracted {filecount - issueFiles.Count} files from {rbinList.Count} rbins in {stopwatch.ElapsedMilliseconds} milliseconds");
                    if (issueFiles.Count > 0)
                    {
                        Console.WriteLine("The following files had issues extracting:");
                        foreach (string file in issueFiles)
                        {
                            Console.WriteLine($"\t{file}");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Zero errors detected.");
                    }
                }

                using (var reportStream = File.CreateText(Path.Combine(DstFolder, "report.json")))
                {
                    string jsonTxt = System.Text.Json.JsonSerializer.Serialize<UnpackReport>(report);
                    reportStream.Write(jsonTxt);
                    Console.WriteLine("Wrote unpack report.");
                }

                return 0;
            }
        }
        
        private static bool ExtractFile(FileStream stream, Ddd.Rbin.TocEntry tocEntry, string mountPath, string basePath, out string dstPath, bool ignoreTocPath = false)
        {
            try
            {
                var filePath = tocEntry.FullPath;
                if (string.IsNullOrEmpty(filePath) || ignoreTocPath)
                {
                    filePath = Path.Combine(mountPath, tocEntry.Name);
                }
                var outPath = Path.Combine(basePath, filePath);
                dstPath = outPath;
                Directory.CreateDirectory(Path.GetDirectoryName(outPath));

                stream.Seek(tocEntry.Offset, SeekOrigin.Begin);
                if (tocEntry.IsCompressed)
                {
                    File.WriteAllBytes(outPath, Ddd.Utils.BLZ.Uncompress(stream, (int)tocEntry.Size));
                }
                else
                {
                    var writeStream = File.OpenWrite(outPath);
                    writeStream.Write(stream.ReadBytes((int)tocEntry.Size));
                    writeStream.Close();
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EXCEPTION: {ex.Message}");
                dstPath = string.Empty;
                return false;
            }
        }

        private class UnpackReport
        {
            public List<string> inputFiles= new List<string>();
            
            public Dictionary<string, int> rbinFileCounts = new Dictionary<string, int>();
            public Dictionary<string, string> rbinMountPaths = new Dictionary<string, string>();
            public Dictionary<string, List<string>> rbinFileMap = new Dictionary<string, List<string>>();
            
            public Dictionary<string, int> fileTypeCounts = new Dictionary<string, int>();
            public Dictionary<string, int> CompressedFileTypeCounts = new Dictionary<string, int>();
        }

    }
}
