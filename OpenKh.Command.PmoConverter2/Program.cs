using OpenKh.Imaging;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using OpenKh.Bbs;
using System.Collections.Generic;
using System.Numerics;
using OpenKh.Common.Utils;

namespace OpenKh.Command.PmoConverter2
{
    [Command("OpenKh.Command.PmoConverter2")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CommandLineApplication.Execute<Program>(args);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
            }
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        [Required]
        [Argument(0, "Source File", "")]
        public string SourceFile { get; }

        [Required]
        [Argument(1, "Target File", "")]
        public string TargetFile { get; }

        public bool LimitBoneCount { get; } = false;

        private void OnExecute()
        {
            try
            {
                Convert(SourceFile, TargetFile);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
        }

        private void Convert(string fileIn, string fileOut)
        {
            using Assimp.AssimpContext assimpCtx = new Assimp.AssimpContext();
            var postProcess = Assimp.PostProcessSteps.JoinIdenticalVertices
                | Assimp.PostProcessSteps.Triangulate 
                | Assimp.PostProcessSteps.SortByPrimitiveType
                | Assimp.PostProcessSteps.FlipUVs;
            if (LimitBoneCount)
            {
                assimpCtx.SetConfig(new Assimp.Configs.VertexBoneWeightLimitConfig(8));
                postProcess |= Assimp.PostProcessSteps.LimitBoneWeights;
            }
            var scene = assimpCtx.ImportFile(fileIn, postProcess);
            
            // TODO: Bones
            if (scene.Meshes.Any(m => m.HasBones))
                throw new Exception("Skeletons Not Supported Yet!");

            Mesh theMesh = new Mesh();
            
            foreach (var mat in scene.Materials)
            {
                TextureInfo textureInfo = new TextureInfo();
                textureInfo.Name = mat.Name;
                textureInfo.FilePath = mat.TextureDiffuse.FilePath;
                theMesh.Textures.Add(textureInfo);
            }

            AddNode(theMesh, scene.RootNode, Matrix4x4.Identity, scene.Meshes);
            
            theMesh.RecalculateBounds();

            Pmo pmo = new Pmo();
            
            pmo.textureInfo = new Pmo.TextureInfo[theMesh.Textures.Count];
            for (int i = 0; i < theMesh.Textures.Count; i++)
            {
                pmo.textureInfo[i] = new Pmo.TextureInfo();
                pmo.textureInfo[i].TextureName = theMesh.Textures[i].Name;
            }
            pmo.texturesData = new List<Tm2>();
            foreach (var tex in theMesh.Textures)
            {
                using var fs = File.OpenRead(tex.FilePath);
                PngImage png = PngImage.Read(fs);
                pmo.texturesData.Add(Tm2.Create(png));
            }

            pmo.Meshes = new List<Pmo.MeshChunks>();
            foreach (var sec in theMesh.SubMeshes)
            {
                var chunk = new Pmo.MeshChunks();
                chunk.
            }
        }

        private void AddNode(Mesh mesh, Assimp.Node node, Matrix4x4 combinedTransform, List<Assimp.Mesh> meshes)
        {
            Matrix4x4 thisTransform = combinedTransform * AssimpUtils.AssimpGeneric.ToNumerics(node.Transform);
            foreach (Assimp.Mesh source in node.MeshIndices.Select(i => meshes[i]))
            {
                SubMesh dest = new SubMesh();
                dest.TextureIndex = (ushort)source.MaterialIndex;
                dest.Positions = source.Vertices.Select(v => new Vector3(v.X, v.Y, v.Z)).ToList();
                if (source.HasTextureCoords(0))
                    dest.TextureCoordinates = source.TextureCoordinateChannels[0].Select(t => new Vector2(t.X, t.Y)).ToList();
                if (source.HasVertexColors(0))
                    dest.Colors = source.VertexColorChannels[0].Select(c => new Vector4(c.R, c.G, c.B, c.A)).ToList();

                // TODO: Bones support

                if (source.HasFaces)
                {
                    foreach (var face in source.Faces)
                    {
                        // NOTE: YES, WE DO NEED BOTH CASTS. Blame whoever wrote assimp.net
                        foreach (var idx in face.Indices)
                            dest.VertexIndicies.Add((ushort)(uint)idx);
                    }
                }
                else
                {
                    // TODO: Gen index array
                }

                // TODO: Transform handling
            }

            foreach (var child in node.Children)
                AddNode(mesh, child, thisTransform, meshes);
        }

        
    }
}
