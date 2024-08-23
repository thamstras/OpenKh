using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.AssimpUtils
{
    public static class AssimpExtensions
    {
        public static System.Numerics.Vector3 ToSystem(this Assimp.Vector3D v)
            => new System.Numerics.Vector3(v.X, v.Y, v.Z);

        public static Assimp.Vector3D ToAssimp(this System.Numerics.Vector3 v)
            => new Assimp.Vector3D(v.X, v.Y, v.Z);

        public static System.Numerics.Vector2 ToSystem(this Assimp.Vector2D v)
            => new System.Numerics.Vector2(v.X, v.Y);

        public static Assimp.Vector2D ToAssimp(this System.Numerics.Vector2 v)
            => new Assimp.Vector2D(v.X, v.Y);

        public static System.Numerics.Matrix4x4 ToSystem(this Assimp.Matrix4x4 m)
            => new System.Numerics.Matrix4x4(m.A1, m.A2, m.A3, m.A4, m.B1, m.B2, m.B3, m.B4, m.C1, m.C2, m.C3, m.C4, m.D1, m.D2, m.D3, m.D4);

        public static Assimp.Matrix4x4 ToAssimp(this System.Numerics.Matrix4x4 m)
            => new Assimp.Matrix4x4(m.M11, m.M12, m.M13, m.M14, m.M21, m.M22, m.M23, m.M24, m.M31, m.M32, m.M33, m.M34, m.M41, m.M42, m.M43, m.M44);

        public static System.Drawing.Color ToSystem(this Assimp.Color3D c)
            => System.Drawing.Color.FromArgb((int)(c.R * 255), (int)(c.G * 255), (int)(c.B * 255));

        public static Assimp.Color3D ToAssimpC3(this System.Drawing.Color c)
            => new Assimp.Color3D(c.R, c.G, c.B);

        public static System.Drawing.Color ToSystem(this Assimp.Color4D c)
            => System.Drawing.Color.FromArgb((int)(c.A * 255), (int)(c.R * 255), (int)(c.G * 255), (int)(c.B * 255));

        public static Assimp.Color4D ToAssimpC4(this System.Drawing.Color c)
            => new Assimp.Color4D(c.R, c.G, c.B, c.A);

        // TODO: MOVE THESE TO A SEPARATE LIB SO WE CAN DROP THE -WINDOWS AND ALSO THE DEPENDENCY ON ENGINE.MONOGAME!

        public static Microsoft.Xna.Framework.Matrix ToXna(this Assimp.Matrix4x4 m)
            => new Microsoft.Xna.Framework.Matrix(m.A1, m.B1, m.C1, m.D1, m.A2, m.B2, m.C2, m.D2, m.A3, m.B3, m.C3, m.D3, m.A4, m.B4, m.C4, m.D4);

        public static Assimp.Matrix4x4 ToAssimp(this Microsoft.Xna.Framework.Matrix m)
            => new Assimp.Matrix4x4(m.M11, m.M21, m.M31, m.M41, m.M12, m.M22, m.M32, m.M42, m.M13, m.M23, m.M33, m.M43, m.M14, m.M24, m.M34, m.M44);
    }
}
