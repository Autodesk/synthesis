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
using Autodesk.ADN.Utility.Interaction;
using Autodesk.ADN.Utility.InventorUtils;

namespace ClientGraphicsDemoAU.GraphicsManagerSamples
{
    //////////////////////////////////////////////////////////////////////////////////////////////
    // Description: Displays a LineStripGraphics between three vertices selected by the user 
    //              on the model.
    //////////////////////////////////////////////////////////////////////////////////////////////
    class LineStrip
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // The public static method to start the demo
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        public static void Demo()
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            object selection1 = InvApp.CommandManager.Pick(
                SelectionFilterEnum.kPartVertexFilter,
                "Select first vertex: ");

            if (selection1 == null)
                return;

            object selection2 = InvApp.CommandManager.Pick(
                SelectionFilterEnum.kPartVertexFilter,
                "Select second vertex: ");

            if (selection2 == null)
                return;

            object selection3 = InvApp.CommandManager.Pick(
                SelectionFilterEnum.kPartVertexFilter,
                "Select third vertex: ");

            if (selection3 == null)
                return;

            List<double> coords = new List<double>();

            coords.AddRange(AdnInventorUtilities.ToArray(AdnInventorUtilities.GetPoint(selection1)));
            coords.AddRange(AdnInventorUtilities.ToArray(AdnInventorUtilities.GetPoint(selection2)));
            coords.AddRange(AdnInventorUtilities.ToArray(AdnInventorUtilities.GetPoint(selection3)));

            AdnClientGraphicsManager clientGraphicsMng = new AdnClientGraphicsManager(
               InvApp,
               AdnInventorUtilities.AddInGuid);

            LineStripGraphics lineStrip = 
                clientGraphicsMng.DrawLineStrip(coords.ToArray());

            lineStrip.LineWeight = 5.0;

            int id = clientGraphicsMng.WorkingGraphics.GetDataSetFreeId();
            
            lineStrip.ColorSet =
                clientGraphicsMng.WorkingGraphics.GraphicsDataSets.CreateColorSet(id);

            lineStrip.ColorSet.Add(1, 119, 187, 17);

            clientGraphicsMng.UpdateView();
        }
    }
}
