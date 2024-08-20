using OpenKh.Common;
using OpenKh.Kh1;
using OpenKh.Kh2;
using OpenKh.Bbs;
using OpenKh.Tools.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using OpenKh.Tools.Common.Imaging;
using OpenKh.Bbs;
using System.Runtime.InteropServices;
using System.Numerics;

//const string Input = @"D:\kh2\somefile.bar";
//const string Output = @"D:\kh2\modifiedfile.bar";

//Console.WriteLine("Hello OpenKH!");

//var idxs = File.OpenRead(@"E:\Hacking\KH1\KINGDOM.IDX").Using(Idx1.Read);
//var names = Idx1Name.Lookup(idxs.OrderBy(x => x.IsoBlock)).OrderBy(x => x.Entry.Hash).ToList();
//File.WriteAllLines(@"D:\asd.txt", names.Select(x => x.ToString()));
//var aaa = File
//    .ReadAllLines(Path.Combine(AppContext.BaseDirectory, "resources/kh1idx.txt"))
//    .Select(x => new
//    {
//        Name = x,
//        Hash = Idx1.GetHash(x)
//    })
//    .GroupBy(x => x.Hash)
//    .Where(x => x.Count() > 1)
//    .Select(x => new
//    {
//        Name1 = x.First().Name,
//        Name2 = x.Skip(1).First().Name
//    })
//    .ToList();
//idxs = idxs;
// Parse, modify and save a KH2 BinArc file:
//var binarc = File.OpenRead(Input).Using(Bar.Read);
//// perform here your changes
//File.Create(Output).Using(stream => Bar.Write(stream, binarc));

// Parse and convert all textures to PNG from a BBS 3D model using all CPUs:
//var pmo = File.OpenRead(Input).Using(Pmo.Read);
//pmo.texturesData
//    .Select((texture, index) => new { texture, index })
//    .AsParallel()
//    .ForAll(x => x.texture.SaveImage($"{pmo.textureInfo[x.index].TextureName}.png"));

// Connect to PCSX2 and do live editing via code:
//var process = ProcessStream.TryGetProcess(x => x.ProcessName == "pcsx2");
//using var stream = new ProcessStream(process, 0x20000000, 0x2000000);
//var gameSpeed = stream.SetPosition(0x349E0C).ReadFloat();
//stream.SetPosition(0x349E0C).Write(gameSpeed);

const string RootDir = "H:\\BBS\\Dump\\";
Dictionary<string, Dictionary<string, int>> ClassCounts = new(StringComparer.OrdinalIgnoreCase);
Dictionary<string, int> TypeCounts = new(StringComparer.OrdinalIgnoreCase)
{
    //["P1"] = 0,
    ["P2"] = 0,
    ["P4"] = 0,
    ["T1"] = 0,
    ["T2"] = 0,
    ["T4"] = 0,
    //["C1"] = 0,
    //["C2"] = 0,
    //["C3"] = 0,
    ["C4"] = 0,
    ["C5"] = 0,
    ["W1"] = 0,
    //["W2"] = 0,
    //["W4"] = 0
};

Dictionary<string, (float Min, float Max)> PosRanges = new()
{
    //["P1"] = (float.MaxValue, float.MinValue),
    ["P2"] = (float.MaxValue, float.MinValue),
    ["P4"] = (float.MaxValue, float.MinValue)
};

Dictionary<string, (int Min, int Max)> TexRanges = new()
{
    ["T1"] = (int.MaxValue, int.MinValue),
    ["T2"] = (int.MaxValue, int.MinValue),
    ["T4"] = (int.MaxValue, int.MinValue)
};

foreach (var file in Directory.EnumerateFiles(RootDir, "*.pmo", SearchOption.AllDirectories))
{
    try
    {
        string pmoClass = "UNKNOWN";
        string subPath = file.Substring(RootDir.Length);
        if (subPath.StartsWith("arc", StringComparison.OrdinalIgnoreCase))
        {
            pmoClass = subPath.Substring("arc\\".Length);
            pmoClass = pmoClass.Substring(0, pmoClass.IndexOf('\\'));
        }
        else if (subPath.StartsWith("chara", StringComparison.OrdinalIgnoreCase))
        {
            pmoClass = subPath.Substring("chara\\".Length);
            pmoClass = pmoClass.Substring(0, pmoClass.IndexOf('\\'));
            pmoClass = $"C_{pmoClass}";
        }

        Dictionary<string, int> counts;
        if (ClassCounts.ContainsKey(pmoClass))
        {
            counts = ClassCounts[pmoClass];
        }
        else
        {
            counts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
            ClassCounts[pmoClass] = counts;
        }

        using var fs = File.OpenRead(file);
        var pmo = Pmo.Read(fs);
        //Console.WriteLine($"READ {file}");
        var pmoSize = pmo.header.ModelScale;
        foreach (var chunk in pmo.Meshes)
        {
            Pmo.VertexFlags flags = Pmo.GetFlags(chunk.SectionInfo);
            string formatStr = VertexFormatString(flags);
            //Console.WriteLine($"\t{chunk.MeshNumber}: {formatStr}");

            if (counts.ContainsKey(formatStr))
                counts[formatStr] = counts[formatStr] + 1;
            else
                counts[formatStr] = 1;

            var posSizeRange = PosRanges[PosFormatStr(flags)];
            if (PosFormatStr(flags) == "P4")
            {
                var minMax = chunk.vertices.Aggregate((new Vector3(float.MaxValue), new Vector3(float.MinValue)), (curr, vert) =>
                {
                    return (
                        Vector3.Min(curr.Item1, vert),
                        Vector3.Max(curr.Item2, vert)
                    );
                });
                //var min = Math.Min(minMax.Item1.X, Math.Min(minMax.Item1.Y, minMax.Item1.Z));
                //var min = minMax.Item1.Length();
                var max = Math.Abs(Math.Min(minMax.Item2.X, Math.Min(minMax.Item2.Y, minMax.Item2.Z)));
                //var max = minMax.Item2.Length();
                if (max < posSizeRange.Min)
                    posSizeRange.Min = max;
                if (max > posSizeRange.Max)
                    posSizeRange.Max = max;
            }
            else
            {
                if (pmoSize < posSizeRange.Min)
                    posSizeRange.Min = pmoSize;
                if (pmoSize > posSizeRange.Max)
                    posSizeRange.Max = pmoSize;
            }
            PosRanges[PosFormatStr(flags)] = posSizeRange;

            if (TexFormatStr(flags) != string.Empty)
            {
                var texSize = pmo.texturesData[chunk.TextureID].Size;
                var maxTexSize = Math.Max(texSize.Width, texSize.Height);
                var texSizeRange = TexRanges[TexFormatStr(flags)];
                if (maxTexSize < texSizeRange.Min)
                    texSizeRange.Min = maxTexSize;
                else if (maxTexSize > texSizeRange.Max)
                    texSizeRange.Max = maxTexSize;
                TexRanges[TexFormatStr(flags)] = texSizeRange;
            }
        }
        
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"ERR {file} : {ex.Message}");
    }
}

Console.WriteLine();
Console.WriteLine("RESULTS");
foreach (var outer in ClassCounts)
{
    Console.WriteLine(outer.Key);
    foreach (var kvp in outer.Value.OrderBy(kvp => kvp.Key))
        Console.WriteLine($"\t{kvp.Value,4} {kvp.Key}");
}
Console.WriteLine();
foreach (var kvp in TypeCounts)
{
    Console.Write($"{kvp.Key}: {kvp.Value}  ");
}
Console.WriteLine();
Console.WriteLine();
foreach (var kvp in PosRanges)
{
    Console.WriteLine($"{kvp.Key}:\t{kvp.Value.Min}\t{kvp.Value.Max}");
}
Console.WriteLine();
foreach (var kvp in TexRanges)
{
    Console.WriteLine($"{kvp.Key}:\t{kvp.Value.Min}\t{kvp.Value.Max}");
}
return;

string VertexFormatString(Pmo.VertexFlags flags)
{
    string posStr = PosFormatStr(flags);
    if (posStr != string.Empty)
        TypeCounts[posStr] += 1;
    string texStr = TexFormatStr(flags);
    if (texStr != string.Empty)
        TypeCounts[texStr] += 1;
    string colorStr = ColorFormatStr(flags);
    if (colorStr != string.Empty)
        TypeCounts[colorStr] += 1;
    string weightStr = WeightFormatStr(flags);
    if (weightStr != string.Empty)
        TypeCounts[weightStr] += 1;
    return $"{posStr}{texStr}{colorStr}{weightStr}";
}

static string PosFormatStr(Pmo.VertexFlags flags)
{
    return flags.PositionFormat switch
    {
        Pmo.CoordinateFormat.NORMALIZED_8_BITS => "P1",
        Pmo.CoordinateFormat.NORMALIZED_16_BITS => "P2",
        Pmo.CoordinateFormat.FLOAT_32_BITS => "P4",
        _ => string.Empty,
    };
}

static string TexFormatStr(Pmo.VertexFlags flags)
{
    return flags.TextureCoordinateFormat switch
    {
        Pmo.CoordinateFormat.NORMALIZED_8_BITS => "T1",
        Pmo.CoordinateFormat.NORMALIZED_16_BITS => "T2",
        Pmo.CoordinateFormat.FLOAT_32_BITS => "T4",
        _ => string.Empty,
    };
}

static string ColorFormatStr(Pmo.VertexFlags flags)
{
    return flags.ColorFormat switch
    {
        Pmo.ColorFormat.BGR_5650_16BITS => "C1",
        Pmo.ColorFormat.ABGR_5551_16BITS => "C2",
        Pmo.ColorFormat.ABGR_4444_16BITS => "C3",
        Pmo.ColorFormat.ABGR_8888_32BITS => "C4",
        _ when flags.UniformDiffuseFlag => "C5",
        _ => string.Empty
    };
}

static string WeightFormatStr(Pmo.VertexFlags flags)
{
    return flags.WeightFormat switch
    {
        Pmo.CoordinateFormat.NORMALIZED_8_BITS => "W1",
        Pmo.CoordinateFormat.NORMALIZED_16_BITS => "W2",
        Pmo.CoordinateFormat.FLOAT_32_BITS => "W4",
        _ => string.Empty,
    };
}
