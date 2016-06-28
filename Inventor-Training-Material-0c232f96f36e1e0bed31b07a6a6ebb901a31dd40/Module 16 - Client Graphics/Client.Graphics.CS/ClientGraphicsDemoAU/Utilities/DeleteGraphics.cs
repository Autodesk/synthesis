////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// Copyright (c) Autodesk, Inc. All rights reserved 
// Written by Philippe Leefsma 2011 - ADN/Developer Technical Services
//
// Permission to use, copy, modify, and distribute this software in
// object code form for any purpose and without fee is hereby granted, 
// provided that the above copyright notice appears in all copies and 
// that both that copyright notice and the limited warranty and
// restricted rights notice below appear in all supporting 
// documentation.
//
// AUTODESK PROVIDES THIS PROGRAM "AS IS" AND WITH ALL FAULTS. 
// AUTODESK SPECIFICALLY DISCLAIMS ANY IMPLIED WARRANTY OF
// MERCHANTABILITY OR FITNESS FOR A PARTICULAR USE.  AUTODESK, INC. 
// DOES NOT WARRANT THAT THE OPERATION OF THE PROGRAM WILL BE
// UNINTERRUPTED OR ERROR FREE.
////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using Inventor;
using Autodesk.ADN.Utility.Graphics;
using Autodesk.ADN.Utility.InventorUtils;

namespace ClientGraphicsDemoAU.RegularCGDemo
{
    //////////////////////////////////////////////////////////////////////////////////////////////
    // Description: Exposes two methods to delete ClientGraphics in a document
    //
    //              1/ DeleteAllGraphics will delete all the graphics without exception
    //
    //              2/ DeleteManagerGraphics will only delete graphics created 
    //                 by the AdnClientGraphicsManager
    //////////////////////////////////////////////////////////////////////////////////////////////
    class DeleteGraphics
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // Deletes all graphics (valid for parts and assembly documents only, and which doesn't 
        // contain ClientFeatures with graphics elements)
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        public static void DeleteAllGraphics()
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            Document document = InvApp.ActiveDocument;

            ClientGraphicsCollection cgCol = null;
            GraphicsDataSetsCollection dataSetsCol = null;

            switch (document.DocumentType)
            {
                case DocumentTypeEnum.kAssemblyDocumentObject:
                case DocumentTypeEnum.kPartDocumentObject:

                    ComponentDefinition compDef =
                         AdnInventorUtilities.GetCompDefinition(document);

                    //Not a Part or Assembly, return
                    if (compDef == null)
                        return;

                    cgCol = compDef.ClientGraphicsCollection;
                    dataSetsCol = document.GraphicsDataSetsCollection;

                    break;

                case DocumentTypeEnum.kDrawingDocumentObject:

                    DrawingDocument drawing = document as DrawingDocument;

                    cgCol = drawing.ActiveSheet.ClientGraphicsCollection;
                    dataSetsCol = drawing.ActiveSheet.GraphicsDataSetsCollection;

                    break;

                default:
                    return;
            }

            if(cgCol.Count != 0)
                foreach (ClientGraphics cg in cgCol)
                    cg.Delete();

            if (dataSetsCol.Count != 0)
                foreach (GraphicsDataSets dataSets in dataSetsCol)
                    dataSets.Delete();

            InvApp.ActiveView.Update();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Deletes all graphics created by ClientGraphicsManager
        // 
        //////////////////////////////////////////////////////////////////////////////////////////////
        public static void DeleteManagerGraphics()
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            Document document = InvApp.ActiveDocument;

            AdnClientGraphicsManager clientGraphicsMng = new AdnClientGraphicsManager(
               InvApp,
               AdnInventorUtilities.AddInGuid);

            switch (document.DocumentType)
            { 
                case DocumentTypeEnum.kAssemblyDocumentObject:
                case DocumentTypeEnum.kPartDocumentObject:

                    clientGraphicsMng.DeleteGraphics(document, true);
                    break;

                case DocumentTypeEnum.kDrawingDocumentObject:

                    DrawingDocument drawing = document as DrawingDocument;
                    clientGraphicsMng.DeleteGraphics(drawing.ActiveSheet, true);
                    break;

                default:
                    break;
            }

            clientGraphicsMng.UpdateView();
        }
    }
}
