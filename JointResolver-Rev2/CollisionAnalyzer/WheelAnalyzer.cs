using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;
using System.Threading;

class WheelAnalyzer
{
    public static void StartCalculations(RigidNode node)
    {
        const int NUMBER_OF_THREADS = 4; //The maximum number of threads finding radii at the same time.
        SkeletalJoint_Base joint = node.GetSkeletalJoint();
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
            List<ComponentOccurrence> sortedBoxList = new List<ComponentOccurrence>();

            foreach (ComponentOccurrence component in ((RotationalJoint)joint).GetWrapped().childGroup.occurrences)
            {
                sortComponentRadii(component, sortedBoxList);
            }

            Console.WriteLine("Printing sorted list");

            for(int i = 0; i < sortedBoxList.Count; i++)
            {
                Console.WriteLine(sortedBoxList[i].Name + " with box radius " + findBoxRadius(sortedBoxList[i]));

                radiusThreadList.Add(null); //Ensures there are the correct number of spaces for when we insert by index later.
            }

            int nextComponentIndex = 0; //The index of the next component of which to find the radius.
            int activeThreadCount = 0;  //Counts the number of started threads that have yet to complete.
            int largestRadiusIndex = -1; //-1 means it has not been found yet.  Stores the index after which it is pointless to try and find the radius.

            //Loops until it is impossible to find a larger radius in the remaining components.
            while (nextComponentIndex < sortedBoxList.Count && (activeThreadCount > 0 || largestRadiusIndex == -1))
            {
                List<FindRadiusThread> threadsToRemove = new List<FindRadiusThread>();

                for(int index = 0; index < nextComponentIndex; index++)
                {

                    if (radiusThreadList[index] != null)
                    {
                        //Ends threads that cannot have a larger radius than that already found.
                        if (!radiusThreadList[index].GetIsAlive())
                        {
                            activeThreadCount--;
                            threadsToRemove.Add(radiusThreadList[index]);
                        }

                        //If a thread has found a radius that would not fit in the bounding box of the next component, there's no point in continuing trying to find
                        //  a larger radius in the next component.
                        if (FindRadiusThread.GetRadius() > findBoxRadius(sortedBoxList[index+1]) && (index < largestRadiusIndex || largestRadiusIndex == -1))
                        {
                            largestRadiusIndex = index;
                        }

                        if (radiusThreadList[index].GetIsAlive() && index > largestRadiusIndex && largestRadiusIndex != -1)
                        {
                            activeThreadCount--;
                            radiusThreadList[index].endThread = true;
                        }
                    }
                }

                //Now that we're out of iterating through, we remove the threads that have ended.
                foreach(FindRadiusThread threadToRemove in threadsToRemove)
                {
                    radiusThreadList.Remove(threadToRemove);
                    radiusThreadList.Add(null);  //Keeps the index existing.
                }

                //Adds new threads when needed.
                while (activeThreadCount < NUMBER_OF_THREADS && nextComponentIndex < sortedBoxList.Count && largestRadiusIndex == -1)
                {
                    radiusThreadList[nextComponentIndex] = new FindRadiusThread(sortedBoxList[nextComponentIndex], ((RotationalJoint)joint).axis);
                    radiusThreadList[nextComponentIndex].Start();
                    nextComponentIndex++;
                    activeThreadCount++;
                }
            }

            //Waits for all remaining threads.
            for (int index = 0; index < nextComponentIndex; index++)
            {
                if (radiusThreadList[index] != null)
                {
                    radiusThreadList[index].Join();
                }
            }

            Console.WriteLine("Largest radius is " + FindRadiusThread.GetRadius());

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

    private static double findBoxRadius(ComponentOccurrence component)
    {
        return component.RangeBox.MinPoint.VectorTo(component.RangeBox.MaxPoint).Length / 2;
    }

    private static void sortComponentRadii(ComponentOccurrence component, List<ComponentOccurrence> sortedBoxList)
    {
        //Ignores assemblies and goes to parts.
        if (component.SubOccurrences.Count > 0)
        {
            foreach (ComponentOccurrence subComponent in component.SubOccurrences)
            {
                sortComponentRadii(subComponent, sortedBoxList);
            }
        }

        else
        {
            //Finds the correct spot to insert the component based on the magnitude of the diagonal of the bounding box.
            int listPosition;
            for (listPosition = 0; listPosition < sortedBoxList.Count
                && findBoxRadius(component) < findBoxRadius(sortedBoxList[listPosition]);
                listPosition++) ;

            sortedBoxList.Insert(listPosition, component);
        }        
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
            int vertexCount;
            int segmentCount;
            //Todo: change size of array.
            double[] vertexCoords = new double[3000];
            double[] vertexNormals = new double[3000];
            int[] vertexIndicies = new int[3000];

            //surface.CalculateStrokes(.01, out vertexCount, out segmentCount, out vertexCoords, out vertexIndicies);

            //surface.CalculateFacets(.01, out vertexCount, out segmentCount, out vertexCoords, out vertexNormals, out vertexIndicies);

            surface.GetExistingFacets(.01, out vertexCount, out segmentCount, out vertexCoords, out vertexNormals, out vertexIndicies);

            if (vertexCoords.Length == 1)
            {
                surface.CalculateFacets(.01, out vertexCount, out segmentCount, out vertexCoords, out vertexNormals, out vertexIndicies);
            }

            for(int i = 0; i < vertexCoords.Length; i+=3)
            {
                totalVertexCount++;

                vertexVector = Program.INVENTOR_APPLICATION.TransientGeometry.CreateVector(vertexCoords[i], vertexCoords[i + 1], vertexCoords[i+2]);

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
        Console.WriteLine("Center is at (" + center.X + ", " + center.Y + ", " + center.Z + ").");
    }
}

