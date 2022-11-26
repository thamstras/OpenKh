using System.Text;
using OpenKh.Common.Utils;

namespace OpenKh.Bbs
{
    public partial class Bbsa
    {
        public static uint GetHash(string text) => CRC32B.GetHash(text);

        public static uint GetHash(byte[] data) => CRC32B.GetHash(data);
        
    }
}
