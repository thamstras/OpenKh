using OpenKh.Bbs.Graphics;
using OpenKh.Common;
using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public partial class Pmo2
    {
        private static readonly IBinaryMapping Mapping =
           MappingConfiguration.DefaultConfiguration()
               .ForTypeMatrix4x4()
               .ForTypeVector4()
               .Build();

        private const UInt32 MagicCode = 0x4F4D50;

        private class FileHeader
        {
            [Data] public UInt32 MagicCode { get; set; }
            [Data] public byte Number { get; set; }
            [Data] public byte Group { get; set; }
            [Data] public byte Version { get; set; }
            [Data] public byte Padding1 { get; set; }
            [Data] public byte TextureCount { get; set; }
            [Data] public byte Padding2 { get; set; }
            [Data] public ushort Flag { get; set; }
            [Data] public UInt32 SkeletonOffset { get; set; }
            [Data] public UInt32 MeshOffset0 { get; set; }
            [Data] public ushort TriangleCount { get; set; }
            [Data] public ushort VertexCount { get; set; }
            [Data] public float ModelScale { get; set; }
            [Data] public UInt32 MeshOffset1 { get; set; }
            [Data(Count = 8)] public Vector4[] BoundingBox { get; set; }
        }

        // TODO: BBS Texture handling (needs to be shared with PMP
        internal class FileTextureInfo
        {
            [Data] public UInt32 TextureOffset { get; set; }
            [Data(Count = 12)] public string TextureName { get; set; }
            [Data] public float ScrollSpdU { get; set; }
            [Data] public float ScrollSpdV { get; set; }
            [Data(Count = 2)] public UInt32[] Padding { get; set; }
        }

        private class MeshSectionHeader
        {
            [Data] public ushort VertexCount { get; set; }
            [Data] public byte TextureID { get; set; }
            [Data] public byte VertexStride { get; set; }
            [Data] public UInt32 VertexFlags { get; set; }
            [Data] public byte Group { get; set; }
            [Data] public byte TriangleStripCount { get; set; }
            [Data] public ushort Attribute { get; set; }
        }

        public enum CoordinateFormat
        {
            NO_VERTEX,
            NORMALIZED_8_BITS,
            NORMALIZED_16_BITS,
            FLOAT_32_BITS
        }

        public enum ColorFormat
        {
            NO_COLOR,
            BGR_5650_16BITS = 4,
            ABGR_5551_16BITS,
            ABGR_4444_16BITS,
            ABGR_8888_32BITS,
        }

        public enum PrimitiveType
        {
            PRIMITIVE_POINT,
            PRIMITIVE_LINE,
            PRIMITIVE_LINE_STRIP,
            PRIMITIVE_TRIANGLE,
            PRIMITIVE_TRIANGLE_STRIP,
            PRIMITIVE_TRIANGLE_FAN,
            PRIMITIVE_QUAD
        }

        // TODO: Number and Group?

        public ushort TriangleCount { get; set; } = 0;
        
        public ushort VertexCount { get; set; } = 0;
        
        public float ModelScale { get; set; } = 1.0f;
        
        // Note: private set only fixes the array itself, the members are still settable publicly.
        // Ensures this array doesn't change size on us.
        public Vector4[] BoundingBox { get; private set; } = new Vector4[8];
        
        public List<TextureInfo> TextureInfos { get; set; } = new List<TextureInfo>();

        public static Pmo2 Read(Stream stream)
        {
            Pmo2 pmo = new();

            var header = Mapping.ReadObject<FileHeader>(stream);
            pmo.TriangleCount = header.TriangleCount;
            pmo.VertexCount = header.VertexCount;
            pmo.ModelScale = header.ModelScale;
            pmo.BoundingBox = header.BoundingBox;

            List<FileTextureInfo> texInfos = new List<FileTextureInfo>(header.TextureCount);
            for (int i = 0; i < header.TextureCount; i++)
                texInfos.Add(Mapping.ReadObject<FileTextureInfo>(stream));
            foreach (var texInfo in texInfos)
            {
                stream.SetPosition(texInfo.TextureOffset);
                pmo.TextureInfos.Add(new TextureInfo(texInfo.TextureName, texInfo.ScrollSpdU, texInfo.ScrollSpdV, Tm2.Read(stream, true).First()));
            }
        }
    }
}
