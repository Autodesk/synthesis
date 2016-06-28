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
using System;
using System.Collections.Generic;
using System.Text;

using Inventor;
using Autodesk.ADN.Utility.Graphics;
using Autodesk.ADN.Utility.InventorUtils;

namespace ClientGraphicsDemoAU.GraphicsManagerSamples
{
    //////////////////////////////////////////////////////////////////////////////////////////////
    // Description: Displays a CurveGraphics based on edges form surface body selected by user.
    //
    //////////////////////////////////////////////////////////////////////////////////////////////
    class CurveGraphic
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // The public static method to start the demo
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        public static void Demo()
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            object selection = InvApp.CommandManager.Pick(
                SelectionFilterEnum.kPartBodyFilter,
                "Select a SurfaceBody: ");

            if (selection == null)
                return;

            SurfaceBody body = selection as SurfaceBody;

            AdnClientGraphicsManager clientGraphicsMng = new AdnClientGraphicsManager(
               InvApp,
               AdnInventorUtilities.AddInGuid);

            GraphicsNode node = clientGraphicsMng.CreateNewGraphicsNode();

            foreach (Edge edge in body.Edges)
            {
                CurveGraphics graphic = clientGraphicsMng.DrawCurve(edge.Geometry, node);

                graphic.LineWeight = 5.0;
                graphic.Parent.RenderStyle = InvApp.ActiveDocument.RenderStyles["Red"];
            }

            clientGraphicsMng.UpdateView();
        }
    }
}
