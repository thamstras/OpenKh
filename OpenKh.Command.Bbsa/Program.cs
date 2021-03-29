using McMaster.Extensions.CommandLineUtils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OpenKh.Command.Bbsa
{
    [Command("OpenKh.Command.Bbsa")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(ExtractCommand), typeof(ListCommand), typeof(InjectCommand), typeof(IndexCommand))]
    public class Program
    {
        static int Main(string[] args)
        {
            try
            {
                return CommandLineApplication.Execute<Program>(args);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
                return 2;
            }
            catch (ArchiveNotFoundException e)
            {
                Console.WriteLine(e.Message);
                return 3;
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
                return -1;
            }
        }

        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        private class ExtractCommand
        {
            [Required]
            [DirectoryExists]
            [Argument(0, Description = "Required. Path where the various BBSAx.DAT files are located")]
            public string InputPath { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Path where the content will be extracted", ShortName = "o", LongName = "output")]
            public string OutputDir { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Archive file name prefix. By default it is 'BBS'.", ShortName = "p", LongName = "prefix")]
            public string ArchivePrefix { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var prefix = ArchivePrefix ?? "BBS";

                if (!DoesContainBbsa(InputPath, prefix))
                    throw new ArchiveNotFoundException(InputPath, 0);

                var bbsaFileNames = Enumerable.Range(0, 5)
                    .Select(x => Path.Combine(InputPath, $"{prefix}{x}.DAT"));

                var outputDir = OutputDir ?? Path.Combine(Path.GetDirectoryName(InputPath), prefix);

                ExtractArchives(bbsaFileNames, outputDir);
                return 0;
            }

            private static void ExtractArchives(IEnumerable<string> bbsaFileNames, string outputDir)
            {
                var streams = bbsaFileNames
                    .Select(x => File.OpenRead(x))
                    .ToArray();

                var bbsa = Bbs.Bbsa.Read(streams[0]);
                foreach (var file in bbsa.Files)
                {
                    var name = file.CalculateNameWithExtension(i => streams[i]);
                    var bbsaFileStream = file.OpenStream(i => streams[i]);
                    if (bbsaFileStream == null)
                        continue;

                    var destinationFileName = Path.Combine(outputDir, name);
                    var destinationFolder = Path.GetDirectoryName(destinationFileName);
                    if (!Directory.Exists(destinationFolder))
                        Directory.CreateDirectory(destinationFolder);

                    Console.WriteLine(name);

                    using (var outStream = File.Create(destinationFileName))
                        bbsaFileStream.CopyTo(outStream);
                }
            }
        }

        private class ListCommand
        {
            [Required]
            [DirectoryExists]
            [Argument(0, Description = "Required. Path where the various BBSAx.DAT files are located")]
            public string InputPath { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Archive file name prefix. By default it is 'BBS'.", ShortName = "p", LongName = "prefix")]
            public string ArchivePrefix { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var prefix = ArchivePrefix ?? "BBS";

                if (!DoesContainBbsa(InputPath, prefix))
                    throw new ArchiveNotFoundException(InputPath, 0);

                var bbsaFileName = Path.Combine(InputPath, $"{prefix}{0}.DAT");
                using var stream = File.OpenRead(bbsaFileName);
                var bbsa = Bbs.Bbsa.Read(stream);
                foreach (var file in bbsa.Files)
                {
                    Console.WriteLine(file.Name);
                }

                return 0;
            }
        }

        private class InjectCommand
        {
            [Required]
            [DirectoryExists]
            [Argument(0, Description = "Required. Path where the various BBSAx.DAT files are located")]
            public string ArchivePath { get; set; }

            [Required]
            [FileExists]
            [Argument(1, Description = "Required. The file to inject into the BBSA")]
            public string SourceFile { get; set; }

            [Required]
            [Argument(2, Description = "Required. The file to replace inside the BBSA")]
            public string TargetFile { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Archive file name prefix. By default it is 'BBS'.", ShortName = "p", LongName = "prefix")]
            public string ArchivePrefix { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var prefix = ArchivePrefix ?? "BBS";

                if (!DoesContainBbsa(ArchivePath, prefix))
                    throw new ArchiveNotFoundException(ArchivePath, 0);

                var bbsaFileNames = Enumerable.Range(0, 5)
                    .Select(x => Path.Combine(ArchivePath, $"{prefix}{x}.DAT"));

                var streams = bbsaFileNames
                    .Select(x => File.Open(x, FileMode.Open))
                    .ToArray();

                var bbsa = Bbs.Bbsa.Read(streams[0]);

                //var offset = bbsa.GetOffset(TargetFile);
                //if (offset < 0)
                //    throw new FileNotFoundException(null, TargetFile);

                var targetDir = Path.GetDirectoryName(TargetFile);
                var targetFileName = Path.GetFileNameWithoutExtension(TargetFile);
                var targetEntry = Path.Combine(targetDir, targetFileName).Replace('\\', '/');
                Bbs.Bbsa.Entry entry;
                try
                {
                    entry = bbsa.Files.First(entry => {
                        return entry.Name == targetEntry;
                    });
                }
                catch (ArgumentNullException ex)
                {
                    throw new FileNotFoundException(null, TargetFile);
                }

                var outStream = entry.OpenStream(i => streams[i]);
                using (var inStream = File.OpenRead(SourceFile))
                {
                    if (inStream.Length > outStream.Length)
                    {
                        Console.WriteLine("File is too big to inject, sorry.");
                        return 1;
                    }
                    inStream.CopyTo(outStream);

                    if (outStream.Position < outStream.Length)
                    {
                        // TODO: null out remaining?
                    }
                }

                return 0;
            }
        }

        private class IndexCommand
        {
            [Required]
            [DirectoryExists]
            [Argument(0, Description = "Required. Path where the various BBSAx.DAT files are located")]
            public string ArchivePath { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Archive file name prefix. By default it is 'BBS'.", ShortName = "p", LongName = "prefix")]
            public string ArchivePrefix { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var prefix = ArchivePrefix ?? "BBS";

                if (!DoesContainBbsa(ArchivePath, prefix))
                    throw new ArchiveNotFoundException(ArchivePath, 0);

                var bbsaFileName = Path.Combine(ArchivePath, $"{prefix}{0}.DAT");
                using var stream = File.OpenRead(bbsaFileName);
                var bbsa = Bbs.Bbsa.Read(stream);

                bbsa.PrintIndex();

                return 0;
            }
        }

        private static bool DoesContainBbsa(string path, string prefix) =>
            File.Exists(Path.Combine(path, $"{prefix}{0}.DAT"));
    }
}
