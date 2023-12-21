using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Bbs.Graphics
{
    public class TextureInfo
    {
        public string Name { get; set; } = string.Empty;
        public float ScrollU { get; set; } = 0.0f;
        public float ScrollV { get; set; } = 0.0f;
        public Tm2 Tm2 { get; set; }

        public TextureInfo() { }

        public TextureInfo(string name, float scrollU, float scrollV, Tm2 tm2)
        {
            Name = name;
            ScrollU = scrollU;
            ScrollV = scrollV;
            Tm2 = tm2;
        }
    }
}
