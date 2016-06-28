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
using Point = Autodesk.DesignScript.Geometry.Point;
using SimpleInjector;

namespace InventorLibrary.WorkGeometry
{
    [IsVisibleInDynamoLibrary(true)]
    public class InvWorkPoint
    {
        private Point point;
        private IObjectBinder binder;

        [IsVisibleInDynamoLibrary(false)]
        private InvWorkPoint(Point point, IObjectBinder binder)
        {
            this.point = point;
            this.binder = binder;

            if (binder.DocumentManager.ActiveAssemblyDoc != null)
            {
                this.binder.ContextManager.BindingContextManager = binder.DocumentManager.ActiveAssemblyDoc.ReferenceKeyManager;
            }
            else if (binder.DocumentManager.ActivePartDoc != null)
            {
                this.binder.ContextManager.BindingContextManager = binder.DocumentManager.ActivePartDoc.ReferenceKeyManager;
            }
            this.binder.ContextManager.BindingContextManager = binder.DocumentManager.ActiveAssemblyDoc.ReferenceKeyManager;
            Inventor.WorkPoint wp;
            if (this.binder.GetObjectFromTrace<Inventor.WorkPoint>(out wp))
            {
                InternalWorkPoint = wp;
                AssemblyWorkPointDef wpDef = (AssemblyWorkPointDef)wp.Definition;
                wpDef.Point = point.ToPoint();
            }

            else
            {
                wp = binder.DocumentManager.ActiveAssemblyDoc.ComponentDefinition.WorkPoints.AddFixed(point.ToPoint(), false);
                InternalWorkPoint = wp;
                this.binder.SetObjectForTrace<InvWorkPoint>(this.InternalWorkPoint);
            }
        }

        #region Private fields
        #endregion

        #region Internal properties   
        internal Inventor.WorkPoint InternalWorkPoint { get; set; }
        #endregion

        #region Private constructors
        #endregion

        #region Private methods    
        #endregion

        #region Public static constructors
        public static InvWorkPoint ByPoint(Point point)
        {
            var binder = PersistenceManager.IoC.GetInstance<IObjectBinder>();
            return new InvWorkPoint(point, binder);
        }
        #endregion

        #region Public methods
        #endregion

        
    }
}
