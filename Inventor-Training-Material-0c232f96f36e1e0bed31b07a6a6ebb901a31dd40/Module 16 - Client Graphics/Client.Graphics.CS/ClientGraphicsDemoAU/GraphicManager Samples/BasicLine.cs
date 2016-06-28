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
    // Description: Illustrates simple use of AdnClientGraphicsManager
    //   
    //////////////////////////////////////////////////////////////////////////////////////////////
    class BasicLine
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // Description: Displays a LineGraphics between points [0, 0, 0] and [1, 1, 1]
        //              using AdnClientGraphicsManager
        //////////////////////////////////////////////////////////////////////////////////////////////
        public static void Demo()
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            double[] startPoint = new double[] {0.0, 0.0, 0.0};
            double[] endPoint = new double[] {1.0, 1.0, 1.0};

            // Create instance of AdnClientGraphicsManager
            AdnClientGraphicsManager clientGraphicsMng = 
                new AdnClientGraphicsManager(InvApp, AdnInventorUtilities.AddInGuid);

            // Set document as current graphics source
            clientGraphicsMng.SetGraphicsSource(InvApp.ActiveDocument);

            // Create LineGraphics primitive
            LineGraphics lineGraph = clientGraphicsMng.DrawLine(startPoint, endPoint);
            lineGraph.LineWeight = 5.0;

            // Update view to see results
            clientGraphicsMng.UpdateView();
        }
    }
}
