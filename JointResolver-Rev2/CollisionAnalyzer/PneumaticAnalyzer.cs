using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;
using System.Threading;

class PneumaticAnalyzer
{

    public static void StartCalculations(RigidNode node)
    {
        SkeletalJoint_Base joint = node.GetSkeletalJoint();
        FindRadiusThread newRadiusThread;
        List<FindRadiusThread> radiusThreadList = new List<FindRadiusThread>(); //Stores references to all the threads finding the radius of the rigid group.
        ComponentOccurrence treadPart = null; //The part of the wheel with the largest radius.
        double maxWidth = 0; //The width of the part of the wheel with the largest radius.
        Vector center; //The average center of all the vertex coordinates.
        Matrix invertedTransform; //Stores the transfrom from part axes to assembly axes.
        WheelDriverMeta wheelDriver = new WheelDriverMeta(); //The info about the wheel attached to the joint.

        wheelDriver = joint.cDriver.GetInfo<WheelDriverMeta>();

        FindRadiusThread.Reset(); //Prepares the shared memeory for the next component.

        //Only need to worry about wheels if it is a rotational joint.
        if (joint is RotationalJoint)
        {
            foreach (ComponentOccurrence component in ((RotationalJoint)joint).GetWrapped().childGroup.occurrences)
            {
                newRadiusThread = new FindRadiusThread(component, ((RotationalJoint)joint).axis);
                radiusThreadList.Add(newRadiusThread);
                newRadiusThread.Start();
            }

            //Waits for all radii to be found before operating on the final values.
            foreach (FindRadiusThread thread in radiusThreadList)
            {
                thread.Join();
            }

            //Finds width.
            treadPart = FindRadiusThread.GetWidthComponent();
            WheelAnalyzer.FindWheelWidthCenter(treadPart, ((RotationalJoint)joint).axis, out maxWidth, out center);

            invertedTransform = treadPart.Transformation;
            invertedTransform.Invert();

            center.TransformBy(invertedTransform);

            //Beings saving calculated values to the driver.
            wheelDriver.radius = (float)FindRadiusThread.GetRadius();
            wheelDriver.width = (float)maxWidth;
            wheelDriver.center.x = (float)(center.X + treadPart.Transformation.Translation.X);
            wheelDriver.center.y = (float)(center.Y + treadPart.Transformation.Translation.Y);
            wheelDriver.center.z = (float)(center.Z + treadPart.Transformation.Translation.Z);

            Console.WriteLine("Center of " + treadPart.Name + "found to be:");
            Console.WriteLine(wheelDriver.center.x + ", " + wheelDriver.center.y + ", " + wheelDriver.center.z);
            Console.WriteLine("Whilst the node coordinates are: ");
            Console.WriteLine(treadPart.Transformation.Translation.X + ", " + treadPart.Transformation.Translation.Y + ", " +
                treadPart.Transformation.Translation.Z);
        }


        //Finally saves the bundle of info to the driver attached to the wheel.
        joint.cDriver.AddInfo(wheelDriver);
    }


    /// <summary>
    /// Saves all of the informations for a wheel collider, such as width, radius, and center, to a joint.
    /// </summary>
    /// <param name="diameter">
    /// The position of the wheel on the robot's frame.  Will most likely be removed later.
    /// </param>
    /// <param name="pressure">
    /// The joint that controls the collider.
    /// </param>
    public static void SaveToPneumaticJoint(JointDriverType driverType, PneumaticDiameter diameter, PneumaticPressure pressure, RigidNode node)
    {
        SkeletalJoint_Base joint = node.GetSkeletalJoint();
        PneumaticDriverMeta pneumaticDriver = new PneumaticDriverMeta(); //The info about the wheel attached to the joint.
        RigidNode.DeferredCalculation newCalculation;

        pneumaticDriver.type = driverType;

        //TODO: Find real values that make sense for the friction.  Also add Mecanum wheels.
        switch (diameter)
        {
            case PneumaticDiameter.HIGH:
                pneumaticDriver.widthMM = 10;
                break;
            case PneumaticDiameter.MEDIUM:
                pneumaticDriver.widthMM = 5;
                break;
            case PneumaticDiameter.LOW:
                pneumaticDriver.widthMM = 1;
                break;
        }

        switch (pressure)
        {
            case PneumaticPressure.HIGH:
                pneumaticDriver.pressurePSI = 10;
                break;
            case PneumaticPressure.MEDIUM:
                pneumaticDriver.pressurePSI = 5;
                break;
            case PneumaticPressure.LOW:
                pneumaticDriver.pressurePSI = 1;
                break;
        }

        joint.cDriver.AddInfo(pneumaticDriver);

        newCalculation = StartCalculations;
        node.RegisterDeferredCalculation(node.GetModelID(), newCalculation);
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
    public static void FindWheelWidthCenter(ComponentOccurrence wheelTread, BXDVector3 rotationAxis, out double fullWidth, out Vector center)
    {
        double newWidth; //The distance from the origin to the latest vertex.
        double minWidth = 0.0; //The lowest newWidth ever recorded.
        double maxWidth = 0.0; //The highest newWidth ever recorded.
        fullWidth = 0.0; //The difference between min and max widths. The actual width of the part.
        center = ((Inventor.Application)System.Runtime.InteropServices.Marshal.
            GetActiveObject("Inventor.Application")).TransientGeometry.CreateVector(0, 0, 0); //The average coordinates of all the vertices.  Roughly the center.
        Vector myRotationAxis = Program.INVENTOR_APPLICATION.TransientGeometry.CreateVector(); //The axis of rotation relative to the part axes.
        Matrix asmToPart = Program.INVENTOR_APPLICATION.TransientGeometry.CreateMatrix(); //The transformation from assembly axes to part axes.
        Matrix transformedVector = Program.INVENTOR_APPLICATION.TransientGeometry.CreateMatrix(); //Stores the rotation axis as it is transformed.
        Inventor.Point origin;
        Vector partXAxis;
        Vector partYAxis;
        Vector partZAxis;
        Vector asmXAxis = Program.INVENTOR_APPLICATION.TransientGeometry.CreateVector(1, 0, 0);
        Vector asmYAxis = Program.INVENTOR_APPLICATION.TransientGeometry.CreateVector(0, 1, 0);
        Vector asmZAxis = Program.INVENTOR_APPLICATION.TransientGeometry.CreateVector(0, 0, 1);
        Vector vertexVector;
        int totalVertexCount = 0;

        Console.WriteLine("Finding width and center of " + wheelTread.Name + ".");

        wheelTread.Transformation.GetCoordinateSystem(out origin, out partXAxis, out partYAxis, out partZAxis);

        asmToPart.SetToAlignCoordinateSystems(origin, partXAxis, partYAxis, partZAxis, origin, asmXAxis, asmYAxis, asmZAxis);

        //The joint normal is changed from being relative to assembly to relative to the part axes.
        transformedVector.Cell[1, 1] = rotationAxis.x;
        transformedVector.Cell[2, 1] = rotationAxis.y;
        transformedVector.Cell[3, 1] = rotationAxis.z;

        Console.Write("Changing vector from " + transformedVector.Cell[1, 1] + ", " + transformedVector.Cell[2, 1] + ", " + transformedVector.Cell[3, 1]);

        transformedVector.TransformBy(asmToPart);

        myRotationAxis.X = transformedVector.Cell[1, 1];
        myRotationAxis.Y = transformedVector.Cell[2, 1];
        myRotationAxis.Z = transformedVector.Cell[3, 1];

        Console.Write(" to " + transformedVector.Cell[1, 1] + ", " + transformedVector.Cell[2, 1] + ", " + transformedVector.Cell[3, 1] + ".\n");

        foreach (SurfaceBody surface in wheelTread.Definition.SurfaceBodies)
        {
            totalVertexCount += surface.Vertices.Count;

            foreach (Vertex vertex in surface.Vertices)
            {
                vertexVector = Program.INVENTOR_APPLICATION.TransientGeometry.CreateVector(vertex.Point.X, vertex.Point.Y, vertex.Point.Z);

                center.X += vertexVector.X;
                center.Y += vertexVector.Y;
                center.Z += vertexVector.Z;

                newWidth = myRotationAxis.DotProduct(vertexVector);

                //Stores the distance to the point. 
                if (newWidth > maxWidth)
                {
                    maxWidth = newWidth;

                    if (minWidth == 0.0)
                    {
                        minWidth = newWidth;
                    }
                }
                //Changes the starting point when detecting distance for later vertices.
                if (newWidth < minWidth)
                {
                    minWidth = newWidth;

                    if (maxWidth == 0.0)
                    {
                        maxWidth = newWidth;
                    }
                }
            }

        }

        fullWidth = maxWidth - minWidth;
        center.X = center.X / totalVertexCount; //Finds the average for all the vertex coordinates.
        center.Y = center.Y / totalVertexCount;
        center.Z = center.Z / totalVertexCount;

        Console.WriteLine("Found width and center of " + wheelTread.Name + ".");
    }
}