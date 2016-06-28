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
    public class InvPrintManager
    {
        #region Internal properties
        internal Inventor.PrintManager InternalPrintManager { get; set; }

        internal Object InternalApplication
        {
            get { return PrintManagerInstance.Application; }
        }

        internal InvObjectTypeEnum InternalType
        {
            get { return PrintManagerInstance.Type.As<InvObjectTypeEnum>(); }
        }


        internal PrintColorModeEnum InternalColorMode { get; set; }

        internal int InternalNumberOfCopies { get; set; }

        internal PrintOrientationEnum InternalOrientation { get; set; }

        internal double InternalPaperHeight { get; set; }

        internal PaperSizeEnum InternalPaperSize { get; set; }

        internal int InternalPaperSource { get; set; }

        internal double InternalPaperWidth { get; set; }

        internal string InternalPrinter { get; set; }

        internal int InternalPrinterDeviceContext { get; set; }
        #endregion

        //internal PrintManager InternalkNoOwnership
        //{
        //    get { return Inventor.PrintManager.kNoOwnership; }
        //}
        //internal PrintManager InternalkSaveOwnership
        //{
        //    get { return Inventor.PrintManager.kSaveOwnership; }
        //}
        //internal PrintManager InternalkExclusiveOwnership
        //{
        //    get { return Inventor.PrintManager.kExclusiveOwnership; }
        //}
        #region Private constructors
        private InvPrintManager(InvPrintManager invPrintManager)
        {
            InternalPrintManager = invPrintManager.InternalPrintManager;
        }

        private InvPrintManager(Inventor.PrintManager invPrintManager)
        {
            InternalPrintManager = invPrintManager;
        }
        #endregion

        #region Private methods
        private void InternalPrintToFile(string fileName)
        {
            PrintManagerInstance.PrintToFile( fileName);
        }

        private void InternalSubmitPrint()
        {
            PrintManagerInstance.SubmitPrint();
        }

        #endregion

        #region Public properties
        public Inventor.PrintManager PrintManagerInstance
        {
            get { return InternalPrintManager; }
            set { InternalPrintManager = value; }
        }

        public Object Application
        {
            get { return InternalApplication; }
        }

        public InvObjectTypeEnum Type
        {
            get { return InternalType; }
        }

        //public InvPrintColorModeEnum ColorMode
        //{
        //    get { return InternalColorMode; }
        //    set { InternalColorMode = value; }
        //}

        //Fix this in generator
        //public Invint NumberOfCopies
        //{
        //    get { return InternalNumberOfCopies; }
        //    set { InternalNumberOfCopies = value; }
        //}

        //public InvPrintOrientationEnum Orientation
        //{
        //    get { return InternalOrientation; }
        //    set { InternalOrientation = value; }
        //}

        public double PaperHeight
        {
            get { return InternalPaperHeight; }
            set { InternalPaperHeight = value; }
        }

        public PaperSizeEnum PaperSize
        {
            get { return InternalPaperSize; }
            set { InternalPaperSize = value; }
        }

        public int PaperSource
        {
            get { return InternalPaperSource; }
            set { InternalPaperSource = value; }
        }

        public double PaperWidth
        {
            get { return InternalPaperWidth; }
            set { InternalPaperWidth = value; }
        }

        public string Printer
        {
            get { return InternalPrinter; }
            set { InternalPrinter = value; }
        }

        public int PrinterDeviceContext
        {
            get { return InternalPrinterDeviceContext; }
            set { InternalPrinterDeviceContext = value; }
        }

        #endregion
        //public PrintManager kNoOwnership
        //{
        //    get { return InternalkNoOwnership; }
        //}
        //public PrintManager kSaveOwnership
        //{
        //    get { return InternalkSaveOwnership; }
        //}
        //public PrintManager kExclusiveOwnership
        //{
        //    get { return InternalkExclusiveOwnership; }
        //}
        #region Public static constructors
        public static InvPrintManager ByInvPrintManager(InvPrintManager invPrintManager)
        {
            return new InvPrintManager(invPrintManager);
        }
        public static InvPrintManager ByInvPrintManager(Inventor.PrintManager invPrintManager)
        {
            return new InvPrintManager(invPrintManager);
        }
        #endregion

        #region Public methods
        public void PrintToFile(string fileName)
        {
            InternalPrintToFile( fileName);
        }

        public void SubmitPrint()
        {
            InternalSubmitPrint();
        }

        #endregion
    }
}
