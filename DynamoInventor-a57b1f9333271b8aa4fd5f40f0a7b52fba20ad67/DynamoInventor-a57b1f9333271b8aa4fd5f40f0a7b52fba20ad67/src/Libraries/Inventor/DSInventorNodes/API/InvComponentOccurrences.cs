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
    public class InvComponentOccurrences : IEnumerable<InvComponentOccurrence>
    {
        #region Internal properties
        List<InvComponentOccurrence> occurrenceList;

        internal Inventor.ComponentOccurrences InternalComponentOccurrences { get; set; }

        internal int InternalCount
        {
            get { return ComponentOccurrencesInstance.Count; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return ComponentOccurrencesInstance.Type.As<InvObjectTypeEnum>(); }
        }

        #endregion

        #region Private constructors
        private InvComponentOccurrences(InvComponentOccurrences invComponentOccurrences)
        {
            InternalComponentOccurrences = invComponentOccurrences.InternalComponentOccurrences;
            occurrenceList = new List<InvComponentOccurrence>();
            foreach (var occurrenceDef in InternalComponentOccurrences)
            {
                occurrenceList.Add(InvComponentOccurrence.ByInvComponentOccurrence((Inventor.ComponentOccurrence)occurrenceDef));
            }
        }

        private InvComponentOccurrences(Inventor.ComponentOccurrences invComponentOccurrences)
        {
            InternalComponentOccurrences = invComponentOccurrences;
            occurrenceList = new List<InvComponentOccurrence>();
            foreach (var occurrenceDef in InternalComponentOccurrences)
            {
                occurrenceList.Add(InvComponentOccurrence.ByInvComponentOccurrence((Inventor.ComponentOccurrence)occurrenceDef));
            }
        }
        #endregion

        #region Private methods
        #endregion

        #region Public properties
        public Inventor.ComponentOccurrences ComponentOccurrencesInstance
        {
            get { return InternalComponentOccurrences; }
            set { InternalComponentOccurrences = value; }
        }

        public int Count
        {
            get { return occurrenceList.Count; }
        }

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        #endregion

        #region Public static constructors
        public static InvComponentOccurrences ByInvComponentOccurrences(InvComponentOccurrences invComponentOccurrences)
        {
            return new InvComponentOccurrences(invComponentOccurrences);
        }
        public static InvComponentOccurrences ByInvComponentOccurrences(Inventor.ComponentOccurrences invComponentOccurrences)
        {
            return new InvComponentOccurrences(invComponentOccurrences);
        }
        #endregion

        #region Public methods
        #endregion

        public void Add(InvComponentOccurrence invComponentOccurrence)
        {
            occurrenceList.Add(invComponentOccurrence);
        }

        public IEnumerator<InvComponentOccurrence> GetEnumerator()
        {
            return occurrenceList.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
