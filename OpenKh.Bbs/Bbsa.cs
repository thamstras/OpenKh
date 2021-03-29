using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public partial class Bbsa
    {
        protected static string[] KnownExtensions = new string[] {
            "Arc", "Bin", "Tm2", "Pmo",
            "Pam", "Pmp", "Pvd", "Bcd",
            "Fep", "Frr", "Ead", "Ese",
            "Lub", "Lad", "L2d", "Pst",
            "Epd", "Olo", "Bep", "Txa",
            "Aac", "Abc", "Scd", "Bsd",
            "Seb", "Ctd", "Ecm", "Ept",
            "Mss", "Nmd", "Ite", "Itb",
            "Itc", "Bdd", "Bdc", "Ngd",
            "Exb", "Gpd", "Exa", "Esd",
            "MTX", "INF", "COD", "CLU",
            "PMF", "ESE", "PTX", ""
        };

        protected static Dictionary<uint, string> Paths = new Dictionary<uint, string>
        {
            // TODO: temp hack until arc_dir handling properly implimented
            /*[0x0050414D] = "arc/map",
            [0x4E455645] = "arc/event",
            [0x00004350] = "arc/pc",
            [0x10004350] = "arc/pc_ven",
            [0x20004350] = "arc/pc_aqua",
            [0x30004350] = "arc/pc_terra",
            [0x4D454E45] = "arc/enemy",
            [0x53534F42] = "arc/boss",
            [0x0043504E] = "arc/npc",
            [0x4D4D4947] = "arc/gimmick",
            [0x50414557] = "arc/weapon",
            [0x4D455449] = "arc/item",
            [0x45464645] = "arc/effect",
            [0x554E454D] = "arc/menu",
            [0x00435445] = "arc/etc",
            [0x00535953] = "arc/system",
            [0x53455250] = "arc/preset",
            [0x41455250] = "arc/preset.alpha",
            [0x55424544] = "arc/debug",*/
            [0x53534f42] = "arc/boss",
            [0x5353cfc2] = "arc/boss/de",
            [0x53d34f42] = "arc/boss/es",
            [0x53534fc2] = "arc/boss/fr",
            [0x5353cf42] = "arc/boss/it",
            [0x55424544] = "arc/debug",
            [0x5542c5c4] = "arc/debug/de",
            [0x55c24544] = "arc/debug/es",
            [0x554245c4] = "arc/debug/fr",
            [0x5542c544] = "arc/debug/it",
            [0x45464645] = "arc/effect",
            [0x4546c6c5] = "arc/effect/de",
            [0x45c64645] = "arc/effect/es",
            [0x454646c5] = "arc/effect/fr",
            [0x4546c645] = "arc/effect/it",
            [0x4d454e45] = "arc/enemy",
            [0x4d45cec5] = "arc/enemy/de",
            [0x4dc54e45] = "arc/enemy/es",
            [0x4d454ec5] = "arc/enemy/fr",
            [0x4d45ce45] = "arc/enemy/it",
            [0x00435445] = "arc/etc",
            [0x0043d4c5] = "arc/etc/de",
            [0x00c35445] = "arc/etc/es",
            [0x004354c5] = "arc/etc/fr",
            [0x0043d445] = "arc/etc/it",
            [0x4e455645] = "arc/event",
            [0x4e45d6c5] = "arc/event/de",
            [0x4ec55645] = "arc/event/es",
            [0x4e4556c5] = "arc/event/fr",
            [0x4e45d645] = "arc/event/it",
            [0x4d4d4947] = "arc/gimmick",
            [0x4d4dc9c7] = "arc/gimmick/de",
            [0x4dcd4947] = "arc/gimmick/es",
            [0x4d4d49c7] = "arc/gimmick/fr",
            [0x4d4dc947] = "arc/gimmick/it",
            [0x4d455449] = "arc/item",
            [0x4d45d4c9] = "arc/item/de",
            [0x4dc55449] = "arc/item/es",
            [0x4d4554c9] = "arc/item/fr",
            [0x4d45d449] = "arc/item/it",
            [0x0050414d] = "arc/map",
            [0x0050c1cd] = "arc/map/de",
            [0x00d0414d] = "arc/map/es",
            [0x005041cd] = "arc/map/fr",
            [0x0050c14d] = "arc/map/it",
            [0x554e454d] = "arc/menu",
            [0x554ec5cd] = "arc/menu/de",
            [0x55ce454d] = "arc/menu/es",
            [0x554e45cd] = "arc/menu/fr",
            [0x554ec54d] = "arc/menu/it",
            [0x0043504e] = "arc/npc",
            [0x0043d0ce] = "arc/npc/de",
            [0x00c3504e] = "arc/npc/es",
            [0x004350ce] = "arc/npc/fr",
            [0x0043d04e] = "arc/npc/it",
            [0x00004350] = "arc/pc",
            [0x0000c3d0] = "arc/pc/de",
            [0x00804350] = "arc/pc/es",
            [0x000043d0] = "arc/pc/fr",
            [0x0000c350] = "arc/pc/it",
            [0x20004350] = "arc/pc_aqua",
            [0x2000c3d0] = "arc/pc_aqua/de",
            [0x20804350] = "arc/pc_aqua/es",
            [0x200043d0] = "arc/pc_aqua/fr",
            [0x2000c350] = "arc/pc_aqua/it",
            [0x30004350] = "arc/pc_terra",
            [0x3000c3d0] = "arc/pc_terra/de",
            [0x30804350] = "arc/pc_terra/es",
            [0x300043d0] = "arc/pc_terra/fr",
            [0x3000c350] = "arc/pc_terra/it",
            [0x10004350] = "arc/pc_ven",
            [0x1000c3d0] = "arc/pc_ven/de",
            [0x10804350] = "arc/pc_ven/es",
            [0x100043d0] = "arc/pc_ven/fr",
            [0x1000c350] = "arc/pc_ven/it",
            [0x53455250] = "arc/preset",
            [0x5345d2d0] = "arc/preset/de",
            [0x53c55250] = "arc/preset/es",
            [0x534552d0] = "arc/preset/fr",
            [0x5345d250] = "arc/preset/it",
            [0x41455250] = "arc/preset.alpha",
            [0x4145d2d0] = "arc/preset.alpha/de",
            [0x41c55250] = "arc/preset.alpha/es",
            [0x414552d0] = "arc/preset.alpha/fr",
            [0x4145d250] = "arc/preset.alpha/it",
            [0x00535953] = "arc/system",
            [0x0053d9d3] = "arc/system/de",
            [0x00d35953] = "arc/system/es",
            [0x005359d3] = "arc/system/fr",
            [0x0053d953] = "arc/system/it",
            [0x50414557] = "arc/weapon",
            [0x5041c5d7] = "arc/weapon/de",
            [0x50c14557] = "arc/weapon/es",
            [0x504145d7] = "arc/weapon/fr",
            [0x5041c557] = "arc/weapon/it",
        };

        protected static Dictionary<byte, string> PathCategories = new Dictionary<byte, string>
        {
            [0x00] = "arc_",
            [0x80] = "sound/bgm",
            [0xC0] = "lua",
            [0x90] = "sound/se/common",
            [0x91] = "sound/se/event/{1}",
            [0x92] = "sound/se/footstep/{1}",
            [0x93] = "sound/se/enemy",
            [0x94] = "sound/se/weapon",
            [0x95] = "sound/se/act",
            [0xA1] = "sound/voice/{0}/event/{1}",
            [0xAA] = "sound/voice/{0}/battle",
            [0xD0] = "message/{0}/system",
            [0xD1] = "message/{0}/map",
            [0xD2] = "message/{0}/menu",
            [0xD3] = "message/{0}/event",
            [0xD4] = "message/{0}/mission",
            [0xD5] = "message/{0}/npc_talk/{1}",
            [0xD6] = "message/{0}/network",
            [0xD7] = "message/{0}/battledice",
            [0xD8] = "message/{0}/minigame",
            [0xD9] = "message/{0}/shop",
            [0xDA] = "message/{0}/playerselect",
            [0xDB] = "message/{0}/report",
        };

        protected static Dictionary<string, uint> PathCategoriesReverse = PathCategories
            .SelectMany(x =>
                Constants.Language.Select((l, i) => new
                {
                    Key = (x.Key << 24) | (i << 21),
                    Value = x.Value.Replace("{0}", l)
                })
            )
            .SelectMany(x =>
                Constants.Worlds.Select((w, i) => new
                {
                    Key = x.Key | (i << 16),
                    Value = x.Value.Replace("{1}", w)
                })
            )
            .GroupBy(x => x.Value)
            .ToDictionary(x => x.Key, x => (uint)x.First().Key);

        protected static Dictionary<string, uint> PathsReverse =
            Paths.ToDictionary(x => x.Value, x => x.Key);

        protected static string[] AllPaths =
            PathsReverse
            .Select(x => (x.Key + '/').ToUpper())
            .Concat(new string[] { string.Empty })
            .ToArray();

        protected static string[] KnownExtensionsWithDot =
            KnownExtensions.Select(x => ('.' + x).ToUpper()).ToArray();

        protected class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public int Version { get; set; }
            [Data] public short ArcDirectoryCount { get; set; }
            [Data] public short ArcRecordCount { get; set; }
            [Data] public short ExtDirectoryCount { get; set; }
            [Data] public short ExtFileCount { get; set; }
            [Data] public int ArcRecordOffset { get; set; }
            [Data] public int ExtFileOffset { get; set; }
            [Data] public short LinkFileSector { get; set; }
            [Data] public short Archive0Sector { get; set; }
            [Data] public int TotalSectorCount { get; set; }
            [Data] public int Archive1Sector { get; set; }
            [Data] public int Archive2Sector { get; set; }
            [Data] public int Archive3Sector { get; set; }
            [Data] public int Archive4Sector { get; set; }
            public Partition<ArcDirectoryEntry>[] Partitions { get; set; }
        }

        protected class ArchivePartitionHeader
        {
            [Data] public byte Unknown00 { get; set; }
            [Data] public byte PartitionCount { get; set; }
            [Data] public short Unknown02 { get; set; }
            [Data] public short LbaStartOffset { get; set; }
            [Data] public short UnknownOffset { get; set; }
            public Partition<ArchivePartitionEntry>[] Partitions { get; set; }
        }

        protected class DirectoryEntry
        {
            public uint FileHash { get; set; }
            public uint Info { get; set; }
            public uint DirectoryHash { get; set; }
            public int Offset => (int)(Info >> 12);
            public int Size => (int)(Info & 0xFFF);

            public override string ToString() =>
                $"{DirectoryHash:X08}/{FileHash:X08} {Offset:X05} {Size:X03}";
        }

        private const int LbaLength = 8;
        protected readonly Header _header;
        protected readonly ArchivePartitionHeader _header2;
        protected readonly DirectoryEntry[] _directoryEntries;

        protected Bbsa(Stream stream)
        {
            _header = BinaryMapping.ReadObject<Header>(stream, (int)stream.Position);
            _header.Partitions = ReadPartitions<ArcDirectoryEntry>(stream, 0x30, _header.ArcDirectoryCount);
            ReadPartitionLba(_header.Partitions, stream, _header.ArcRecordOffset);

            stream.Position = _header.ExtFileOffset;
            var reader = new BinaryReader(stream);
            _directoryEntries = Enumerable.Range(0, _header.ExtFileCount)
                    .Select(x => new DirectoryEntry
                    {
                        FileHash = reader.ReadUInt32(),
                        Info = reader.ReadUInt32(),
                        DirectoryHash = reader.ReadUInt32()
                    }).ToArray();

            int header2Offset = _header.LinkFileSector * 0x800;
            stream.Position = header2Offset;
            _header2 = BinaryMapping.ReadObject<ArchivePartitionHeader>(stream);
            _header2.Partitions = ReadPartitions<ArchivePartitionEntry>(stream, header2Offset + 8, _header2.PartitionCount);
            ReadPartitionLba(_header2.Partitions, stream, header2Offset + _header2.LbaStartOffset);
            ReadUnknownStruct(_header2.Partitions, stream, header2Offset + _header2.UnknownOffset);
        }

        public IEnumerable<Entry> Files
        {
            get
            {
                foreach (var partition in _header.Partitions)
                {
                    Paths.TryGetValue(partition.Name, out var folder);

                    foreach (var lba in partition.Lba)
                    {
                        NameDictionary.TryGetValue(lba.Hash, out var fileName);

                        yield return new Entry(
                            this,
                            lba.Offset,
                            lba.Size,
                            fileName,
                            folder,
                            lba.Hash,
                            0);
                    }
                }

                foreach (var file in _directoryEntries)
                {
                    NameDictionary.TryGetValue(file.FileHash, out var fileName);
                    NameDictionary.TryGetValue(file.DirectoryHash, out var folderName);

                    yield return new Entry(
                        this,
                        file.Offset,
                        file.Size,
                        fileName,
                        folderName,
                        file.FileHash,
                        file.DirectoryHash);
                }
            }
        }

        public int GetOffset(string fileName)
        {
            var directory = Path.GetDirectoryName(fileName).Replace('\\', '/');
            var file = Path.GetFileName(fileName);
            var name = Path.GetFileNameWithoutExtension(file);

            if (!PathsReverse.TryGetValue(directory, out var pathId))
                return -1;

            var pathInfo = _header.Partitions.FirstOrDefault(x => x.Name == pathId);
            if (pathInfo == null)
                return -1;

            var hash = GetHash(name.ToUpper());
            var lba = pathInfo.Lba.FirstOrDefault(x => x.Hash == hash);
            if (lba == null)
                return -1;

            return (lba.Offset + _header.Archive0Sector) * 0x800;
        }

        public static Bbsa Read(Stream stream) => new Bbsa(stream);

        public static string GetDirectoryName(uint hash) =>
            Paths.TryGetValue(hash, out var path) ? path : CalculateFolderName(hash);

        public static uint GetDirectoryHash(string directory)
        {
            if (PathsReverse.TryGetValue(directory.ToLower(), out var hash))
                return (uint)hash;

            if (PathCategoriesReverse.TryGetValue(directory.ToLower(), out hash))
                return (uint)hash;

            return uint.MaxValue;
        }

        public void PrintIndex()
        {
            Console.WriteLine("Version {0} Arc Directories: {1} Arc Count: {2} Ext Group Count: {3} Ext File Count: {4}", _header.Version, _header.ArcDirectoryCount, _header.ArcRecordCount, _header.ExtDirectoryCount, _header.ExtFileCount);
            Console.WriteLine("#### ARCS ####");
            foreach (var arcDir in _header.Partitions)
            {
                if (!Paths.TryGetValue(arcDir.Name, out string arcDirName))
                    arcDirName = "UNKNOWN_ARC_DIR";
                Console.WriteLine($"{arcDir.Name:X04} : {arcDirName}, {arcDir.Count} files");
                foreach (var arcFile in arcDir.Lba)
                {
                    if (!NameDictionary.TryGetValue(arcFile.Hash, out var arcFileName))
                        arcFileName = "UNKNOWN_ARC";
                    Console.WriteLine($"\t{arcFileName}.arc");
                }
            }

            Console.WriteLine("#### LOOSE GROUPS ####");
            Console.WriteLine("NYI");

            Console.WriteLine("#### LOOSE FILES ####");
            foreach (var extFile in _directoryEntries)
            {
                if (!NameDictionary.TryGetValue(extFile.FileHash, out var fileName))
                    fileName = "UNKNOWN_FILE";
                if (!NameDictionary.TryGetValue(extFile.DirectoryHash, out string folderName))
                {
                    folderName = CalculateFolderName(extFile.DirectoryHash);
                    if (string.IsNullOrEmpty(folderName))
                        folderName = "UNKNOWN_DIR";
                }
                Console.WriteLine("FILE {0:X04} : {1}, DIR {2:X04} : {3}", extFile.FileHash, fileName, extFile.DirectoryHash, folderName);
            }

            Console.WriteLine("#### LINKS ####");
            foreach (var linkDirectory in _header2.Partitions)
            {
                string arcDirName = "UNKNOWN_ARC_DIR";
                Paths.TryGetValue(linkDirectory.Name, out arcDirName);
                Console.WriteLine($"{linkDirectory.Name:X04} : {arcDirName}, {linkDirectory.Count} files");
                foreach (var linkFile in linkDirectory.Lba)
                {
                    if (!NameDictionary.TryGetValue(linkFile.Hash, out var targetFileName))
                        targetFileName = "UNKNOWN_FILE";
                    Console.WriteLine("\t{0:X04} : {1}, {2} links, {3} unk", linkFile.Hash, targetFileName, linkFile.Count, linkFile.Unknown);
                    foreach(var linkedFile in linkFile.UnknownItems)
                    {
                        
                        if (!NameDictionary.TryGetValue(linkedFile.Hash, out var fileName))
                            fileName = "UNKNOWN_FILE";
                        Console.WriteLine("\t\tDir idx {0} {1:X04} : {2}", linkedFile.Unknown00, linkedFile.Hash, fileName);
                    }
                }
            }
        }
    }
}
