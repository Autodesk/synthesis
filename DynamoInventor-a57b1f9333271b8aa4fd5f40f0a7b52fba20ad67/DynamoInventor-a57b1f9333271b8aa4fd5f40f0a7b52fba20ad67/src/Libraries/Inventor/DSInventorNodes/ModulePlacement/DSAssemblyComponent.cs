using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Inventor;

using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using DSNodeServices;
using Dynamo.Models;
using Dynamo.Utilities;
using DSInventorNodes.GeometryConversion;
using InventorServices.Persistence;
using InventorServices.Utilities;
using Point = Autodesk.DesignScript.Geometry.Point;

namespace DSInventorNodes.ModulePlacement
{
    [RegisterForTrace]
    public class DSAssemblyComponent
    {

        #region Internal properties
        internal WorkPoint InternalWorkPoint { get; private set; }

        //public override ComponentOccurrence InternalOccurrence
        //{
        //    get { return InternalWorkPoint.; }
        //}
        
        #endregion

        #region Private constructors
        private DSAssemblyComponent(List<List<Point>> points, 
                                    string targetAssembly, 
                                    string targetDrawing, 
                                    string targetDirectory)
        {
        }

        #endregion

        #region Private mutators

        #endregion

        #region Public properties

        #endregion

        #region Public static constructors
        public static DSAssemblyComponent ByPoints(List<List<Point>> points, 
                                                        string templateAssembly, 
                                                        string templateDrawing,  
                                                        string destinationDirectory = null)
        {
            return new DSAssemblyComponent(points, templateAssembly, templateDrawing, destinationDirectory);
        }

        #endregion

        #region Internal static constructors
        #endregion

        #region Tesselation

        #endregion
    
    }
}
