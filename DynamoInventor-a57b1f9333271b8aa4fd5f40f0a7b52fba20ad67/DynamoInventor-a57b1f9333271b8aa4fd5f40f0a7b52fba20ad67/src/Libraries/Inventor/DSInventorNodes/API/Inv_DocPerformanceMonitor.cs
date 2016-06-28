using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using Inventor;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Runtime;
using DSNodeServices;
using Dynamo.Models;
using Dynamo.Utilities;
using InventorLibrary.GeometryConversion;
using InventorServices.Persistence;

namespace InventorLibrary.API
{
    [IsVisibleInDynamoLibrary(false)]
    public class Inv_DocPerformanceMonitor
    {
        #region Internal properties
        internal Inventor._DocPerformanceMonitor Internal_DocPerformanceMonitor { get; set; }

        #endregion

        #region Private constructors
        private Inv_DocPerformanceMonitor(Inv_DocPerformanceMonitor inv_DocPerformanceMonitor)
        {
            Internal_DocPerformanceMonitor = inv_DocPerformanceMonitor.Internal_DocPerformanceMonitor;
        }

        private Inv_DocPerformanceMonitor(Inventor._DocPerformanceMonitor inv_DocPerformanceMonitor)
        {
            Internal_DocPerformanceMonitor = inv_DocPerformanceMonitor;
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor._DocPerformanceMonitor _DocPerformanceMonitorInstance
        {
            get { return Internal_DocPerformanceMonitor; }
            set { Internal_DocPerformanceMonitor = value; }
        }

        #endregion
        #region Public static constructors
        public static Inv_DocPerformanceMonitor ByInv_DocPerformanceMonitor(Inv_DocPerformanceMonitor inv_DocPerformanceMonitor)
        {
            return new Inv_DocPerformanceMonitor(inv_DocPerformanceMonitor);
        }
        public static Inv_DocPerformanceMonitor ByInv_DocPerformanceMonitor(Inventor._DocPerformanceMonitor inv_DocPerformanceMonitor)
        {
            return new Inv_DocPerformanceMonitor(inv_DocPerformanceMonitor);
        }
        #endregion

        #region Public methods
        #endregion
    }
}
