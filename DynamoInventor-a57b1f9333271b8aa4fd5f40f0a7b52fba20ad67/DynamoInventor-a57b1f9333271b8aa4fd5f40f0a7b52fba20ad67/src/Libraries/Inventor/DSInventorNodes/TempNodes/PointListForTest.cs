using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
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
using Point = Autodesk.DesignScript.Geometry.Point;

namespace InventorLibrary.TestingHelpers
{
    //[RegisterForTrace]
    [IsVisibleInDynamoLibrary(false)]
    public class PointListForTest
    {
        #region Private fields
        private List<List<Point>> pointsList = new List<List<Point>>();
        #endregion

        #region Internal properties
        internal List<List<Point>> InternalPointsList
        {
            get { return pointsList; }
        }
        #endregion

        #region Private constructors
        private PointListForTest()
        {
            Point pt01 = Point.ByCoordinates(9576.501953125, 2030.5830078125, 1902.763671875);
            Point pt02 = Point.ByCoordinates(9489.2392578125, 2030.5830078125, 1902.763671875);
            Point pt03 = Point.ByCoordinates(9489.2392578125, 2156.5830078125, 1902.763671875);
            Point pt04 = Point.ByCoordinates(9576.501953125, 2156.5830078125, 1902.763671875);

            List<Point> pointList1 = new List<Point>() { pt01, pt02, pt03, pt04 };
            //List<Point> pointList1 = new List<Point>() { pt01, pt02, pt03 };
            
            Point pt05 = Point.ByCoordinates(8316.84765625, 2030.5830078125, 79.3935546875);
            Point pt06 = Point.ByCoordinates(8353.060546875, 2030.5830078125, 0);
            Point pt07 = Point.ByCoordinates(8353.060546875, 2156.5830078125, 0);
            Point pt08 = Point.ByCoordinates(8316.84765625, 2156.5830078125, 79.3935546875);

            List<Point> pointList2 = new List<Point>() { pt05, pt06, pt07, pt08 };

            Point pt09 = Point.ByCoordinates(8280.634765625, 2030.5830078125, 158.787109375);
            Point pt10 = Point.ByCoordinates(8316.84765625, 2030.5830078125, 79.3935546875);
            Point pt11 = Point.ByCoordinates(8316.84765625, 2156.5830078125, 79.3935546875);
            Point pt12 = Point.ByCoordinates(8280.634765625, 2156.5830078125, 158.787109375);

            List<Point> pointList3 = new List<Point>() { pt09, pt10, pt11, pt12 };

            InternalPointsList.Add(pointList1);
            InternalPointsList.Add(pointList2);
            InternalPointsList.Add(pointList3);
        }
   
        #endregion

        #region Private mutators

        #endregion

        #region Public properties
        public List<List<Point>> PointList
        {
            get { return InternalPointsList; }
            set { value = InternalPointsList; }
        }

        #endregion

        #region Public static constructors
        public static PointListForTest ByPointsFile ()
        {
            return new PointListForTest();
        }
        

        #endregion

        #region Internal static constructors

        #endregion

        #region Tesselation

        #endregion





    }
}