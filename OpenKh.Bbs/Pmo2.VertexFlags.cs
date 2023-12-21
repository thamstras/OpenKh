namespace OpenKh.Bbs
{
    public partial class Pmo2
    {
        public struct VertexFlags
        {
            private uint raw = 0;
            
            public CoordinateFormat TextureCoordinateFormat
            {
                get => (CoordinateFormat)((raw >> 0) & 0x3);
                set => raw = (raw & (~0x3u << 0)) | (((uint)value & 0x3) << 0);
            }

            public ColorFormat ColorFormat
            {
                get => (ColorFormat)((raw >> 2) & 0x7);
                set => raw = (raw & (~0x3u << 2)) | (((uint)value & 0x7) << 2);
            }

            public CoordinateFormat NormalFormat // Unused
            {
                get => (CoordinateFormat)((raw >> 5) & 0x3);
                set => raw = (raw & (~0x3u << 5)) | (((uint)value & 0x3) << 5);
            }

            public CoordinateFormat PositionFormat
            {
                get => (CoordinateFormat)((raw >> 7) & 0x3);
                set => raw = (raw & (~0x3u << 7)) | (((uint)value & 0x3) << 7);
            }

            public CoordinateFormat WeightFormat
            {
                get => (CoordinateFormat)((raw >> 9) & 0x3);
                set => raw = (raw & (~0x3u << 9)) | (((uint)value & 0x3) << 9);
            }

            public CoordinateFormat IndicesFormat // Unused
            {
                get => (CoordinateFormat)((raw >> 11) & 0x3);
                set => raw = (raw & (~0x3u << 11)) | (((uint)value & 0x3) << 11);
            }

            //public bool Unused1;
            
            public byte SkinningWeightsCount
            {
                get => (byte)((raw >> 14) & 0x7);
                set => raw = (raw & (~0x7u << 14)) | (((uint)value & 0x7) << 14);
            }

            //public bool Unused2;

            public byte MorphWeightsCount // Unused
            {
                get => (byte)((raw >> 18) & 0x7);
                set => raw = (raw & (~0x7u << 18)) | (((uint)value & 0x7) << 18);
            }

            //public byte Unused3;

            public bool SkipTransformPipeline // Unused
            {
                get => ((raw >> 23) & 0x1) != 0;
                set => raw = (raw & (~0x3u << 23)) | ((value ? 1u : 0u) << 23);
            }

            public bool UniformDiffuseFlag
            {
                get => ((raw >> 24) & 0x1) != 0;
                set => raw = (raw & (~0x3u << 24)) | ((value ? 1u : 0u) << 24);
            }

            //public byte Unknown1;
            
            public PrimitiveType Primitive
            {
                get => (PrimitiveType)((raw >> 28) & 0xF);
                set => raw = (raw & (~0xFu << 28)) | (((uint)value & 0xF) << 28);
            }

            public uint Flags => raw;

            public VertexFlags() { }
            public VertexFlags(uint flags)
            {
                raw = flags;
            }
        }
    }
}
