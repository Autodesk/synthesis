using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;

class WheelAnalyzer
{
    public static void SaveToJoint(WheelPosition position, SkeletalJoint_Base joint)
    {
        Inventor.Point origin = Program.INVENTOR_APPLICATION.TransientGeometry.CreatePoint();
        Vector partXAxis;
        Vector partYAxis;
        Vector partZAxis;
        Vector asmXAxis = Program.INVENTOR_APPLICATION.TransientGeometry.CreateVector(1, 0, 0);
        Vector asmYAxis = Program.INVENTOR_APPLICATION.TransientGeometry.CreateVector(0, 1, 0);
        Vector asmZAxis = Program.INVENTOR_APPLICATION.TransientGeometry.CreateVector(0, 0, 1);
        WheelDriverMeta wheelDriver = new WheelDriverMeta();
        Matrix asmToPart = Program.INVENTOR_APPLICATION.TransientGeometry.CreateMatrix();
        Matrix transformedVector = Program.INVENTOR_APPLICATION.TransientGeometry.CreateMatrix();
        Vector rotationAxis = Program.INVENTOR_APPLICATION.TransientGeometry.CreateVector();
        double maxRadius = 0;
        double maxWidth = 0;
        ComponentOccurrence treadPart = null;
        Vector center;

        wheelDriver.position = position;

        if (joint is RotationalJoint)
        {
            foreach (ComponentOccurrence component in ((RotationalJoint)joint).GetWrapped().childGroup.occurrences)
            {
                //Takes the part axes and the assembly axes and creates a transformation from one to the other.
                component.Transformation.GetCoordinateSystem(out origin, out partXAxis, out partYAxis, out partZAxis);

                asmToPart.SetToAlignCoordinateSystems(origin, partXAxis, partYAxis, partZAxis, origin, asmXAxis, asmYAxis, asmZAxis);

                transformedVector.Cell[1, 1] = ((RotationalJoint)joint).axis.x;
                transformedVector.Cell[2, 1] = ((RotationalJoint)joint).axis.y;
                transformedVector.Cell[3, 1] = ((RotationalJoint)joint).axis.z;

                Console.Write("Changing vector from " + transformedVector.Cell[1, 1] + ", " + transformedVector.Cell[2, 1] + ", " + transformedVector.Cell[3, 1]);

                transformedVector.TransformBy(asmToPart);

                rotationAxis.X = transformedVector.Cell[1, 1];
                rotationAxis.Y = transformedVector.Cell[2, 1];
                rotationAxis.Z = transformedVector.Cell[3, 1];

                Console.Write(" to " + transformedVector.Cell[1, 1] + ", " + transformedVector.Cell[2, 1] + ", " + transformedVector.Cell[3, 1] + ".\n");

                maxRadius = WheelAnalyzer.FindMaxRadius(component, rotationAxis, 0.0, out treadPart);
            }

            wheelDriver.radius = (float)maxRadius;
            WheelAnalyzer.FindWheelWidthCenter(treadPart, rotationAxis, out maxWidth, out center);

            wheelDriver.width = (float)maxWidth;

            //Vector testVector = treadPart.Transformation.SetTranslation();
            treadPart.Transformation.GetCoordinateSystem(out origin, out partXAxis, out partYAxis, out partZAxis);

            asmToPart.SetToAlignCoordinateSystems(origin, asmXAxis, asmYAxis, asmZAxis, origin, partXAxis, partYAxis, partZAxis);

            center.TransformBy(asmToPart);

            wheelDriver.centerX = (float)(center.X + treadPart.Transformation.Translation.X);
            wheelDriver.centerY = (float)(center.Y + treadPart.Transformation.Translation.Y);
            wheelDriver.centerZ = (float)(center.Z + treadPart.Transformation.Translation.Z);
        }

        joint.cDriver.AddInfo(wheelDriver);
    }

    /// <summary>
    /// Calculates the radius of the component and stores the component.
    /// </summary>
    /// <param name="component">
    /// The component to analyze.
    /// </param>
    /// <param name="rotationAxis">
    /// The axis of the rotation joint.  Needs to have been converted to the part's axes, not the assemblies.
    /// </param>
    /// <param name="currentMaxRadius">
    /// Contains the largest radius found.
    /// </param>
    /// <param name="treadPart">
    /// Outputs the component occurrence with the largest radius.  Most likely this is the part of the wheel that touches the ground.
    /// </param>
    /// <returns>
    /// Returns the largest radius.
    /// </returns>
    public static double FindMaxRadius(ComponentOccurrence component, Vector rotationAxis, double currentMaxRadius, out ComponentOccurrence treadPart)
    {
        const double MESH_TOLERANCE = 0.5;
        Inventor.Point tmp = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreatePoint();
        int vertexCount;
        int segmentCount;
        //TODO: Figure out if arrays are right for c#.
        double[] verticeCoords = new double[10000];
        int[] verticeIndicies = new int[10000];
        double newRadius;
        Vector vertex = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector();
        Vector projectedVector = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector();
        treadPart = null;
        double maxRadius = 0;


        Console.WriteLine("Finding radius of " + component.Name + ".");

        foreach (ComponentOccurrence sub in component.SubOccurrences)
        {
            newRadius = FindMaxRadius(sub, rotationAxis, maxRadius, out component);

            if (newRadius > currentMaxRadius)
            {
                currentMaxRadius = newRadius;

                treadPart = component;
            }
        }

        foreach (SurfaceBody surface in component.Definition.SurfaceBodies)
        {
            surface.CalculateStrokes(MESH_TOLERANCE, out vertexCount, out segmentCount, out verticeCoords, out verticeIndicies);

            for (int i = 0; i < verticeCoords.Length; i += 3)
            {
                vertex.X = verticeCoords[i];
                vertex.Y = verticeCoords[i + 1];
                vertex.Z = verticeCoords[i + 2];

                projectedVector = rotationAxis.CrossProduct(vertex);

                newRadius = Math.Sqrt(Math.Pow(projectedVector.X, 2) + Math.Pow(projectedVector.Y, 2) + Math.Pow(projectedVector.Z, 2));

                if (newRadius > currentMaxRadius)
                {
                    currentMaxRadius = newRadius;

                    treadPart = component;
                }
            }
        }

        return currentMaxRadius;
    }


    /// <summary>
    /// Calculates the width and centerpoint of a wheel.
    /// </summary>
    /// <param name="wheelTread">
    /// The component of which to find the width and centerpoint.  For wheels, it works best if this is the component with the largest radius.
    /// </param>
    /// <param name="rotationAxis">
    /// The normal of the rotation joint in terms of the components axes, not the assembly's.
    /// </param>
    /// <param name="maxWidth">
    /// The output variable to store the largest width measured between vertices.
    /// </param>
    /// <param name="center">
    /// The output object to store the coordinates of the center with respect to the component.
    /// </param>
    public static void FindWheelWidthCenter(ComponentOccurrence wheelTread, Vector rotationAxis, out double maxWidth, out Vector center)
    {
        const double MESH_TOLERANCE = 0.5;
        Inventor.Point tmp = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreatePoint();
        int vertexCount;
        int segmentCount;
        //TODO: Figure out if arrays are right for c#.
        double[] verticeCoords = new double[10000];
        int[] verticeIndicies = new int[10000];
        double newWidth;
        Vector vertex = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector();
        Vector projectedVector = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector();
        Inventor.Point sideVertex = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreatePoint(0, 0, 0);
        double minWidth = 0.0;
        maxWidth = 0;
        center = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector(0, 0, 0);

        foreach (SurfaceBody surface in wheelTread.Definition.SurfaceBodies)
        {
            //TODO: use extending box or finding projection on plane.
            surface.CalculateStrokes(MESH_TOLERANCE, out vertexCount, out segmentCount, out verticeCoords, out verticeIndicies);
            for (int i = 0; i < verticeCoords.Length; i += 3)
            {
                center.X += verticeCoords[i];
                center.Y += verticeCoords[i + 1];
                center.Z += verticeCoords[i + 2];

                vertex.X = verticeCoords[i] - sideVertex.X;
                vertex.Y = verticeCoords[i + 1] - sideVertex.Y;
                vertex.Z = verticeCoords[i + 2] - sideVertex.Z;

                newWidth = rotationAxis.DotProduct(vertex);

                //Stores the distance to the point. 
                if (newWidth > maxWidth)
                {
                    maxWidth = newWidth;
                }
                //Changes the starting point when detecting distance for later vertices.
                if (newWidth < minWidth)
                {
                    sideVertex.X = vertex.X;
                    sideVertex.Y = vertex.Y;
                    sideVertex.Z = vertex.Z;

                    minWidth = newWidth;
                }

                //These two statements result in an end where the starting point is on one side of the wheel, and the distance that is being
                //      calculated and stored is for a vertex on the other side of the wheel.
            }

            //May cause issue if there are multiple surfaces.

            center.X = center.X / vertexCount;
            center.Y = center.Y / vertexCount;
            center.Z = center.Z / vertexCount;
        }
    }


}

