using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Bbs.Archive
{
    public class Archive
    {
        private static readonly List<byte[]> archiveSignatures = new List<byte[]>()
        {
            Encoding.ASCII.GetBytes("bbsa"),
            Encoding.ASCII.GetBytes("bbs1.dat"),
            Encoding.ASCII.GetBytes("bbs2.dat"),
            Encoding.ASCII.GetBytes("bbs3.dat"),
            Encoding.ASCII.GetBytes("bbs4.dat"),
        };

        // TODO: DISPOSABLE!
        private List<Stream> archiveStreams;

        private BbsaIndex _index;

        public Archive(List<string> files)
        {
            if (files.Count != 5)
                throw new ArgumentException(/* TODO */);
            foreach (string file in files)
                if (!File.Exists(file))
                    throw new ArgumentException(/* TODO */);

            archiveStreams = new List<Stream>();
            for (int i = 0; i < archiveSignatures.Count; i++)
            {
                Stream stream = File.Open(files[i], FileMode.Open, FileAccess.Read, FileShare.Read);
                byte[] signature = stream.ReadBytes(archiveSignatures[i].Length);
                if (!Helpers.ByteArrayCompare(signature, archiveSignatures[i]))
                {
                    // ERROR
                }
                stream.SetPosition(0);
                archiveStreams.Add(stream);
            }

            _index.Read(archiveStreams[0]);
        }
    }
}
