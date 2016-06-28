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
    public class InvComponentDefinitions : IEnumerable<InvComponentDefinition>
    {
        #region Internal properties
        List<InvComponentDefinition> compDefList;

        internal Inventor.ComponentDefinitions InternalComponentDefinitions { get; set; }

        internal int InternalCount
        {
            get { return compDefList.Count; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return ComponentDefinitionsInstance.Type.As<InvObjectTypeEnum>(); }
        }


        #endregion

        #region Private constructors
        private InvComponentDefinitions(InvComponentDefinitions invComponentDefinitions)
        {
            InternalComponentDefinitions = invComponentDefinitions.InternalComponentDefinitions;
            compDefList = new List<InvComponentDefinition>();
            foreach (var compDef in InternalComponentDefinitions)
            {
                compDefList.Add(InvComponentDefinition.ByInvComponentDefinition((Inventor.ComponentDefinition)compDef));
            }
        }

        private InvComponentDefinitions(Inventor.ComponentDefinitions invComponentDefinitions)
        {
            InternalComponentDefinitions = invComponentDefinitions;
            compDefList = new List<InvComponentDefinition>();
            foreach (var compDef in InternalComponentDefinitions)
            {
                compDefList.Add(InvComponentDefinition.ByInvComponentDefinition((Inventor.ComponentDefinition)compDef));
            }
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.ComponentDefinitions ComponentDefinitionsInstance
        {
            get { return InternalComponentDefinitions; }
            set { InternalComponentDefinitions = value; }
        }

        public int Count
        {
            get { return InternalCount; }
        }

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        #endregion

        #region Public static constructors
        public static InvComponentDefinitions ByInvComponentDefinitions(InvComponentDefinitions invComponentDefinitions)
        {
            return new InvComponentDefinitions(invComponentDefinitions);
        }
        public static InvComponentDefinitions ByInvComponentDefinitions(Inventor.ComponentDefinitions invComponentDefinitions)
        {
            return new InvComponentDefinitions(invComponentDefinitions);
        }
        #endregion

        #region Public methods
        #endregion

        public void Add(InvComponentDefinition invCompDef)
        {
            compDefList.Add(invCompDef);
        }

        public IEnumerator<InvComponentDefinition> GetEnumerator()
        {
            return compDefList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
