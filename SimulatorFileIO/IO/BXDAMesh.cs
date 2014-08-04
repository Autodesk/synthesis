using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

/// <summary>
/// Represents a 3D object composed of one or more <see cref="BXDAMesh.BXDASubMesh"/> and physical properties of the object.
/// </summary>
public class BXDAMesh
{
    /// <summary>
    /// Represents an indexed triangle mesh with normals and optional colors and texture coordinates.
    /// </summary>
    public class BXDASubMesh
    {
        /// <summary>
        /// Vertex positions.  Three values (X, Y, Z) per vertex.
        /// </summary>
        public double[] verts;
        /// <summary>
        /// Vertex normals.  Three values (X, Y, Z) composing a unit vector per vertex.
        /// </summary>
        public double[] norms;


        public List<BXDASurface> surfaces = new List<BXDASurface>();
    }

    public class BXDASurface
    {
        public bool hasColor = false;
        public uint color;
        public float transparency;
        public float translucency;

        //Indecies for polygons associated with Facet.
        public int[] indicies;
    }

    /// <summary>
    /// The physical properties of this object.
    /// </summary>
    public PhysicalProperties physics
    {
        get;
        private set;
    }

    /// <summary>
    /// This object's sub meshes.
    /// </summary>
    public List<BXDASubMesh> meshes
    {
        get;
        private set;
    }

    public List<BXDASubMesh> colliders
    {
        get;
        private set;
    }

    public BXDAMesh()
    {
        physics = new PhysicalProperties();
        meshes = new List<BXDASubMesh>();
        colliders = new List<BXDASubMesh>();
    }

    private static void WriteMeshList(BinaryWriter writer, List<BXDASubMesh> meshes)
    {
        writer.Write(meshes.Count);
        foreach (BXDASubMesh mesh in meshes)
        {
            int vertCount = mesh.verts.Length / 3;
            byte meshFlags = (byte)((mesh.norms != null ? 1 : 0));

            writer.Write(meshFlags);
            writer.Write(vertCount);           

            for (int i = 0; i < vertCount; i++)
            {
                int vecI = i * 3;
                int texI = i * 2;
                int colI = i;
                writer.Write(mesh.verts[vecI]);
                writer.Write(mesh.verts[vecI + 1]);
                writer.Write(mesh.verts[vecI + 2]);
                if (mesh.norms != null)
                {
                    writer.Write(mesh.norms[vecI]);
                    writer.Write(mesh.norms[vecI + 1]);
                    writer.Write(mesh.norms[vecI + 2]);
                }
            }

            writer.Write(mesh.surfaces.Count);
            foreach (BXDASurface surface in mesh.surfaces)
            { 
                int facetCount = surface.indicies.Length / 3;

                writer.Write(surface.hasColor);
                if (surface.hasColor)
                {
                    writer.Write(surface.color);
                }
                writer.Write(surface.transparency);
                writer.Write(surface.translucency);

                writer.Write(facetCount);
                byte[] result = new byte[surface.indicies.Length * sizeof(int)];
                for (int i = 0; i < facetCount; i++)
                {
                    int fI = i * 3;
                    // Integrity check
                    /*for (int j = 0; j < 3; j++)
                    {
                        if (surface.indicies[fI + j] < 0 || surface.indicies[fI + j] >= mesh.verts.Length)
                        {
                            Console.WriteLine("Tris #" + i + " failed.  Index is " + surface.indicies[fI + j]);
                            Console.ReadLine();
                        }
                    } MAINLY FOR DEBUGGING */
                    writer.Write(surface.indicies[fI]);
                    writer.Write(surface.indicies[fI + 1]);
                    writer.Write(surface.indicies[fI + 2]);
                }
            } 
        }
    }

    private static void ReadMeshList(BinaryReader reader, List<BXDASubMesh> meshes)
    {
        int meshCount = reader.ReadInt32();
        for (int id = 0; id < meshCount; id++)
        {
            BXDASubMesh mesh = new BXDASubMesh();
            byte meshFlags = reader.ReadByte();
            int vertCount = reader.ReadInt32();
            mesh.verts = new double[vertCount * 3];
            mesh.norms = (meshFlags & 1) == 1 ? new double[vertCount * 3] : null;
            for (int i = 0; i < vertCount; i++)
            {
                int vecI = i * 3;
                int texI = i * 2;
                int colI = i;
                mesh.verts[vecI] = reader.ReadDouble();
                mesh.verts[vecI + 1] = reader.ReadDouble();
                mesh.verts[vecI + 2] = reader.ReadDouble();
                if (mesh.norms != null)
                {
                    mesh.norms[vecI] = reader.ReadDouble();
                    mesh.norms[vecI + 1] = reader.ReadDouble();
                    mesh.norms[vecI + 2] = reader.ReadDouble();
                }
            }

            int surfaceCount = reader.ReadInt32();



            for (int i = 0; i < surfaceCount; i++)
            {
                BXDASurface nextSurface = new BXDASurface();

                nextSurface.hasColor = reader.ReadBoolean();

                if (nextSurface.hasColor)
                {
                    nextSurface.color = reader.ReadUInt32();
                }
                nextSurface.transparency = reader.ReadSingle();
                nextSurface.translucency = reader.ReadSingle();

                int facetCount = reader.ReadInt32();
                nextSurface.indicies = new int[facetCount * 3];
                for (int j = 0; j < facetCount; j++)
                {
                    int fJ = j * 3;
                    nextSurface.indicies[fJ] = reader.ReadInt32();
                    nextSurface.indicies[fJ + 1] = reader.ReadInt32();
                    nextSurface.indicies[fJ + 2] = reader.ReadInt32();
                }

                mesh.surfaces.Add(nextSurface);
            }

            meshes.Add(mesh);
        }
    }

    /// <summary>
    /// Writes the current mesh storage structure as a segmented BXDA to the given file path.
    /// </summary>
    /// <param name="path">Output path</param>
    public void WriteBXDA(String path)
    {
        BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate));
        writer.Write(BXDIO.FORMAT_VERSION);
        WriteMeshList(writer, meshes);
        WriteMeshList(writer, colliders);
        physics.WriteData(writer);
        writer.Close();
    }

    /// <summary>
    /// Reads the BXDA file stored at the given path.
    /// </summary>
    /// <param name="path">The file to read from</param>
    /// <exception cref="FormatException">If the given file was created by a different API version.</exception>
    public void ReadBXDA(string path)
    {
        meshes.Clear();
        BinaryReader reader = new BinaryReader(new FileStream(path, FileMode.Open));

        // Sanity check
        uint version = reader.ReadUInt32();
        if (version != BXDIO.FORMAT_VERSION)
        {
            reader.Close();
            throw new Exception("\"" + path + "\" was created with format version " + BXDIO.VersionToString(version) + ", this library was compiled to read version " + BXDIO.VersionToString(BXDIO.FORMAT_VERSION));
        }
        meshes.Clear();
        colliders.Clear();
        ReadMeshList(reader, meshes);
        ReadMeshList(reader, colliders);

        physics.ReadData(reader);
        reader.Close();
    }
}