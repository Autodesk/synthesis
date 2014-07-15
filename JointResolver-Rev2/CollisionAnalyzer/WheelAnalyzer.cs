using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;
using System.Threading;

class WheelAnalyzer
{
    /// <summary>
    /// Saves the various information for a wheel collider.
    /// </summary>
    /// <param name="joint">
    /// The joint associated with the wheel.
    /// </param>
    /// <param name="type">
    /// The kind of wheel attached to the joint, such as omni or mecanum.
    /// </param>
    public static void SaveToJoint(SkeletalJoint_Base joint, WheelType type, FrictionLevel friction)
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
        double maxWidth = 0;
        ComponentOccurrence treadPart = null;
        Vector center;
        List<FindRadiusThread> radiusThreadList = new List<FindRadiusThread>();
        FindRadiusThread newRadiusThread;

        wheelDriver.type = type;

        //TODO: Figure out mecanum friction.
        switch (friction)
        {
            case FrictionLevel.HIGH:
                wheelDriver.forwardExtremeSlip = 1; //Speed of max static friction force.
                wheelDriver.forwardExtremeValue = 10; //Force of max static friction force.
                wheelDriver.forwardAsympSlip = 1.5f; //Speed of leveled off kinetic friction force.
                wheelDriver.forwardAsympValue = 8;

                if (wheelDriver.type == WheelType.OMNI)
                {
                    wheelDriver.sideExtremeSlip = 1;
                    wheelDriver.sideExtremeValue = .01f;
                    wheelDriver.sideAsympSlip = 1.5f;
                    wheelDriver.sideAsympValue = .005f;
                }
                else
                {
                    wheelDriver.sideExtremeSlip = 1;
                    wheelDriver.sideExtremeValue = 10;
                    wheelDriver.sideAsympSlip = 1.5f;
                    wheelDriver.sideAsympValue = 8;
                }
                break;
            case FrictionLevel.MEDIUM:
                wheelDriver.forwardExtremeSlip = 1f; //Speed of max static friction force.
                wheelDriver.forwardExtremeValue = 7; //Force of max static friction force.
                wheelDriver.forwardAsympSlip = 1.5f; //Speed of leveled off kinetic friction force.
                wheelDriver.forwardAsympValue = 5;

                if (wheelDriver.type == WheelType.OMNI)
                {
                    wheelDriver.sideExtremeSlip = 1;
                    wheelDriver.sideExtremeValue = .01f;
                    wheelDriver.sideAsympSlip = 1.5f;
                    wheelDriver.sideAsympValue = .005f;
                }
                else
                {
                    wheelDriver.sideExtremeSlip = 1;
                    wheelDriver.sideExtremeValue = 7;
                    wheelDriver.sideAsympSlip = 1.5f;
                    wheelDriver.sideAsympValue = 5;
                }
                break;
            case FrictionLevel.LOW:
                wheelDriver.forwardExtremeSlip = 1; //Speed of max static friction force.
                wheelDriver.forwardExtremeValue = 5; //Force of max static friction force.
                wheelDriver.forwardAsympSlip = 1.5f; //Speed of leveled off kinetic friction force.
                wheelDriver.forwardAsympValue = 3;

                if (wheelDriver.type == WheelType.OMNI)
                {
                    wheelDriver.sideExtremeSlip = 1;
                    wheelDriver.sideExtremeValue = .01f;
                    wheelDriver.sideAsympSlip = 1.5f;
                    wheelDriver.sideAsympValue = .005f;
                }
                else
                {
                    wheelDriver.sideExtremeSlip = 1;
                    wheelDriver.sideExtremeValue = 5;
                    wheelDriver.sideAsympSlip = 1.5f;
                    wheelDriver.sideAsympValue = 3;
                }
                break;
        }

        FindRadiusThread.Reset();

        if (joint is RotationalJoint)
        {
            foreach (ComponentOccurrence component in ((RotationalJoint)joint).GetWrapped().childGroup.occurrences)
            {
                //Takes the part axes and the assembly axes and creates a transformation from one to the other.
                component.Transformation.GetCoordinateSystem(out origin, out partXAxis, out partYAxis, out partZAxis);

                asmToPart.SetToAlignCoordinateSystems(origin, partXAxis, partYAxis, partZAxis, origin, asmXAxis, asmYAxis, asmZAxis);

                //The joint normal is changed from being relative to assembly to relative to the part axes.
                transformedVector.Cell[1, 1] = ((RotationalJoint)joint).axis.x;
                transformedVector.Cell[2, 1] = ((RotationalJoint)joint).axis.y;
                transformedVector.Cell[3, 1] = ((RotationalJoint)joint).axis.z;

                Console.Write("Changing vector from " + transformedVector.Cell[1, 1] + ", " + transformedVector.Cell[2, 1] + ", " + transformedVector.Cell[3, 1]);

                transformedVector.TransformBy(asmToPart);

                rotationAxis.X = transformedVector.Cell[1, 1];
                rotationAxis.Y = transformedVector.Cell[2, 1];
                rotationAxis.Z = transformedVector.Cell[3, 1];

                Console.Write(" to " + transformedVector.Cell[1, 1] + ", " + transformedVector.Cell[2, 1] + ", " + transformedVector.Cell[3, 1] + ".\n");

                newRadiusThread = new FindRadiusThread(component, rotationAxis);
                radiusThreadList.Add(newRadiusThread);
                newRadiusThread.Start();
            }

            foreach(FindRadiusThread thread in radiusThreadList)
            {
                thread.Join();
            }

            wheelDriver.radius = (float)FindRadiusThread.GetRadius();
            treadPart = FindRadiusThread.GetWidthComponent();
            WheelAnalyzer.FindWheelWidthCenter(treadPart, rotationAxis, out maxWidth, out center);

            wheelDriver.width = (float)maxWidth;

            //The average vertex coordinates are changed to assembly space.
            treadPart.Transformation.GetCoordinateSystem(out origin, out partXAxis, out partYAxis, out partZAxis);

            asmToPart.SetToAlignCoordinateSystems(origin, asmXAxis, asmYAxis, asmZAxis, origin, partXAxis, partYAxis, partZAxis);

            center.TransformBy(asmToPart);

            wheelDriver.center.x = (float)(center.X + treadPart.Transformation.Translation.X);
            wheelDriver.center.y = (float)(center.Y + treadPart.Transformation.Translation.Y);
            wheelDriver.center.z = (float)(center.Z + treadPart.Transformation.Translation.Z);

            Console.WriteLine("Center of " + treadPart.Name + "found to be:");
            Console.WriteLine(wheelDriver.center.x + ", " + wheelDriver.center.y + ", " + wheelDriver.center.z);
            Console.WriteLine("Whilst the node coordinates are: ");
            Console.WriteLine(treadPart.Transformation.Translation.X + ", " + treadPart.Transformation.Translation.Y + ", "  + 
                treadPart.Transformation.Translation.Z);
        }

        joint.cDriver.AddInfo(wheelDriver);
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
    public static void FindWheelWidthCenter(ComponentOccurrence wheelTread, Vector rotationAxis, out double fullWidth, out Vector center)
    {
        const double MESH_TOLERANCE = 0.5;
        Inventor.Point tmp = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreatePoint();
        int vertexCount;
        int segmentCount;
        //Not sure how long to make arrays.  
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
        double maxWidth = 0.0;
        fullWidth = 0.0;
        center = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector(0, 0, 0);

        Console.WriteLine("Finding width and center of " + wheelTread.Name + ".");

        foreach (SurfaceBody surface in wheelTread.Definition.SurfaceBodies)
        {
            //TODO: use extending box or finding projection on plane.
            surface.CalculateStrokes(MESH_TOLERANCE, out vertexCount, out segmentCount, out verticeCoords, out verticeIndicies);
            for (int i = 0; i < verticeCoords.Length; i += 3)
            {
                center.X += verticeCoords[i];
                center.Y += verticeCoords[i + 1];
                center.Z += verticeCoords[i + 2];

                vertex.X = verticeCoords[i];
                vertex.Y = verticeCoords[i + 1];
                vertex.Z = verticeCoords[i + 2];

                newWidth = rotationAxis.DotProduct(vertex);

                //Stores the distance to the point. 
                if (newWidth > maxWidth)
                {
                    maxWidth = newWidth;
                }
                //Changes the starting point when detecting distance for later vertices.
                if (newWidth < minWidth)
                {
                    minWidth = newWidth;
                }

                //These two statements result in an end where the starting point is on one side of the wheel, and the distance that is being
                //      calculated and stored is for a vertex on the other side of the wheel.
            }

            fullWidth = maxWidth - minWidth;
            center.X = center.X / vertexCount;
            center.Y = center.Y / vertexCount;
            center.Z = center.Z / vertexCount;
        }

        Console.WriteLine("Found width and center of " + wheelTread.Name + ".");
    }


}

