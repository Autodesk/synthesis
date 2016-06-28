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

namespace InventorLibrary.API
{
    [IsVisibleInDynamoLibrary(false)]
    public class InvDocuments
    {

        #region Internal properties
        internal Inventor.Documents InternalDocuments { get; set; }
        #endregion

        #region Private constructors
        private InvDocuments()
        {
            
        }

        private InvDocuments(InvDocuments documents )
        {
            InternalDocuments = documents.InternalDocuments;
        }

        private InvDocuments(Inventor.Documents documents)
        {
            InternalDocuments = documents;
        }
      
        #endregion

        #region Private mutators
        private InvAssemblyDocument InternalAddAssemblyDocument()
        {
            string assemblyTemplateFile = @"C:\Users\Public\Documents\Autodesk\Inventor 2013\Templates\Standard.iam";
            Inventor.Application invApp = (Inventor.Application)InventorServices.Persistence.PersistenceManager.InventorApplication;
            Inventor.AssemblyDocument assemblyDocument = (Inventor.AssemblyDocument)invApp.Documents.Add(DocumentTypeEnum.kAssemblyDocumentObject, assemblyTemplateFile, true);          
            return InvAssemblyDocument.ByInvAssemblyDocument(assemblyDocument);        
        }

        private InvDocument InternalAdd(InvDocumentTypeEnum documentTypeEnum, string templateFileName, bool createVisible)
        {
            Inventor.Application invApp = (Inventor.Application)InventorServices.Persistence.PersistenceManager.InventorApplication;
            Inventor.Document document = invApp.Documents.Add(InvDocumentTypeEnum.kAssemblyDocumentObject.As<DocumentTypeEnum>(), templateFileName, createVisible);
            return InvDocument.ByInvDocument(document);
        }
        #endregion

        #region Public properties
        public Inventor.Documents DocumentsInstance
        {
            get { return InternalDocuments; }
            set { InternalDocuments = value; }
        }
        #endregion

        #region Public static constructors
        public static InvDocuments ByInvDocuments()
        {
            return new InvDocuments();
        }

        public static InvDocuments ByInvDocuments(InvDocuments documents)
        {
            return new InvDocuments(documents);
        }

        public static InvDocuments ByInvDocuments(Inventor.Documents documents)
        {
            return new InvDocuments(documents);
        }
        #endregion

        #region Public methods

        public InvAssemblyDocument AddAssemblyDefaultTemplate()
        {
            return InternalAddAssemblyDocument();
        }

        public InvDocument Add(InvDocumentTypeEnum documentTypeEnum, string templateFileName, bool createVisible)
        {
            return InternalAdd(documentTypeEnum, templateFileName, createVisible);
        }


        #endregion

        #region Internal static constructors

        #endregion

    }
}
