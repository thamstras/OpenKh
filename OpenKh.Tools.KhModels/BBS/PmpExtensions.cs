using OpenKh.Bbs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.KhModels.BBS
{
    internal static class PmpExtensions
    {
        public static Matrix4x4 GetTransform(this Pmp.ObjectInfo info)
        {
            var translation = Matrix4x4.CreateTranslation(new Vector3(info.PositionX, info.PositionY, info.PositionZ));
            var rotation = Matrix4x4.CreateFromYawPitchRoll(info.RotationY, info.RotationX, info.RotationZ);
            var scale = Matrix4x4.CreateScale(info.ScaleX, info.ScaleY, info.ScaleZ);
            var transform = scale * rotation * translation;
            return transform;
        }

        public static bool ShouldMirrorFaces(this Pmp.ObjectInfo info)
        {
            return info.ScaleX * info.ScaleY * info.ScaleZ < 0;
        }
    }
}
