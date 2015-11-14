using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Runtime.InteropServices;

/// <summary>
/// Represents a 3D object composed of one or more <see cref="BXDAMesh.BXDASubMesh"/> and physical properties of the object.
/// </summary>
public partial class BXDAMesh : BinaryRWObject
{
    /// <summary>
    /// Represents the revision id/version of the BXDA format (increment this when a new revision is released).
    /// </summary>
    const uint BXDA_CURRENT_VERSION = 0;

    /// <summary>
    /// The GUID for identifying the BXDAMesh.
    /// </summary>
    public Guid GUID
    {
        get;
        private set;
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
        : this(Guid.NewGuid())
    {
    }

    /// <summary>
    /// Creates an empty BXDA Mesh.
    /// </summary>
    public BXDAMesh(Guid guid)
    {
        GUID = guid;
        physics = new PhysicalProperties();
        meshes = new List<BXDASubMesh>();
        colliders = new List<BXDASubMesh>();
    }

    /// <summary>
    /// Writes all mesh data with the given BinaryWriter.
    /// </summary>
    /// <param name="writer"></param>
    public void WriteBinaryData(BinaryWriter writer)
    {
        writer.Write(BXDA_CURRENT_VERSION);

        writer.Write(GUID.ToString());

        WriteMeshList(writer, meshes);
        WriteMeshList(writer, colliders);
        physics.WriteBinaryData(writer);
    }

    /// <summary>
    /// Reads with the given BinaryReader to generate mesh data.
    /// </summary>
    /// <param name="reader"></param>
    public void ReadBinaryData(BinaryReader reader)
    {
        // Gets the version to determine how to read the file.
        uint version = reader.ReadUInt32();

        switch (version)
        {
            case 0:
                ReadData_v0(reader);
                break;
        }
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
    /// Represents an indexed triangle mesh with normals and optional colors and texture coordinates.
    /// </summary>
    public partial class BXDASubMesh
    {
        /// <summary>
        /// Vertex positions.  Three values (X, Y, Z) per vertex.
        /// </summary>
        public double[] verts;

        /// <summary>
        /// Vertex normals.  Three values (X, Y, Z) composing one unit vector per vertex.
        /// </summary>
        public double[] norms;

        /// <summary>
        /// A list of indexed surfaces that make up the mesh
        /// </summary>
        public List<BXDASurface> surfaces = new List<BXDASurface>();

        public void WriteData(BinaryWriter writer)
        {
            int vertCount = verts.Length / 3;
            byte meshFlags = (byte)((norms != null ? 1 : 0));

            writer.Write(meshFlags);
            writer.WriteArray(verts, 0, vertCount * 3);
            if (norms != null)
            {
                writer.WriteArray(norms, 0, vertCount * 3);
            }

            writer.Write(surfaces.Count);
            foreach (BXDASurface surface in surfaces)
            {
                surface.WriteData(writer);
            }
        }

        public void ReadData(BinaryReader reader)
        {

        }
    }

    /// <summary>
    /// Index data representing a face with color, transparency, translucency, and specular
    /// </summary>
    public partial class BXDASurface
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
        /// The specular intensity of the material.  [0-1]
        /// </summary>
        public float specular = 0;

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
            writer.Write(specular);

            writer.WriteArray(indicies, 0, facetCount * 3);
        }

    }

}