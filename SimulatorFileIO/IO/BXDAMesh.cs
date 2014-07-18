using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class BXDAMesh
{
    public class BXDASubMesh
    {
        public double[] verts;
        public double[] norms;
        public double[] textureCoords;
        public uint[] colors;
        public int[] indicies;
    }

    public PhysicalProperties physics
    {
        get;
        private set;
    }

    public List<BXDASubMesh> meshes
    {
        get;
        private set;
    }

    public BXDAMesh()
    {
        physics = new PhysicalProperties();
        meshes = new List<BXDASubMesh>();
    }

    /// <summary>
    /// Writes the current mesh storage structure as a segmented BXDA to the given file path.
    /// </summary>
    /// <param name="path">Output path</param>
    public void WriteBXDA(String path)
    {
        BinaryWriter writer = new BinaryWriter(new FileStream(path, FileMode.OpenOrCreate));
        writer.Write(BXDIO.FORMAT_VERSION);
        writer.Write(meshes.Count);
        foreach (BXDASubMesh mesh in meshes)
        {
            int vertCount = mesh.verts.Length / 3;
            int facetCount = mesh.indicies.Length / 3;

            byte flags = (byte) ((mesh.colors != null ? 1 : 0) | (mesh.textureCoords != null ? 2 : 0) | (mesh.norms != null ? 4 :0));
            writer.Write(flags);
            writer.Write(vertCount);
            for (int i = 0; i < vertCount; i++)
            {
                int vecI = i * 3;
                int texI = i * 2;
                int colI = i;
                writer.Write(mesh.verts[vecI]);
                writer.Write(mesh.verts[vecI + 1]);
                writer.Write(mesh.verts[vecI + 2]);
                if (mesh.norms != null) {
                    writer.Write(mesh.norms[vecI]);
                    writer.Write(mesh.norms[vecI + 1]);
                    writer.Write(mesh.norms[vecI + 2]);
                }
                if (mesh.colors != null)
                {
                    writer.Write(mesh.colors[colI]);
                }
                if (mesh.textureCoords != null)
                {
                    writer.Write(mesh.textureCoords[texI]);
                    writer.Write(mesh.textureCoords[texI + 1]);
                }
            }
            writer.Write(facetCount);
            for (int i = 0; i < facetCount; i++)
            {
                int fI = i * 3;
                writer.Write(mesh.indicies[fI] - 1);
                writer.Write(mesh.indicies[fI + 1] - 1);
                writer.Write(mesh.indicies[fI + 2] - 1);
            }
        }
        physics.WriteData(writer);
        writer.Close();
    }

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

        int meshCount = reader.ReadInt32();
        for (int id = 0; id < meshCount; id++)
        {
            BXDASubMesh mesh = new BXDASubMesh();
            byte flags = reader.ReadByte();
            int vertCount = reader.ReadInt32();
            mesh.verts = new double[vertCount * 3];
            mesh.norms = (flags & 4) == 4 ? new double[vertCount * 3] : null;
            mesh.colors = (flags & 1) == 1 ? new uint[vertCount] : null;
            mesh.textureCoords = (flags & 2) == 2 ? new double[vertCount * 2] : null;
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
                if (mesh.colors != null)
                {
                    mesh.colors[colI] = reader.ReadUInt32();
                }
                if (mesh.textureCoords != null)
                {
                    mesh.textureCoords[texI] = reader.ReadDouble();
                    mesh.textureCoords[texI + 1] = reader.ReadDouble();
                }
            }

            int facetCount = reader.ReadInt32();
            mesh.indicies = new int[facetCount * 3];
            for (int i = 0; i < facetCount; i++)
            {
                int fI = i * 3;
                mesh.indicies[fI] = reader.ReadInt32();
                mesh.indicies[fI + 1] = reader.ReadInt32();
                mesh.indicies[fI + 2] = reader.ReadInt32();
            }
            meshes.Add(mesh);
        }
        physics.ReadData(reader);
        reader.Close();
    }
}