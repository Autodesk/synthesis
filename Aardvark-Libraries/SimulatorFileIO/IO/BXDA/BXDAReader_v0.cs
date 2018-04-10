using System;
using System.Collections.Generic;
using System.IO;

/// <summary>
/// Represents a 3D object composed of one or more <see cref="BXDAMesh.BXDASubMesh"/> and physical properties of the object.
/// </summary>
public partial class BXDAMesh : BinaryRWObject
{
    /// <summary>
    /// Reads with the given BinaryReader to generate mesh data of v0.
    /// </summary>
    /// <param name="reader"></param>
    private void ReadData_v0(BinaryReader reader)
    {
        // Re-reads version just in case.
        GUID = new Guid(reader.ReadString());

        meshes.Clear();
        colliders.Clear();
        ReadMeshList_v0(reader, meshes);
        ReadMeshList_v0(reader, colliders);

        physics.ReadBinaryData(reader);
    }

    /// <summary>
    /// Reads a list of meshes from the given stream, adding them to the list passed into this function.
    /// </summary>
    /// <param name="reader">Input stream</param>
    /// <param name="meshes">List to output to</param>
    private static void ReadMeshList_v0(BinaryReader reader, List<BXDASubMesh> meshes)
    {
        int meshCount = reader.ReadInt32();
        for (int id = 0; id < meshCount; id++)
        {
            BXDASubMesh mesh = new BXDASubMesh();
            mesh.ReadData_v0(reader);
            meshes.Add(mesh);
        }
    }

    /// <summary>
    /// Represents an indexed triangle mesh with normals and optional colors and texture coordinates.
    /// </summary>
    public partial class BXDASubMesh
    {
        /// <summary>
        /// Reads BXDASubMesh data of v0.
        /// </summary>
        /// <param name="reader"></param>
        public void ReadData_v0(BinaryReader reader)
        {
            byte meshFlags = reader.ReadByte();
            norms = (meshFlags & 1) == 1 ? new double[1 * 3] : null;
            verts = reader.ReadArray<double>();
            if (norms != null)
            {
                norms = reader.ReadArray<double>();
            }

            int surfaceCount = reader.ReadInt32();
            for (int i = 0; i < surfaceCount; i++)
            {
                BXDASurface nextSurface = new BXDASurface();
                nextSurface.ReadData_v0(reader);
                surfaces.Add(nextSurface);
            }
        }
    }

    /// <summary>
    /// Index data representing a face with color, transparency, translucency, and specular
    /// </summary>
    public partial class BXDASurface
    {
        /// <summary>
        /// Reads BXDASurface data of v0.
        /// </summary>
        /// <param name="reader"></param>
        public void ReadData_v0(BinaryReader reader)
        {
            hasColor = reader.ReadBoolean();

            if (hasColor)
            {
                color = reader.ReadUInt32();
            }
            transparency = reader.ReadSingle();
            translucency = reader.ReadSingle();
            specular = reader.ReadSingle();

            indicies = reader.ReadArray<Int32>();
        }
    }
}