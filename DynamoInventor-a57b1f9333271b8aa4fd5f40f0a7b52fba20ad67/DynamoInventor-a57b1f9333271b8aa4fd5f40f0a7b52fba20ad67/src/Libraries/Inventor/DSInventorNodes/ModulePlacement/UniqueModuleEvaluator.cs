using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Inventor;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using Dynamo.Models;
using Dynamo.Utilities;
using InventorLibrary.GeometryConversion;
using InventorServices.Persistence;
using InventorServices.Utilities;
using Point = Autodesk.DesignScript.Geometry.Point;


namespace InventorLibrary.ModulePlacement
{
    [Browsable(false)]
    [IsVisibleInDynamoLibrary(false)]
    public class UniqueModuleEvaluator : IDisposable
    {
        #region Private fields
        private TupleList<int, List<double>> allDistancesList = new TupleList<int, List<double>>();
        private TupleList<int, int> instanceGeometryMap = new TupleList<int, int>();
        private TupleList<int, List<double>> uniqueDistancesList = new TupleList<int, List<double>>();  
        private List<string> detailDocumentPaths = new List<string>();
        #endregion

        #region Internal properties
        internal TupleList<int, List<double>> AllModuleDistances
        {
            get { return allDistancesList; }
            set { allDistancesList = value; }
        }

        internal int ConstraintCount { get; set; }

        internal double ConstraintTolerance { get; set; }

        internal List<string> DetailDocumentPaths
        {
            get { return detailDocumentPaths; }
            set { detailDocumentPaths = value; }
        }

        internal List<Module> InternalModules { get; set; }

        internal TupleList<int, int> InstanceGeometryMap
        {
            get { return instanceGeometryMap; }
            set { instanceGeometryMap = value; }
        }

        internal TupleList<int, List<double>> UniqueModuleDistances
        {
            get { return uniqueDistancesList; }
            set { uniqueDistancesList = value; }
        }     
        #endregion

        #region Private constructors
        private UniqueModuleEvaluator(List<Module> modules, double tolerance)
        {
            InternalModules = modules;
            //This is sort of not safe if you look at this class by itself.  We know that
            //the Modules class has already evaluated whether or not the shape of the List<List<Point>> 
            //is uniform.
            ConstraintCount = InternalModules[0].InternalModulePoints.Count;
            ConstraintTolerance = tolerance;
            GenerateDistances();
            IdentifyDuplicates();
        }
        #endregion

        #region Private mutators
        private void GenerateDistances()
        {
            for (int i = 0; i < InternalModules.Count; i++)
            {
                List<double> tempDistances = new List<double>();

                //We are going to get the distance of Pn-Pn+1 and Plast-Pfirst. If addlCheck > 2,
                //we need to check Pn-Pn+2 for n < addlCheck+1.
                int addlCheck = 0;
                if (ConstraintCount > 3)
                {
                    addlCheck = InternalModules[i].InternalModulePoints.Count - 2;
                }
                for (int j = 0; j < ConstraintCount - 1; j++)
                {
                    double tempDist = DistanceToPoint(InternalModules[i].InternalModulePoints[j], InternalModules[i].InternalModulePoints[j + 1]);
                    tempDistances.Add(tempDist);
                }
                double firstToLastDist = DistanceToPoint(InternalModules[i].InternalModulePoints[ConstraintCount - 1], InternalModules[i].InternalModulePoints[0]);
                tempDistances.Add(firstToLastDist);

                //Add the additional check distances.
                for (int y = 0; y < addlCheck; y++)
                {
                    double tempAddlCheck = DistanceToPoint(InternalModules[i].InternalModulePoints[y], InternalModules[i].InternalModulePoints[y + 2]);
                    tempDistances.Add(tempAddlCheck);
                }

                allDistancesList.Add(i, tempDistances);
            }
        }

        private double DistanceToPoint(Point start, Point end)
        {
            double dx = start.X - end.X;
            double dy = start.Y - end.Y;
            double dz = start.Z - end.Z;
            double distance = System.Math.Sqrt(System.Math.Pow(dx, 2) + System.Math.Pow(dy, 2) + System.Math.Pow(dz, 2));
            return distance;
        }

        internal void IdentifyDuplicates()
        {
            //If the constraintCount is 3 or less, we can just check that the distance from point1-point2,
            //point2-point3, and point3-point1 are the same.  
            //If the constraintCount is greater than 3, we must check constraintCount-2 additional distances.
            //There is a more efficient way to do this but this is simple and it works.
            List<List<double>> tempDistList = new List<List<double>>();
            for (int i = 0; i < AllModuleDistances.Count; i++)
            {
                //The unique geometries list is empty, put the first one in it.
                if (i == 0)
                {
                    UniqueModuleDistances.Add(AllModuleDistances[i]);
                    InstanceGeometryMap.Add(i, i);
                    ////StringBuilder firstLine = new StringBuilder();
                    ////firstLine.Append(i.ToString() + ", " + i.ToString());
                    ////logFile.WriteLine(firstLine.ToString());
                }

                else
                {
                    //This delegate is for comparing to the specified constraint tolerance.
                    Func<double, double, double> currentTolerance = (double item1, double item2) => System.Math.Abs(item1 - item2);
                    int uniqueOuterCounter = 0;
                    for (int k = 0; k < UniqueModuleDistances.Count; k++)
                    {
                        int uniqueInnerCounter = 0;
                        for (int r = 0; r < AllModuleDistances[i].Item2.Count; r++)
                        {
                            double thisCurrentTolerance = currentTolerance(UniqueModuleDistances[k].Item2[r], AllModuleDistances[i].Item2[r]);
                            if (thisCurrentTolerance > ConstraintTolerance)
                            {
                                uniqueInnerCounter++;
                            }
                        }
                        //After looping through all the constraint sets, if there were any out of tolerance, then this geometry could be 
                        //unique.  This counter has to tick up for all geometries in the unique set for it to be added to the set.
                        if (uniqueInnerCounter != 0)
                        {
                            uniqueOuterCounter++;
                        }

                        if (uniqueInnerCounter == 0)
                        {
                            InstanceGeometryMap.Add(i, k);
                        }
                    }
                    //If this evaluates to true, then the current geometry did not match any of the geometry in the 
                    //unique list.
                    if (uniqueOuterCounter == UniqueModuleDistances.Count)
                    {
                        UniqueModuleDistances.Add(AllModuleDistances[i]);
                        InstanceGeometryMap.Add(i, UniqueModuleDistances.Count - 1);
                    }
                }
            }

            for (int t = 0; t < InternalModules.Count; t++)
            {
                //This sets the property on the Module class instance that identifies
                //which copied assembly should be used for its instance. 
                InternalModules[t].GeometryMapIndex = InstanceGeometryMap[t].Item2;
            }
        }
        #endregion

        #region Public properties
        public List<Module> Modules
        {
            get
            {
                return InternalModules;
            }
        }
        #endregion

        #region Public static constructors
        public static UniqueModuleEvaluator ByModules(List<Module> modules, double tolerance = .5)
        {
            return new UniqueModuleEvaluator(modules, tolerance);
        }
        #endregion

        #region Internal static constructors

        #endregion

        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                ////logFile.Close();
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
