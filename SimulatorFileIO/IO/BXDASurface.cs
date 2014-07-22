using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

public class BXDASurface
{
    public BXDAMesh.BXDASubMesh subMesh = new BXDAMesh.BXDASubMesh();

    //Spot for all of the material attributes.
    public float transparency;
    public float translucency;
    public uint color;

    public static void WriteSurfaces(BinaryWriter writer, List<BXDASurface> surfaces)
    {
        writer.Write(surfaces.Count);

        foreach (BXDASurface surface in surfaces)
        {
            writer.Write(surface.transparency);
            writer.Write(surface.translucency);
            writer.Write(surface.color);

            BXDAMesh.WriteMesh(writer, surface.subMesh);
        }
    }

    public static void ReadSurfaces(BinaryReader reader, List<BXDASurface> surfaces)
    {
        int surfaceCount = reader.ReadInt32();

        for (int i = 0; i < surfaceCount; i++)
        {
            BXDASurface nextSurface = new BXDASurface();

            nextSurface.transparency = reader.ReadSingle();
            nextSurface.translucency = reader.ReadSingle();
            nextSurface.color = reader.ReadUInt32();

            nextSurface.subMesh = BXDAMesh.ReadMesh(reader);

            surfaces.Add(nextSurface);
        }
    }
}


