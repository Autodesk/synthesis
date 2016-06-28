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
using System.Linq;
using System.Text;

using Inventor;
using Autodesk.ADN.Utility.WinUtils;
using Autodesk.ADN.Utility.Graphics;
using Autodesk.ADN.Utility.InventorUtils;

namespace ClientGraphicsDemoAU.GraphicsManagerSamples
{
    //////////////////////////////////////////////////////////////////////////////////////////////
    // Description: Displays a TriangleGraphics attached to a ClientFeature. 
    //              The graphics stored inside the ClientFeature are persisted across sessions.
    //////////////////////////////////////////////////////////////////////////////////////////////
    class ClientFeatureGraphics
    {

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Simple example using Inventor API directly
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        public static void ClientFeatureDemo()
        {
            string clientId = AdnInventorUtilities.AddInGuid;

            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            Document document = InvApp.ActiveDocument;

            // We will use late binding to retrieve ClientFeatures collection,
            // so we dont need to write specific code for PartDocument and 
            // AssemblyDocument
            ComponentDefinition compDef =
                AdnInventorUtilities.GetCompDefinition(document);

            object features =
                 AdnInventorUtilities.GetProperty(compDef, "Features");

            ClientFeatures clientFeatures =
                 AdnInventorUtilities.GetProperty(features, "ClientFeatures")
                    as ClientFeatures;

            ClientFeatureDefinition cfDef = 
                clientFeatures.CreateDefinition("Graphics Feature", null, null, null);
            
            ClientFeature clientFeature =
                clientFeatures.Add(cfDef, clientId);

            NativeBrowserNodeDefinition nodeDef =
                clientFeature.BrowserNode.BrowserNodeDefinition as NativeBrowserNodeDefinition;

            stdole.IPictureDisp pic = 
                PictureDispConverter.ToIPictureDisp(Resources.PointImage);

            ClientNodeResource res =
                document.BrowserPanes.ClientNodeResources.Add(
                    clientId, 
                    document.BrowserPanes.ClientNodeResources.Count + 1, 
                    pic);

            nodeDef.OverrideIcon = res;

            cfDef = clientFeature.Definition;

            cfDef.HighlightClientGraphicsWithFeature = true;

            GraphicsDataSets sets =
                cfDef.GraphicsDataSetsCollection.Add2(clientId, true);

            GraphicsCoordinateSet coordSet = sets.CreateCoordinateSet(1);

            double[] coords = new double[]
            {
                0.0, 0.0, 0.0,
                5.0, 0.0, 0.0,
                2.5, 5.0, 0.0
            };

            coordSet.PutCoordinates(ref coords);

            ClientGraphics cg = 
                cfDef.ClientGraphicsCollection.Add(clientId);

            GraphicsNode node = cg.AddNode(1);

            node.RenderStyle = document.RenderStyles["Green (Flat)"];

            TriangleGraphics primitive = node.AddTriangleGraphics();

            primitive.CoordinateSet = coordSet;

            InvApp.ActiveView.Update();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Example using the AdnClientGraphicsManager
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        public static void DemoMng()
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            Document document = InvApp.ActiveDocument;

            // We will use late binding to retrieve ClientFeatures collection,
            // so we dont need to write specific code for PartDocument and 
            // AssemblyDocument
            ComponentDefinition compDef =
                AdnInventorUtilities.GetCompDefinition(document);
               
            object features =
                 AdnInventorUtilities.GetProperty(compDef, "Features");

            ClientFeatures clientFeatures =
                 AdnInventorUtilities.GetProperty(features, "ClientFeatures")
                    as ClientFeatures;

            ClientFeatureDefinition cfDef = 
                clientFeatures.CreateDefinition("Graphics Feature", null, null, null);

            ClientFeature clientFeature = 
                clientFeatures.Add(cfDef, AdnInventorUtilities.AddInGuid);

            cfDef = clientFeature.Definition;

            cfDef.HighlightClientGraphicsWithFeature = true;

            NativeBrowserNodeDefinition nodeDef =
               clientFeature.BrowserNode.BrowserNodeDefinition as NativeBrowserNodeDefinition;

            stdole.IPictureDisp pic =
                PictureDispConverter.ToIPictureDisp(Resources.PointImage);

            ClientNodeResource res =
                document.BrowserPanes.ClientNodeResources.Add(
                    AdnInventorUtilities.AddInGuid,
                    document.BrowserPanes.ClientNodeResources.Count + 1,
                    pic);

            nodeDef.OverrideIcon = res;

            AdnClientGraphicsManager clientGraphicsMng = new AdnClientGraphicsManager(
               InvApp,
               AdnInventorUtilities.AddInGuid);

            clientGraphicsMng.SetGraphicsSource(clientFeature);

            Random rd = new Random();

            TriangleGraphics graphics = clientGraphicsMng.DrawTriangle(
                new double[] { rd.Next(0, 10), rd.Next(0, 10), rd.Next(0, 10) },
                new double[] { rd.Next(0, 10), rd.Next(0, 10), rd.Next(0, 10) },
                new double[] { rd.Next(0, 10), rd.Next(0, 10), rd.Next(0, 10) },
                null);

            int id = clientGraphicsMng.WorkingGraphics.GetDataSetFreeId();

            GraphicsColorSet colorSet = 
                clientGraphicsMng.WorkingGraphics.GraphicsDataSets.CreateColorSet(id);

            colorSet.Add(1, (byte)rd.Next(0, 255), (byte)rd.Next(0, 255), (byte)rd.Next(0, 255));

            graphics.ColorSet = colorSet;

            clientGraphicsMng.UpdateView();
        }
    }
}
