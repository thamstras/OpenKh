using OpenKh.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Bbs.Graphics
{
    public class PmoVertexProcessor
    {
        // TODO: Put more useful info in these exceptions
        private float ReadInvalidF() => throw new InvalidOperationException();
        private Vector4 ReadInvalidV4() => throw new InvalidOperationException();

        private float ReadSNorm8()
        {
            sbyte c = 0; // TODO: Read the actual value
            float v = MathF.Max(c / 127.0f, -1.0f); // 127 = 2^7 - 1
            return v;
        }

        private float ReadSNorm16()
        {
            sbyte c = 0; // TODO: Read the actual value
            float v = MathF.Max(c / 32767.0f, -1.0f);   // 32767 = 2^15 - 1
            return v;
        }

        private float ReadUNorm8()
        {
            byte c = 0; // TODO: Read the actual value
            float v = c / 255.0f; // 255 = 2^8 - 1
            return v;
        }

        private float ReadUNorm16()
        {
            byte c = 0; // TODO: Read the actual value
            float v = c / 65565.0f;   // 65565 = 2^15 - 1
            return v;
        }

        private float ReadFloat()
        {
            float v = 0; // TODO: Read the actual value
            return v;
        }

        private Vector2 ReadVector2(Func<float> reader)
        {
            Vector2 v = new Vector2(reader(), reader());
            return v;
        }

        private Vector3 ReadVector3(Func<float> reader)
        {
            Vector3 v = new Vector3(reader(), reader(), reader());
            return v;
        }

        private Vector4 ReadBGR565()
        {
            short c = 0; // TODO: Read the actual value
            int r = BitsUtil.Int.GetBits(c, 0, 5);
            int g = BitsUtil.Int.GetBits(c, 5, 6);
            int b = BitsUtil.Int.GetBits(c, 11, 5);
            return new Vector4(r / 31.0f, g / 63.0f, b / 31.0f, 1.0f);
        }

        private Vector4 ReadABGR5551()
        {
            short c = 0; // TODO: Read the actual value
            int r = BitsUtil.Int.GetBits(c, 0, 5);
            int g = BitsUtil.Int.GetBits(c, 5, 5);
            int b = BitsUtil.Int.GetBits(c, 10, 5);
            int a = BitsUtil.Int.GetBits(c, 15, 1);
            return new Vector4(r / 31.0f, g / 31.0f, b / 31.0f, a);
        }

        private Vector4 ReadABGR4444()
        {
            short c = 0; // TODO: Read the actual value
            int r = BitsUtil.Int.GetBits(c, 0, 4);
            int g = BitsUtil.Int.GetBits(c, 4, 4);
            int b = BitsUtil.Int.GetBits(c, 8, 4);
            int a = BitsUtil.Int.GetBits(c, 12, 4);
            return new Vector4(r / 15.0f, g / 15.0f, b / 15.0f, a / 15.0f);
        }

        private Vector4 ReadABGR8888()
        {
            int c = 0; // TODO: Read the actual value
            int r = BitsUtil.Int.GetBits(c, 0, 8);
            int g = BitsUtil.Int.GetBits(c, 8, 8);
            int b = BitsUtil.Int.GetBits(c, 16, 8);
            int a = BitsUtil.Int.GetBits(c, 24, 8);
            return new Vector4(r / 255.0f, g / 255.0f, b / 255.0f, a / 255.0f);
        }

        private Func<float>[] _positionReaders; // position
        private Func<float>[] _attribReaders;   // weights, textures
        private Func<Vector4>[] _colorReaders;  // colors

        public PmoVertexProcessor()
        {
            _positionReaders = new Func<float>[4]
            {
                ReadInvalidF, ReadSNorm8, ReadSNorm16, ReadFloat
            };
            _attribReaders = new Func<float>[4]
            {
                ReadInvalidF, ReadUNorm8, ReadUNorm16, ReadFloat
            };
            _colorReaders = new Func<Vector4>[8]
            {
                ReadInvalidV4, ReadInvalidV4, ReadInvalidV4, ReadInvalidV4, ReadBGR565, ReadABGR5551, ReadABGR4444, ReadABGR8888
            };
        }

    }
}
