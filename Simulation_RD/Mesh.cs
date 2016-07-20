using System;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Linq;
using Assimp;
using BulletSharp;

namespace Simulation_RD
{
    struct Vertex
    {
        public Vector3 position;
        public Vector3 normal;
        public Vector2 texCoord;

        public static int SizeInBytes()
        {
            return Vector3.SizeInBytes * 2 + Vector2.SizeInBytes;
        }
    };

    public struct Texture
    {
        public int id;
        public string type;
        public TextureSlot path;
    };

    class Mesh// : BXDAMesh //Takes a vertex array, an index array, and a texture coordinate array.
    {
        public List<Vertex> vertices;
        public List<int> indices;
        public List<Texture> textures;
        private double[] vertexData;

        private int VBO, VAO, EBO;
        private int shader;

        public Mesh(List<Vertex> vertices, List<int> indices, List<Texture> textures)
        {
            this.vertices = vertices;
            this.indices = indices;
            this.textures = textures;

            setupMesh();
        }

        public Mesh(BXDAMesh.BXDASubMesh mesh, Vector3 position)
        {
            Vector3[] vertData = MeshUtilities.DataToVector(mesh.verts);
            Vector3[] normsData = MeshUtilities.DataToVector(mesh.norms);
            vertexData = mesh.verts;

            //Translate objects
            for(int i = 0; i < vertexData.Length;)
            {
                vertexData[i++] += position.X;
                vertexData[i++] += position.Y;
                vertexData[i++] += position.Z;
            }

            for (int i = 0; i < vertData.Length; i++)
            {
                vertData[i] *= 0.001f;
            }

            vertices = new List<Vertex>();
            //for(int i = 0; i < vertData.Length; i++)
            //{
            //    Vertex toAdd = new Vertex()
            //    {
            //        position = vertData[i],
            //        normal = normsData[i],
            //        texCoord = new Vector2(0, 0)
            //    };
            //    vertices.Add(toAdd);
            //}
            vertices = vertData.Zip(normsData, (v, n) => new Vertex { position = v, normal = n, texCoord = new Vector2(0, 0) }).ToList();

            indices = new List<int>();
            mesh.surfaces.ForEach((s) => indices = new List<int>(indices.Concat(s.indicies)));

            IEnumerable<double> temp = vertexData.Zip(indices, (v, i) => vertexData[i]);
            //vertexData = temp.ToArray();

            textures = new List<Texture>();
            setupMesh();
        }

        private void setupMesh()
        {
            //setup and generate all buffer objects
            VAO = GL.GenVertexArray();
            //EBO = GL.GenBuffer();
            GL.BindVertexArray(VAO);

            VBO = GL.GenBuffer();

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            //GL.BufferData(BufferTarget.ArrayBuffer, (IntPtr)(Vertex.SizeInBytes() * vertices.Count), vertices.ToArray(), BufferUsageHint.DynamicDraw);
            GL.BufferData<double>(BufferTarget.ArrayBuffer, (IntPtr)(sizeof(double) * vertexData.Length), vertexData, BufferUsageHint.StaticDraw);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Double, false, 0, 0);
            GL.EnableVertexAttribArray(0);

            //GL.BindBuffer(BufferTarget.ElementArrayBuffer, EBO);
            //GL.BufferData(BufferTarget.ElementArrayBuffer, (IntPtr)(indices.Count * sizeof(int)), indices.ToArray(), BufferUsageHint.StaticDraw);

            //vertex positions
            //GL.EnableVertexAttribArray(0);
            //GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes(), 0);

            //vertex normals
            //GL.EnableVertexAttribArray(1);
            //GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, Vertex.SizeInBytes(), Vertex.SizeInBytes() * vertices.Count);

            //vertex texture coordinates
            //GL.EnableVertexAttribArray(2);
            //GL.VertexAttribPointer(2, 2, VertexAttribPointerType.Float, false, Vertex.SizeInBytes(), (Vertex.SizeInBytes() * vertices.Count) + (sizeof(int) * indices.Count));
            //                 |\/|
            //vertices---------\||/
            //GL.BindVertexArray(0);
        }

        public void Draw(RigidBody b)
        {
            //int diffuseNr = 1;
            //int specularNr = 1;
            //for (int i = 0; i < textures.Count; i++)
            //{
            //    //activate texture unit before binding
            //    GL.ActiveTexture(TextureUnit.Texture0 + i);

            //    //retrieve texture number (N in diffuse_textureN)
            //    string number;
            //    string name = textures[i].ToString();
            //    if (name == "texture_diffuse")
            //    {
            //        diffuseNr++;
            //        number = diffuseNr.ToString();
            //    }
            //    else if (name == "texture_specular")
            //    {
            //        specularNr++;
            //        number = specularNr.ToString();
            //    }

            //}
            //GL.ActiveTexture(TextureUnit.Texture0);

            //draw the mesh
            GL.BindVertexArray(VAO);
            //GL.DrawElements(OpenTK.Graphics.OpenGL.PrimitiveType.TriangleStrip, indices.Count, DrawElementsType.UnsignedInt, 0);
            //GL.BindVertexArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.DrawArrays(OpenTK.Graphics.OpenGL.PrimitiveType.Triangles, 0, vertexData.Length / 9);

            //Immediate Drawing, AKA NOT that buffered shit that doesn't work
            //GL.Begin(BeginMode.Triangles);

            //for(int i = 0; i < indices.Count; i++)
            //{
            //    GL.Vertex3(vertices[indices[i]].position);
            //}
            //GL.End();
        }
    }
}
