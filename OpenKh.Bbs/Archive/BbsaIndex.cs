using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xe.BinaryMapper;

namespace OpenKh.Bbs.Archive
{
    // Repack sequence
    // Build file list
    // Build ArcSets + FileSets
    // Build ExtraInfo
    // Build Index
    // Write Index + ExtraInfo to archive
    // Write each file in correct order to archive, keeping track of where
    // Write corrected index

    public sealed class BbsaIndex
    {
        public enum ArcSetType : uint
        {
            ARC_PC = 0x00004350, // 'P' 'C' '\0' '\0'
            ARC_NPC = 0x0043504E, // 'N' 'P' 'C' '\0'
            ARC_ETC = 0x00435445, // 'E' 'T' 'C' '\0'
            ARC_MAP = 0x0050414D, // 'M' 'A' 'P' '\0'
            ARC_SYSTEM = 0x00535953, // 'S' 'Y' 'S' '\0'
            ARC_PC_VEN = 0x10004350, // 'P' 'C' '\0' 10
            ARC_PC_AQUA = 0x20004350, // 'P' 'C' '\0' 20
            ARC_PC_TERRA = 0x30004350, // 'P' 'C' '\0' 30
            ARC_PRESET_ALPHA = 0x41455250, // 'P' 'R' 'E' 'A'
            ARC_EFFECT = 0x45464645, // 'E' 'F' 'F' 'E'
            ARC_ENEMY = 0x4D454E45, // 'E' 'N' 'E' 'M'
            ARC_ITEM = 0x4D455449, // 'I' 'T' 'E' 'M'
            ARC_GIMMICK = 0x4D4D4947, // 'G' 'I' 'M' 'M'
            ARC_EVENT = 0x4E455645, // 'E' 'V' 'E' 'N'
            ARC_WEAPON = 0x50414557, // 'W' 'E' 'A' 'P'
            ARC_PRESET = 0x53455250, // 'P' 'R' 'E' 'S'
            ARC_BOSS = 0x53534F42, // 'B' 'O' 'S' 'S'
            ARC_DEBUG = 0x55424544, // 'D' 'E' 'B' 'U'
            ARC_MENU = 0x554E454D, // 'M' 'E' 'N' 'U'
        }

        public enum LangType : uint
        {
            LANG_EN = 0x00000000,
            LANG_FR = 0x00000080,
            LANG_IT = 0x00008000,
            LANG_DE = 0x00008080,
            LANG_ES = 0x00800000
        }

        public const uint ArcSetTypeMask = 0xFF7F7F7F;
        public const uint LangTypeMask = 0x00808080;

        public enum FileSetType
        {
            FILE_NONE = 0,
            FILE_ARC = 1,
            FILE_BIN = 2,
            FILE_TM2 = 3,
            FILE_PMO = 4,
            FILE_PAM = 5,
            FILE_PMP = 6,
            FILE_PVD = 7,
            FILE_BCD = 8,
            FILE_FEP = 9,
            FILE_FRR = 10,
            FILE_EAD = 11,
            FILE_ESE = 12,
            FILE_LUB = 13,
            FILE_LAD = 14,
            FILE_L2D = 15,
            FILE_PST = 16,
            FILE_EPD = 17,
            FILE_OLO = 18,
            FILE_BEP = 19,
            FILE_TXA = 20,
            FILE_AAC = 21,
            FILE_ABC = 22,
            FILE_SCD = 23,
            FILE_BSD = 24,
            FILE_SEB = 25,
            FILE_CTD = 26,
            FILE_ECM = 27,
            FILE_EPT = 28,
            FILE_MSS = 29,
            FILE_NMD = 30,
            FILE_ITE = 31,
            FILE_ITB = 32,
            FILE_ITC = 33,
            FILE_BDD = 34,
            FILE_BDC = 35,
            FILE_NGD = 36,
            FILE_EXB = 37,
            FILE_GPD = 38,
            FILE_EXA = 39,
            FILE_ESD = 40,
            FILE_EDP = 41
        }

        public class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public int Version { get; set; }
            [Data] public short ArcSetCount { get; set; }
            [Data] public short ArcRecordCount { get; set; }
            [Data] public short FileSetCount { get; set; }
            [Data] public short FileRecordCount { get; set; }
            [Data] public int ArcRecordOffset { get; set; }
            [Data] public int FileRecordOffset { get; set; }
            [Data] public short ExtraInfoSector { get; set; }
            [Data] public short DataStartSector { get; set; }
            [Data] public int TotalSectorCount { get; set; }
            [Data] public int Archive1Sector { get; set; }
            [Data] public int Archive2Sector { get; set; }
            [Data] public int Archive3Sector { get; set; }
            [Data] public int Archive4Sector { get; set; }
        }

        public class ArcSetDef
        {
            [Data] public uint TypeCode { get; set; }
            [Data] public short RecordCount { get; set; }
            [Data] public short RecordOffset { get; set; }

            public ArcSetType SetType => (ArcSetType)(TypeCode & ArcSetTypeMask);
            public LangType LangType => (LangType)(TypeCode & LangTypeMask);
        }

        public class FileSetDef
        {
            [Data] public short RecordCount { get; set; }
            [Data] public short RecordOffset { get; set; }
        }

        public class ArcRecord
        {
            [Data] public uint NameHash { get; set; }
            [Data] public uint Info { get; set; }

            // TODO: Setters?
            public int StartSector => (int)(Info >> 12);
            public int SectorCount => (int)(Info & 0xFFF);
        }

        public class FileRecord
        {
            [Data] public uint NameHash { get; set; }
            [Data] public uint Info { get; set; }
            [Data] public uint DirHash { get; set; }

            // TODO: Setters?
            public int StartSector => (int)(Info >> 12);
            public int SectorCount => (int)(Info & 0xFFF);
        }

        public class ArcSet
        {
            public ArcSetType Type { get; set; }
            public LangType Lang { get; set; }
            public List<ArcRecord> Arcs { get; set; }
        }

        public class FileSet
        {
            public FileSetType Type { get; set; }
            public List<FileRecord> Files { get; set; }
        }

        private Header _header;
        private ArcSetDef[] _arcSets;
        private FileSetDef[] _fileSets;
        private ArcRecord[] _arcRecords;
        private FileRecord[] _fileRecords;

        public void Read(Stream stream)
        {
            _header = BinaryMapping.ReadObject<Header>(stream);
            _arcSets = Enumerable.Range(0, _header.ArcSetCount)
                .Select(x => BinaryMapping.ReadObject<ArcSetDef>(stream))
                .ToArray();
            _fileSets = Enumerable.Range(0, _header.FileSetCount)
                .Select(x => BinaryMapping.ReadObject<FileSetDef>(stream))
                .ToArray();
            stream.Seek(_header.ArcRecordOffset, SeekOrigin.Begin);
            _arcRecords = Enumerable.Range(0, _header.ArcRecordCount)
                .Select(x => BinaryMapping.ReadObject<ArcRecord>(stream))
                .ToArray();
            stream.Seek(_header.FileRecordOffset, SeekOrigin.Begin);
            _fileRecords = Enumerable.Range(0, _header.FileRecordCount)
                .Select(x => BinaryMapping.ReadObject<FileRecord>(stream))
                .ToArray();
        }

        public IEnumerable<ArcSet> AllArcSets
        {
            get
            {
                foreach (var arcSet in _arcSets)
                {
                    yield return new ArcSet()
                    {
                        Type = arcSet.SetType,
                        Lang = arcSet.LangType,
                        Arcs = _arcRecords.Skip(arcSet.RecordOffset).Take(arcSet.RecordCount).ToList()
                    };
                }
            }
        }

        public IEnumerable<FileSet> AllFileSets
        {
            get
            {
                for (int i = 0; i < _fileSets.Count(); i++)
                {
                    var fileSet = _fileSets[i];
                    yield return new FileSet()
                    {
                        Type = (FileSetType)i,
                        Files = _fileRecords.Skip(fileSet.RecordOffset).Take(fileSet.RecordCount).ToList()
                    };
                }
            }
        }

        public int ExtraInfoSector => _header.ExtraInfoSector;
        public int TotalSectorCount => _header.TotalSectorCount;
        public int Archive0Sector => _header.DataStartSector;
        public int Archive1Sector => _header.Archive1Sector;
        public int Archive2Sector => _header.Archive1Sector;
        public int Archive3Sector => _header.Archive1Sector;
        public int Archive4Sector => _header.Archive1Sector;
    }
}
