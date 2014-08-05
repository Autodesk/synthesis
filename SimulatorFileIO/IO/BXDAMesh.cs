using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a 3D object composed of one or more <see cref="BXDAMesh.BXDASubMesh"/> and physical properties of the object.
/// </summary>
public class BXDAMesh : RWObject
{
    /// <summary>
    /// Represents an indexed triangle mesh with normals and optional colors and texture coordinates.
    /// </summary>
    public class BXDASubMesh : RWObject
    {
        /// <summary>
        /// Vertex positions.  Three values (X, Y, Z) per vertex.
        /// </summary>
        public double[] verts;
        /// <summary>
        /// Vertex normals.  Three values (X, Y, Z) composing a unit vector per vertex.
        /// </summary>
        public double[] norms;

        public void WriteData(BinaryWriter writer)
        {
            int vertCount = verts.Length / 3;
            byte meshFlags = (byte) ((norms != null ? 1 : 0));

            writer.Write(meshFlags);
            writer.Write(vertCount);

            for (int i = 0; i < vertCount; i++)
            {
                int vecI = i * 3;
                int texI = i * 2;
                int colI = i;
                writer.Write(verts[vecI]);
                writer.Write(verts[vecI + 1]);
                writer.Write(verts[vecI + 2]);
                if (norms != null)
                {
                    writer.Write(norms[vecI]);
                    writer.Write(norms[vecI + 1]);
                    writer.Write(norms[vecI + 2]);
                }
            }

            writer.Write(surfaces.Count);
            foreach (BXDASurface surface in surfaces)
            {
                surface.WriteData(writer);
            }
        }
        public void ReadData(BinaryReader reader)
        {
            byte meshFlags = reader.ReadByte();
            int vertCount = reader.ReadInt32();
            verts = new double[vertCount * 3];
            norms = (meshFlags & 1) == 1 ? new double[vertCount * 3] : null;
            for (int i = 0; i < vertCount; i++)
            {
                int vecI = i * 3;
                int texI = i * 2;
                int colI = i;
                verts[vecI] = reader.ReadDouble();
                verts[vecI + 1] = reader.ReadDouble();
                verts[vecI + 2] = reader.ReadDouble();
                if (norms != null)
                {
                    norms[vecI] = reader.ReadDouble();
                    norms[vecI + 1] = reader.ReadDouble();
                    norms[vecI + 2] = reader.ReadDouble();
                }
            }

            int surfaceCount = reader.ReadInt32();
            for (int i = 0; i < surfaceCount; i++)
            {
                BXDASurface nextSurface = new BXDASurface();
                nextSurface.ReadData(reader);
                surfaces.Add(nextSurface);
            }
        }


        public List<BXDASurface> surfaces = new List<BXDASurface>();
    }

    public class BXDASurface : RWObject
    {
        public bool hasColor = false;
        /// <summary>
        /// The color of the material packed as an unsigned integer 
        /// </summary>
        public uint color = 0xFFFFFFFF;
        /// <summary>
        /// The transparency of the material.  [0-1]
        /// </summary>
        public float transparency;
        /// <summary>
        /// The translucency of the material.  [0-1]
        /// </summary>
        public float translucency;

        /// <summary>
        /// The zero based index buffer for this specific surface of the mesh.
        /// </summary>
        public int[] indicies;

        public void WriteData(BinaryWriter writer)
        {
            int facetCount = indicies.Length / 3;

            writer.Write(hasColor);
            if (hasColor)
            {
                writer.Write(color);
            }
            writer.Write(transparency);
            writer.Write(translucency);

            writer.Write(facetCount);
            for (int i = 0; i < facetCount * 3; i++)
            {
                writer.Write(indicies[i]);
            }
        }
        public void ReadData(BinaryReader reader)
        {
            hasColor = reader.ReadBoolean();

            if (hasColor)
            {
                color = reader.ReadUInt32();
            }
            transparency = reader.ReadSingle();
            translucency = reader.ReadSingle();

            int facetCount = reader.ReadInt32();
            indicies = new int[facetCount * 3];
            for (int j = 0; j < facetCount * 3; j++)
            {
                indicies[j] = reader.ReadInt32();
            }
        }
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

    /// <summary>
    /// This object's collision meshes.
    /// </summary>
    public List<BXDASubMesh> colliders
    {
        get;
        private set;
    }

    /// <summary>
    /// Creates an empty BXDA Mesh.
    /// </summary>
    public BXDAMesh()
    {
        physics = new PhysicalProperties();
        meshes = new List<BXDASubMesh>();
        colliders = new List<BXDASubMesh>();
    }

    /// <summary>
    /// Writes all the sub meshes in the given list to the given stream.
    /// </summary>
    /// <param name="writer">Output stream</param>
    /// <param name="meshes">Mesh list</param>
    private static void WriteMeshList(BinaryWriter writer, List<BXDASubMesh> meshes)
    {
        writer.Write(meshes.Count);
        foreach (BXDASubMesh mesh in meshes)
        {
            mesh.WriteData(writer);
        }
    }

    /// <summary>
    /// Reads a list of meshes from the given stream, adding them to the list passed into this function.
    /// </summary>
    /// <param name="reader">Input stream</param>
    /// <param name="meshes">List to output to</param>
    private static void ReadMeshList(BinaryReader reader, List<BXDASubMesh> meshes)
    {
        int meshCount = reader.ReadInt32();
        for (int id = 0; id < meshCount; id++)
        {
            BXDASubMesh mesh = new BXDASubMesh();
            mesh.ReadData(reader);
            meshes.Add(mesh);
        }
    }

    public void WriteData(BinaryWriter writer)
    {
        writer.Write(BXDIO.FORMAT_VERSION);
        WriteMeshList(writer, meshes);
        WriteMeshList(writer, colliders);
        physics.WriteData(writer);
    }

    public void ReadData(BinaryReader reader)
    {
        // Sanity check
        uint version = reader.ReadUInt32();
        if (version != BXDIO.FORMAT_VERSION)
        {
            reader.Close();
            throw new FormatException("File was created with format version " + BXDIO.VersionToString(version) + ", this library was compiled to read version " + BXDIO.VersionToString(BXDIO.FORMAT_VERSION));
        }
        meshes.Clear();
        colliders.Clear();
        ReadMeshList(reader, meshes);
        ReadMeshList(reader, colliders);

        physics.ReadData(reader);
    }
}