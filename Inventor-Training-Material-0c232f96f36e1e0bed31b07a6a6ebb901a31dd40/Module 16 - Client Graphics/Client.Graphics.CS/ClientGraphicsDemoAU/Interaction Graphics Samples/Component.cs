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
using Autodesk.ADN.Utility.WinUtils;
using Autodesk.ADN.Utility.Interaction;
using Autodesk.ADN.Utility.InventorUtils;

namespace ClientGraphicsDemoAU.InteractionSamples
{
    //////////////////////////////////////////////////////////////////////////////////////////////////
    // Description: A more advanced example illustrating the use of ComponentGraphics and MouseEvents.
    //              The user can select a part or assembly to insert inside the active part, 
    //              a Non-Parametric base feature is then created.
    //
    //////////////////////////////////////////////////////////////////////////////////////////////////
    class Component
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // Class Members
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        AdnClientGraphicsManager _clientGraphicsMng;

        AdnInteractionManager _interactionManager;

        ComponentGraphics _compGraph;

        Document _componentDocument;

        //////////////////////////////////////////////////////////////////////////////////////////////
        // The public static method to start the demo
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        public static void Demo()
        {
            Component instance = new Component();
            instance.DoDemo();
        }

        void DoDemo()
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            string filename =
                AdnInventorUtilities.ShowOpenDialog(
                "Select Inventor file",
                "Inventor Files (*.ipt; *.iam)|*.ipt;*.iam");

            if (filename == string.Empty)
                return;

            _componentDocument = InvApp.Documents.Open(filename, false);

            ComponentDefinition compDef =
                 AdnInventorUtilities.GetCompDefinition(_componentDocument);

            _interactionManager = new AdnInteractionManager(InvApp);

            _interactionManager.Initialize();

            _interactionManager.OnTerminateEvent +=
               new AdnInteractionManager.OnTerminateHandler(OnTerminateEvent);

            _interactionManager.MouseEvents.MouseMoveEnabled = true;

            _interactionManager.MouseEvents.OnMouseDown +=
                new MouseEventsSink_OnMouseDownEventHandler(MouseEvents_OnMouseDown);

            _interactionManager.MouseEvents.OnMouseMove +=
                new MouseEventsSink_OnMouseMoveEventHandler(MouseEvents_OnMouseMove);

            _interactionManager.Start("Place Component: ");

            _clientGraphicsMng = new AdnClientGraphicsManager(
                AdnInventorUtilities.InvApplication,
                AdnInventorUtilities.AddInGuid);

            _clientGraphicsMng.SetGraphicsSource(
                _interactionManager.InteractionEvents);

            _compGraph = _clientGraphicsMng.DrawComponent(compDef);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // 
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void MouseEvents_OnMouseDown(
            MouseButtonEnum Button,
            ShiftStateEnum ShiftKeys,
            Inventor.Point ModelPosition,
            Point2d ViewPosition,
            Inventor.View View)
        {
            if (Button != MouseButtonEnum.kLeftMouseButton)
                return;

            InsertAsSurfaceGraphics(_compGraph.Parent.Transformation);

            _clientGraphicsMng.SetGraphicsSource(
               _interactionManager.InteractionEvents);

            ComponentDefinition compDef =
                AdnInventorUtilities.GetCompDefinition(_componentDocument);

            _compGraph = _clientGraphicsMng.DrawComponent(compDef);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Insert Component as ComponentGraphics
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void InsertAsComponentGraphics(Matrix transfo)
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            _clientGraphicsMng.SetGraphicsSource(InvApp.ActiveDocument);

            ComponentDefinition compDef =
               AdnInventorUtilities.GetCompDefinition(_componentDocument);

            _compGraph = _clientGraphicsMng.DrawComponent(compDef);

            _compGraph.Parent.Transformation = transfo;
            _compGraph.Parent.Selectable = true;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // 
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void InsertAsSurfaceGraphics(Matrix transfo)
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            Document document = InvApp.ActiveDocument;

            ComponentDefinition compDef =
                AdnInventorUtilities.GetCompDefinition(document);

            _clientGraphicsMng.SetGraphicsSource(InvApp.ActiveDocument);

            Dictionary<SurfaceBody, SurfaceBody> surfaceBodies =
                AdnInventorUtilities.GetTransientBodies(
                    AdnInventorUtilities.GetCompDefinition(_componentDocument));

            foreach (KeyValuePair<SurfaceBody, SurfaceBody> pair in surfaceBodies)
            {
                SurfaceGraphics surfGraph = _clientGraphicsMng.DrawSurface(pair.Value);

                GraphicsNode node = surfGraph.Parent;

                node.Transformation = transfo;
                node.Selectable = true;

                StyleSourceTypeEnum styleSourceType;
                node.RenderStyle = pair.Key.GetRenderStyle(out styleSourceType);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // 
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void MouseEvents_OnMouseMove(
            MouseButtonEnum Button,
            ShiftStateEnum ShiftKeys,
            Inventor.Point ModelPosition,
            Point2d ViewPosition,
            Inventor.View View)
        {
            Matrix transfo = _compGraph.Parent.Transformation;

            Vector Tx = AdnInventorUtilities.InvApplication.TransientGeometry.CreateVector(
                ModelPosition.X, ModelPosition.Y, ModelPosition.Z);

            transfo.SetTranslation(Tx, false);

            _compGraph.Parent.Transformation = transfo;

            _clientGraphicsMng.UpdateView();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Trigger final view update when Interaction is terminated
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void OnTerminateEvent(object o, AdnInteractionManager.OnTerminateEventArgs e)
        {
            _clientGraphicsMng.UpdateView();

            _componentDocument.Close(true);
        }
    }
}
