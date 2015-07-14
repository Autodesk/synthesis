using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;

public class _2014FieldBounding
{
    public static BXDAMesh.BXDASubMesh createBB(float x, float y, float z, float xw, float yw, float zw)
    {
        BXDAMesh.BXDASubMesh ss = new BXDAMesh.BXDASubMesh();
        ss.surfaces.Clear();
        ss.norms = null;
        ss.verts = new double[] { x - xw, y - yw, z - zw, x + xw, y + yw, z + zw };
        return ss;
    }
    public static void WriteModel()
    {
        AssemblyDocument asmDoc = (AssemblyDocument) Exporter.INVENTOR_APPLICATION.ActiveDocument;
        SurfaceExporter exp = new SurfaceExporter();
        exp.ExportAll(asmDoc.ComponentDefinition.Occurrences.OfType<ComponentOccurrence>().GetEnumerator(), (long progress, long total) =>
        {
            Console.Write(Math.Round((progress / (float) total) * 100.0f, 2) + "%\t" + progress + " / " + total);
        });
        Console.WriteLine();
        BXDAMesh mesh = exp.GetOutput();
        Apply(mesh);
        mesh.WriteToFile("C:/Temp/field.bxda");
    }
    public static void Apply(BXDAMesh meshThing)
    {
        meshThing.colliders.Clear();
        meshThing.colliders.Add(createBB(0, -10000 / 2, 0, 10000, 10000, 10000));   // Ground

        for (float signX = -1; signX <= 1; signX += 2)
        {
            for (float signZ = -1; signZ <= 1; signZ += 2)
            {
                meshThing.colliders.Add(createBB(370 * signX, 260, 827 * signZ, 10, 110, 10));  // Ends ENDVERTBAR

                meshThing.colliders.Add(createBB(281.5f * signX, 0, 738 * signZ, 10, 190, 10)); // Lowgoal_vert
                meshThing.colliders.Add(createBB(323f * signX, 91.75f, 735.6f * signZ, 91, 4.5f, 4.5f)); // Lowgoal_top_a
                meshThing.colliders.Add(createBB(280f * signX, 91.75f, 778f * signZ, 4.5f, 4.5f, 91f)); // Lowgoal_top_b
                meshThing.colliders.Add(createBB(366f * signX, 91.75f, 778f * signZ, 4.5f, 4.5f, 91f)); // Lowgoal_top_c
                meshThing.colliders.Add(createBB(323f * signX, 0, 780f * signZ, 91f, 37f, 91f)); // Lowgoal_bottom
            }


            meshThing.colliders.Add(createBB(signX * 475, 100, 0, 30, 180, 30));    // Truss ends
            meshThing.colliders.Add(createBB(0, 260, signX * 827, 30, 110, 10));  // Ends CENTER VERTBAR
            meshThing.colliders.Add(createBB(signX * 400, 0, 0, 50, 100, 1765));    // Side walls
            meshThing.colliders.Add(createBB(0, 0, signX * 860, 750, 420, 75));     // Ends BASE
            meshThing.colliders.Add(createBB(0, 310, signX * 827, 750, 10, 10));    // Ends TOPBAR
        }
        meshThing.colliders.Add(createBB(0, 175, 0, 980, 30, 30));  // Truss
    }
}
