using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using Assimp;
using System.Collections.Generic;

namespace Simulation_RD.Graphics
{
    class Model
    {
        List<Texture> textures_loaded;

        public Model(string path)
        {
            loadModel(path);
        }

        //Modeldata
        private List<Mesh> meshes;
        private string directory;
        //Functions
        void Draw()
        {
            for(int i = 0; i < meshes.Count; i++)
            {
                //meshes[i].Draw();
            }
        }

        private void loadModel(string path)
        {
            AssimpContext importer = new AssimpContext();
            Assimp.Scene scene = new Assimp.Scene();
            scene = importer.ImportFile(path, PostProcessSteps.Triangulate | PostProcessSteps.FlipUVs);

            directory = path.Substring(0, path.LastIndexOf('/'));
            processNode(scene.RootNode, scene);
        }

        private void processNode(Node node, Assimp.Scene scene)
        {
            for (int i = 0; i < node.MeshCount; i++)
            {
                Assimp.Mesh mesh = scene.Meshes[node.MeshIndices[i]];
                meshes.Add(processMesh(mesh, scene));
            }
        }

        Mesh processMesh(Assimp.Mesh mesh, Assimp.Scene scene)
        {
            List<Vertex> vertices = null;
            List<int> indices = null;
            List<Texture> textures = null;

            for(int i = 0; i < mesh.VertexCount; i++)
            {
                Vertex vert = new Vertex();
                Vector3 vec = new Vector3();
                vec.X = mesh.Vertices[i].X;
                vec.Y = mesh.Vertices[i].Y;
                vec.Z = mesh.Vertices[i].Z;
                vert.position = vec;

                vec.X = mesh.Normals[i].X;
                vec.Y = mesh.Normals[i].Y;
                vec.Z = mesh.Normals[i].Z;
                vert.normal = vec;

                if (mesh.HasTextureCoords(0))
                {
                    Vector2 vec2 = new Vector2();
                    vec2.X = mesh.TextureCoordinateChannels[0][i].X;
                    vec2.Y = mesh.TextureCoordinateChannels[0][i].Y;
                    vert.texCoord = vec2;
                }
                else vert.texCoord = new Vector2(0, 0);

                vertices.Add(vert);
            }

            for(int i = 0; i < mesh.FaceCount; i++)
            {
                Assimp.Face face = new Face();
                face = mesh.Faces[i];
                for (int j = 0; j < face.IndexCount; j++)
                {
                    indices.Add(face.Indices[j]);
                }
            }

            if(mesh.MaterialIndex >= 0)
            {
                Material material = scene.Materials[mesh.MaterialIndex];
                List<Texture> diffuseMaps = new List<Texture>();
                diffuseMaps.AddRange(loadMaterialTextures(material, TextureType.Diffuse, "texture_diffuse"));
                textures.InsertRange(textures.Count + 1, diffuseMaps);

                List<Texture> specularMaps = new List<Texture>();
                specularMaps.AddRange(loadMaterialTextures(material, TextureType.Specular, "texture_specular"));
                textures.InsertRange(textures.Count + 1, specularMaps);
            }

            return new Mesh(vertices, indices, textures);

        }

        List<Texture> loadMaterialTextures(Material mat, TextureType type, string typeName)
        {
            List<Texture> textures = null;
            for (int i = 0; i < mat.GetMaterialTextureCount(type); i++)
            {
                

                TextureSlot slot;
                mat.GetMaterialTexture(type, i, out slot);
                bool skip = false;
                for(int j = 0; j < textures_loaded.Count; j++)
                {
                    if(textures_loaded[j].path.ToString() == slot.ToString())
                    {
                        textures.Add(textures_loaded[j]);
                        skip = true;
                        break;
                    }
                }
                if (!skip)
                {
                    Texture texture;
                    texture.id = TextureFromFile(slot.ToString(), this.directory);
                    texture.type = typeName;
                    texture.path = slot;
                    textures.Add(texture);
                }
            }

            return textures;
        }

        int TextureFromFile(string path, string directory)
        {
            path = directory + "/" + path;
            int textureID;
            GL.GenTextures(1, out textureID);
            return textureID;
        }
    }
}
