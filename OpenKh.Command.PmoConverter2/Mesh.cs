using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.PmoConverter2
{
    internal struct BoneIndicies
    {
        public ushort B0, B1, B2, B3, B4, B5, B6, B7;
    }
    
    internal struct BoneWeights
    {
        public float W0, W1, W2, W3, W4, W5, W6, W7;
    }

    internal class SubMesh
    {
        public List<Vector3> Positions;
        public List<Vector2> TextureCoordinates;
        public List<Vector4> Colors;
        public List<BoneWeights> BoneWeights;
        public BoneIndicies BoneIndicies;

        public List<ushort> VertexIndicies;
        public ushort TextureIndex;
        // TODO: Blending flags?
    }

    internal class Bone
    {
        public ushort BoneIndex;
        public ushort ParentIndex;
        public string Name;
        public Matrix4x4 Transform;
        public Matrix4x4 InverseTransform;
    }

    internal class TextureInfo
    {
        public string Name;
        public string FilePath;
        // TODO: More
    }

    internal struct BoundingBox
    {
        public Vector3 Min;
        public Vector3 Max;
    }

    internal class Mesh
    {
        public float ModelScale = 1.0f;
        public BoundingBox BoundingBox;
        public List<TextureInfo> Textures = new List<TextureInfo>();
        public List<SubMesh> SubMeshes = new List<SubMesh>();
        public List<Bone> Skeleton = new List<Bone>();

        public void RecalculateBounds()
        {
            BoundingBox bounds = new BoundingBox();
            foreach (var mesh in SubMeshes)
            {
                foreach (var pos in mesh.Positions)
                {
                    bounds.Min = Vector3.Min(bounds.Min, pos);
                    bounds.Max = Vector3.Max(bounds.Max, pos);
                }
            }
            BoundingBox = bounds;
        }
    }
}
