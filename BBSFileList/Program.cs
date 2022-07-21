using System;
using System.IO;



/*void PrintFile(string fileName, int level)
{
    for (int i = 0; i < level; i++)
        Console.Write('\t');
    Console.WriteLine($"{fileName}");
}*/

enum PTN_CODE : ushort
{
    //PTNCODE_TERMINATOR = 0xffffffff,
    PTNCODE_SETFILE = 0x1,
    PTNCODE_RESET_MENUFLAG = 0x4,
    PTNCODE_SET_MENUFLAG = 0x5,
    PTNCODE_P_CHARA = 0x6,
    PTNCODE_BGM = 0x7,
    PTNCODE_SET_PARAGRAPH = 0x8,
    PTNCODE_MISSION = 0x9,
    PTNCODE_TRG_ACTION = 0xa,
    PTNCODE_ENEMY_CHANGE = 0xb
};

static class Program
{
    const string root = @"C:\BBS\Dump\arc\preset\";

    static void ParsePtx(Stream stream)
    {
        Console.WriteLine("File is {0} bytes", stream.Length);

        using BinaryReader br = new BinaryReader(stream);

        while (true)
        {
            ushort blockType = br.ReadUInt16();
            ushort blockSize = br.ReadUInt16();
            if (blockType == 0xFFFF)
            {
                Console.WriteLine("TERMINATOR");
                return;
            }
            Console.WriteLine("BLOCK SIZE {0}, CONTAINING {1}", blockSize, blockType);
            int read = 0;
            while (read < blockSize)
            {
                ushort cmd = br.ReadUInt16();
                ushort val = br.ReadUInt16();
                read += 4;
                char[] name;
                switch ((PTN_CODE)cmd)
                {
                    case PTN_CODE.PTNCODE_SETFILE:
                        Console.Write("\tSET FILE {0} ", val);
                        for (int i = 0; i < val; i++)
                        {
                            name = br.ReadChars(4);
                            read += 4;
                            Console.Write("{0} ", new string(name));
                        }
                        Console.WriteLine();
                        break;
                    case PTN_CODE.PTNCODE_BGM:
                        ushort a = br.ReadUInt16();
                        ushort b = br.ReadUInt16();
                        read += 2;
                        Console.WriteLine("\tBGM {0} FIELD: {1}, BATTLE: {2}", val, a, b);
                        break;
                    case PTN_CODE.PTNCODE_MISSION:
                        name = br.ReadChars(val * 4);
                        read += val * 4;
                        Console.WriteLine("\tMISSION {0}", new string(name));
                        break;
                    //case PTN_CODE.PTNCODE_ENEMY_CHANGE:
                    case PTN_CODE.PTNCODE_TRG_ACTION:
                        ushort trg = br.ReadUInt16();
                        ushort id = br.ReadUInt16();

                    default:
                        Console.WriteLine("FATAL UNKNOWN COMMAND {0}", cmd);
                        throw new Exception($"Parse Error");
                }
                if (read > blockSize)
                {
                    Console.WriteLine("FATAL DESYNC");
                    throw new Exception("Read Desync");
                }
            }
        }
    }

    static void ParseArc(string filePath, int level)
    {
        var arcFiles = OpenKh.Bbs.Arc.Read(File.OpenRead(filePath));
        foreach (var file in arcFiles)
        {
            /*if (file.IsLink)
                //PrintFile($"LINK => {file.Path}", level);
                PrintFile($"LINK => {file.Path}", 1);
            else
                //PrintFile(file.Name, level);
                PrintFile(Path.Combine(filePath.Remove(0, root.Length), file.Name), 0);*/
            if (file.IsLink && file.Name.EndsWith("ptx"))
            {
                Console.WriteLine(Path.Combine(filePath.Remove(0, root.Length), file.Name));
                ParsePtx(new MemoryStream(file.Data));
            }
        }
    }

    static void ParseFolder(string folder, int level = 0)
    {
        foreach (var fileSystemEntry in Directory.EnumerateFileSystemEntries(folder))
        {
            FileAttributes attributes = File.GetAttributes(fileSystemEntry);
            //PrintFile(fileSystemEntry.Split(Path.DirectorySeparatorChar).Last(), level);
            //PrintFile(fileSystemEntry.Remove(0, root.Length), 0);
            if (attributes.HasFlag(FileAttributes.Directory))
            {
                ParseFolder(fileSystemEntry, level + 1);
            }
            else
            {
                if (Path.GetExtension(fileSystemEntry) == ".arc")
                {
                    ParseArc(fileSystemEntry, level + 1);
                }
                else if (Path.GetExtension(fileSystemEntry) == ".ptx")
                {
                    Console.WriteLine(fileSystemEntry.Remove(0, root.Length));
                    ParsePtx(File.OpenRead(fileSystemEntry));
                }
            }
        }
    }

    public static int Main(string[] args)
    {
        ParseFolder(root);
        return 0;
    }
}
