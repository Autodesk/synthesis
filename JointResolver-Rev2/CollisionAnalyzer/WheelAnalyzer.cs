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
    /// Does all the calculations for the position/dimensions of a wheel.
    /// </summary>
    /// <param name="node">
    /// The node that has the wheel's joint.
    /// </param>
    public static void StartCalculations(RigidNode node)
    {
        const int MAX_THREAD_COUNT = 4; //The maximum number of threads finding radii at the same time.
        SkeletalJoint_Base joint = node.GetSkeletalJoint();
        List<FindRadiusThread> radiusThreadList = new List<FindRadiusThread>(); //Stores references to all the threads finding the radius of the rigid group.
        ComponentOccurrence treadPart = null; //The part of the wheel with the largest radius.
        double maxWidth = 0; //The width of the part of the wheel with the largest radius.
        BXDVector3 center; //The average center of all the vertex coordinates.
        WheelDriverMeta wheelDriver = new WheelDriverMeta(); //The info about the wheel attached to the joint.


        wheelDriver = joint.cDriver.GetInfo<WheelDriverMeta>();

        FindRadiusThread.Reset(); //Prepares the shared memeory for the next component.

        //Only need to worry about wheels if it is a rotational joint.
        if (wheelDriver != null && wheelDriver.type != WheelType.NOT_A_WHEEL && joint is RotationalJoint)
        {
            List<ComponentRadiusPair> sortedBoxList = new List<ComponentRadiusPair>();

            foreach (ComponentOccurrence component in ((RotationalJoint) joint).GetWrapped().childGroup.occurrences)
            {
                MakeMaxRadiusList(component, sortedBoxList);
            }

            //Sorts from large bounding box to small.
            sortedBoxList.Sort();

            Console.WriteLine("Printing sorted list");

            foreach (ComponentRadiusPair pair in sortedBoxList)
            {
                Console.WriteLine(pair.component.Name + " with box radius " + pair.possibleRadius);

                radiusThreadList.Add(null); //Ensures there are the correct number of spaces for when we insert by index later.
            }



            #region COMPUTE RADIUS
            {
                int nextComponentIndex = 0; //The index of the next component of which to find the radius.
                int activeThreadCount = 0;  //Counts the number of started threads that have yet to complete.
                int largestRadiusIndex = -1; //-1 means it has not been found yet.  Stores the index after which it is pointless to try and find the radius.

                //Loops until it is impossible to find a larger radius in the remaining components.
                while (nextComponentIndex < sortedBoxList.Count && (activeThreadCount > 0 || largestRadiusIndex == -1))
                {
                    List<FindRadiusThread> threadsToRemove = new List<FindRadiusThread>();

                    for (int index = 0; index < nextComponentIndex; index++)
                    {

                        if (radiusThreadList[index] != null)
                        {
                            //Ends threads that cannot have a larger radius than that already found.  Can't remove here since we're in a for loop working on the list.
                            if (!radiusThreadList[index].GetIsAlive())
                            {
                                activeThreadCount--;
                                threadsToRemove.Add(radiusThreadList[index]);
                            }


                            //If a thread has found a radius that would not fit in the bounding box of the next component, there's no point in continuing trying to find
                            //  a larger radius in the next component.
                            if (index + 1 < sortedBoxList.Count && (index < largestRadiusIndex || largestRadiusIndex == -1) && ((FindRadiusThread.GetRadius() > sortedBoxList[index + 1].possibleRadius)))
                            {
                                largestRadiusIndex = index;
                            }


                            //Closes active threads that are working on components that can't contain the largest radius.
                            if (radiusThreadList[index].GetIsAlive() && index > largestRadiusIndex && largestRadiusIndex != -1)
                            {
                                activeThreadCount--;
                                radiusThreadList[index].endThread = true;
                            }
                        }
                    }

                    //Now that we're out of iterating through, we can remove the threads that have ended.
                    foreach (FindRadiusThread threadToRemove in threadsToRemove)
                    {
                        radiusThreadList.Remove(threadToRemove);
                        radiusThreadList.Add(null);  //Keeps the index existing.
                    }

                    //Adds new threads when needed.
                    while (activeThreadCount < MAX_THREAD_COUNT && nextComponentIndex < sortedBoxList.Count && largestRadiusIndex == -1)
                    {
                        radiusThreadList[nextComponentIndex] = new FindRadiusThread(sortedBoxList[nextComponentIndex].component, ((RotationalJoint) joint).axis);
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
            }
            #endregion

            //Finds the width of the component with the largest radius.  This is usually the wheel's tread.
            treadPart = FindRadiusThread.GetWidthComponent();
            WheelAnalyzer.FindWheelWidthCenter(treadPart, ((RotationalJoint) joint).axis, out maxWidth, out center);

            //Saves calculated values to the driver.
            wheelDriver.radius = (float) FindRadiusThread.GetRadius();
            wheelDriver.width = (float) maxWidth;
            wheelDriver.center.x = (float) (center.x + treadPart.Transformation.Translation.X);
            wheelDriver.center.y = (float) (center.y + treadPart.Transformation.Translation.Y);
            wheelDriver.center.z = (float) (center.z + treadPart.Transformation.Translation.Z);

            Console.WriteLine("Center of " + treadPart.Name + "found to be:");
            Console.WriteLine(wheelDriver.center.x + ", " + wheelDriver.center.y + ", " + wheelDriver.center.z);
            Console.WriteLine("Whilst the node coordinates are: ");
            Console.WriteLine(treadPart.Transformation.Translation.X + ", " + treadPart.Transformation.Translation.Y + ", " +
                treadPart.Transformation.Translation.Z);
        }


        //Finally attaches the driver to the wheel's joint.
        joint.cDriver.AddInfo(wheelDriver);
    }

    /// <summary>
    /// Finds the distance between two opposite coners of a part's bounding box.
    /// </summary>
    /// <param name="component">
    /// </param>
    /// <returns></returns>
    private static double FindBoxRadius(ComponentOccurrence component)
    {
        return component.RangeBox.MinPoint.VectorTo(component.RangeBox.MaxPoint).Length / 2;
    }

    /// <summary>
    /// Maes a list containg every part and its maximum radius.
    /// </summary>
    /// <param name="component">
    /// The part to add/assembly to explode and add its parts.
    /// </param>
    /// <param name="sortedBoxList">
    /// The list to add to.
    /// </param>
    private static void MakeMaxRadiusList(ComponentOccurrence component, List<ComponentRadiusPair> sortedBoxList)
    {
        //Goes down to sub components if the component is an assembly.
        if (component.SubOccurrences.Count > 0)
        {
            foreach (ComponentOccurrence subComponent in component.SubOccurrences)
            {
                MakeMaxRadiusList(subComponent, sortedBoxList);
            }
        }

        else
        {
            //Finds the correct spot to insert the component based on the magnitude of the diagonal of the bounding box.
            sortedBoxList.Add(new ComponentRadiusPair(component));
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
    public static void FindWheelWidthCenter(ComponentOccurrence wheelTread, BXDVector3 rotationAxis, out double fullWidth, out BXDVector3 center)
    {
        double minWidth = float.PositiveInfinity; //The most negative width ever recorded.
        double maxWidth = float.NegativeInfinity; //The most positive width ever recorded.
        fullWidth = 0.0; //The difference between min and max widths. The actual width of the part.
        center = new BXDVector3(0, 0, 0); //The average coordinates of all the vertices.  Roughly the center.
        Matrix partAssemblyConversion = Exporter.INVENTOR_APPLICATION.TransientGeometry.CreateMatrix(); //The transformation between part and assembly axes.
        Matrix transformedVector = Exporter.INVENTOR_APPLICATION.TransientGeometry.CreateMatrix(); //Stores the rotation axis as it is transformed.
        Inventor.Point origin;
        Vector partXAxis; //The vectors for the part's coordinate axes.
        Vector partYAxis;
        Vector partZAxis;
        Vector asmXAxis = Exporter.INVENTOR_APPLICATION.TransientGeometry.CreateVector(1, 0, 0); //The vector for the assembly's coordinate axes.
        Vector asmYAxis = Exporter.INVENTOR_APPLICATION.TransientGeometry.CreateVector(0, 1, 0);
        Vector asmZAxis = Exporter.INVENTOR_APPLICATION.TransientGeometry.CreateVector(0, 0, 1);
        int vertexCount = 0;

        Console.WriteLine("Finding width and center of " + wheelTread.Name + ".");

        wheelTread.Transformation.GetCoordinateSystem(out origin, out partXAxis, out partYAxis, out partZAxis);

        partAssemblyConversion.SetToAlignCoordinateSystems(origin, partXAxis, partYAxis, partZAxis, origin, asmXAxis, asmYAxis, asmZAxis);

        //Need to store the axis in a matrix for the transformation.
        transformedVector.Cell[1, 1] = rotationAxis.x;
        transformedVector.Cell[2, 1] = rotationAxis.y;
        transformedVector.Cell[3, 1] = rotationAxis.z;

        Console.Write("Changing vector from " + rotationAxis);

        transformedVector.TransformBy(partAssemblyConversion);

        //Transormed so now we can compare the part's mesh's vertecie to it.
        BXDVector3 partSpaceAxis = new BXDVector3(transformedVector.Cell[1, 1], transformedVector.Cell[2, 1], transformedVector.Cell[3, 1]);

        Console.Write(" to " + partSpaceAxis + ".\n");

        foreach (SurfaceBody surface in wheelTread.Definition.SurfaceBodies)
        {
            double worstTolerance; //Stores the maximum distance between the part and its mesh.  Lower means it looks nicer/more polygons.
            #region EXISTING_TOLERANCES
            {
                int tmpToleranceCount;
                double[] tolerances = new double[10];

                surface.GetExistingFacetTolerances(out tmpToleranceCount, out tolerances);

                int worstIndex = -1;
                for (int i = 0; i < tmpToleranceCount; i++)
                {
                    //Finds the index of the worst resolution within acceptable bounds.
                    if ((worstIndex < 0 || tolerances[i] > tolerances[worstIndex]) && tolerances[i] < .1 && tolerances[i] > .01)
                    {
                        worstIndex = i;
                    }
                }

                //Stores the tolerance, defaults to .01 if no better.
                worstTolerance = (worstIndex == -1) ? .1 : tolerances[worstIndex];
            }
            #endregion

            int segmentCount;
            //Todo: change size of array.
            double[] vertexCoords = new double[3000];
            double[] vertexNormals = new double[3000];
            int[] vertexIndicies = new int[3000];

            //Uses existing mesh if possible...
            surface.GetExistingFacets(worstTolerance, out vertexCount, out segmentCount, out vertexCoords, out vertexNormals, out vertexIndicies);

            //...otherwise makes its own.
            if (vertexCount == 0)
            {
                surface.CalculateFacets(worstTolerance, out vertexCount, out segmentCount, out vertexCoords, out vertexNormals, out vertexIndicies);
            }

            //Iterates through three at a time, as the coordinates are ordered in groups of three.
            for (int i = 0; i < vertexCount * 3; i += 3)
            {
                BXDVector3 vertexVector = new BXDVector3((float) vertexCoords[i], (float) vertexCoords[i + 1], (float) vertexCoords[i + 2]);

                center.x += (float) vertexVector.x;
                center.y += (float) vertexVector.y;
                center.z += (float) vertexVector.z;

                double newWidth = BXDVector3.DotProduct(partSpaceAxis, vertexVector);

                maxWidth = Math.Max(maxWidth, newWidth);
                minWidth = Math.Min(minWidth, newWidth);
            }

        }

        fullWidth = maxWidth - minWidth;
        center.Multiply(1f / vertexCount); //Finds the average for all the vertex coordinates.

        Console.WriteLine("Center delta is now (" + center + ").");

        //Transform center back to assembly.
        partAssemblyConversion.Invert();

        transformedVector.Cell[1, 1] = center.x;
        transformedVector.Cell[2, 1] = center.y;
        transformedVector.Cell[3, 1] = center.z;

        transformedVector.TransformBy(partAssemblyConversion);

        center.x = (float) transformedVector.Cell[1, 1];
        center.y = (float) transformedVector.Cell[2, 1];
        center.z = (float) transformedVector.Cell[3, 1];

        Console.WriteLine("Found width and center of " + wheelTread.Name + ".");
        Console.WriteLine("Center delta after change of axis is now (" + center + ").");
    }
}

public class ComponentRadiusPair : IComparable<ComponentRadiusPair>
{
    public ComponentOccurrence component;
    public double possibleRadius;

    public ComponentRadiusPair(ComponentOccurrence passComponent, double passPossibleRadius)
    {
        component = passComponent;
        possibleRadius = passPossibleRadius;
    }

    public ComponentRadiusPair(ComponentOccurrence passComponent)
    {
        component = passComponent;

        possibleRadius = component.RangeBox.MinPoint.VectorTo(component.RangeBox.MaxPoint).Length / 2;
    }

    public int CompareTo(ComponentRadiusPair other)
    {
        return -this.possibleRadius.CompareTo(other.possibleRadius);
    }
}
