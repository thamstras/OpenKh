using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenKh.Common.Utils;

namespace OpenKh.Ddd
{
    public partial class Rbin
    {
        public static string[] AllPaths = TryReadLines(Path.Combine(AppContext.BaseDirectory, "resources/ddd.txt")).Distinct().ToArray();

        public static Dictionary<uint, List<string>> HashPathMap = AllPaths.GroupBy(path => GetHash(path)).ToDictionary(group => group.Key, group => group.ToList());

        //static Rbin()
        //{
        //AllPaths = TryReadLines(Path.Combine(AppContext.BaseDirectory, "resources/ddd.txt")).Distinct().ToArray();
        //HashPathMap = AllPaths.ToDictionary(x => GetHash(x), x => x);
        //HashPathMap = new Dictionary<uint, string>();
        /*foreach (string path in AllPaths)
        {
            var hash = GetHash(path);
            if (!HashPathMap.ContainsKey(hash))
                HashPathMap[hash] = path;
            else
            {
                Console.WriteLine($"KABOOM {path} {hash} {HashPathMap[hash]}");
                throw new Exception();
            }
        }*/
        //var theDict = AllPaths.GroupBy(path => GetHash(path)).ToDictionary(group => group.Key, group => group.ToList());
        //}


        public static uint GetHash(string text)
            => CRC32B.GetHash(text);

        public static uint GetHash(byte[] data)
            => CRC32B.GetHash(data);

        private static IEnumerable<string> TryReadLines(string fileName) =>
            File.Exists(fileName) ? ReadLines(fileName) : Array.Empty<string>();

        private static IEnumerable<string> ReadLines(string fileName)
        {
            using (var stream = File.OpenText(fileName))
            {
                while (true)
                {
                    var line = stream.ReadLine();
                    if (line == null)
                        break;

                    yield return line;
                }
            }
        }
    }
}
