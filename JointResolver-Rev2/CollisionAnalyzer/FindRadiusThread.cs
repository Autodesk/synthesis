using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;
using System.Threading;

/// <summary>
/// Handles a single thread to find the radius of a single component.
/// </summary>
class FindRadiusThread
{
    Thread findRadius;
    ComponentOccurrence component; //The component of which to find the radius.
    BXDVector3 rotationAxis; //The vector that is being rotated around in terms of the part's axes.
    static double currentMaxRadius; //The largest radius found among all the parts in the rigid group.
    static ComponentOccurrence treadPart; //The component with the largest radius.  This is stored so its width can be found later.

    public FindRadiusThread(ComponentOccurrence passComponent, BXDVector3 passRotationAxis)
    {
        findRadius = new Thread(() => FindMaxRadius());
        component = passComponent;
        rotationAxis = passRotationAxis;
    }

    /// <summary>
    /// Sets the largest radius found back to zero.  This needs to be done between rigid groups.
    /// </summary>
    static public void Reset()
    {
        currentMaxRadius = 0;
        treadPart = null;
    }

    static public double GetRadius()
    {
        return currentMaxRadius;
    }

    static public ComponentOccurrence GetWidthComponent()
    {
        return treadPart;
    }

    public void Start()
    {
        findRadius.Start();
    }

    public void Join()
    {
        findRadius.Join();
    }

    /// <summary>
    /// Calculates the largest radius and its component.
    /// </summary>
    public void FindMaxRadius()
    {
        const double MESH_TOLERANCE = 0.5;
        int vertexCount;
        int segmentCount;
        //TODO: Figure out if arrays are right for c#.
        double[] verticeCoords = new double[10000];
        int[] verticeIndicies = new int[10000];
        double newRadius;
        Vector vertex = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector();
        Vector projectedVector;
        treadPart = null;
        FindRadiusThread newThread;
        List<FindRadiusThread> radiusThreadList = new List<FindRadiusThread>();
        Vector myRotationAxis = Program.INVENTOR_APPLICATION.TransientGeometry.CreateVector();
        Matrix asmToPart = Program.INVENTOR_APPLICATION.TransientGeometry.CreateMatrix();
        Matrix transformedVector = Program.INVENTOR_APPLICATION.TransientGeometry.CreateMatrix();
        double localMaxRadius = 0.0;
        Double boxRadius;
 
        //Calculates the largest possible radius for the part using the bounding box.
        boxRadius = Program.INVENTOR_APPLICATION.TransientGeometry.CreateVector(component.RangeBox.MaxPoint.X - component.RangeBox.MinPoint.X,
            component.RangeBox.MaxPoint.Y - component.RangeBox.MinPoint.Y, component.RangeBox.MaxPoint.Z - component.RangeBox.MinPoint.Z).Length / 2;

        Console.WriteLine("Finding radius of " + component.Name + ".");

        foreach (ComponentOccurrence sub in component.SubOccurrences)
        {
            newThread = new FindRadiusThread(sub, rotationAxis);
            radiusThreadList.Add(newThread);
            newThread.Start();
        }

        //The joint normal is changed from being relative to assembly to relative to the part axes.
        transformedVector.Cell[1, 1] = rotationAxis.x;
        transformedVector.Cell[2, 1] = rotationAxis.y;
        transformedVector.Cell[3, 1] = rotationAxis.z;

        Console.Write("Changing vector from " + transformedVector.Cell[1, 1] + ", " + transformedVector.Cell[2, 1] + ", " + transformedVector.Cell[3, 1]);

        transformedVector.TransformBy(component.Transformation);

        myRotationAxis.X = transformedVector.Cell[1, 1];
        myRotationAxis.Y = transformedVector.Cell[2, 1];
        myRotationAxis.Z = transformedVector.Cell[3, 1];

        Console.Write(" to " + transformedVector.Cell[1, 1] + ", " + transformedVector.Cell[2, 1] + ", " + transformedVector.Cell[3, 1] + ".\n");

        foreach (SurfaceBody surface in component.Definition.SurfaceBodies)
        {


            surface.CalculateStrokes(MESH_TOLERANCE, out vertexCount, out segmentCount, out verticeCoords, out verticeIndicies);

            for (int i = 0; i < verticeCoords.Length; i += 3)
            {
                //Checks if it possible for the radius to exceed the max radius.
                if (boxRadius < currentMaxRadius)
                {
                    return;
                }

                vertex.X = verticeCoords[i];
                vertex.Y = verticeCoords[i + 1];
                vertex.Z = verticeCoords[i + 2];

                projectedVector = myRotationAxis.CrossProduct(vertex);

                newRadius = projectedVector.Length;

                if (newRadius > localMaxRadius)
                {
                    localMaxRadius = newRadius;
                }
            }
        }

        lock (Program.INVENTOR_APPLICATION)
        {
            if (localMaxRadius > currentMaxRadius)
            {
                currentMaxRadius = localMaxRadius;

                treadPart = component;
            }
        }

        foreach (FindRadiusThread thread in radiusThreadList)
        {
            thread.Join();
        }

        Console.WriteLine("Found radius of " + component.Name + ".");
    }   
}

