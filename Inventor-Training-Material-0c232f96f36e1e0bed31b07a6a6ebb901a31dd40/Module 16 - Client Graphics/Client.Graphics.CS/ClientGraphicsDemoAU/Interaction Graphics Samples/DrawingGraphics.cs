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

namespace ClientGraphicsDemoAU.InteractionSamples
{
    //////////////////////////////////////////////////////////////////////////////////////////////////
    // Description: Illutrates use of ClientGraphics in Drawing Sheet, 
    //              using Interaction and MouseEvents to draw a moving symbol on the drawing.
    //
    //////////////////////////////////////////////////////////////////////////////////////////////////
    class DrawingGraphics
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // Class Members
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        Sheet _sheet;

        TransientGeometry _Tg;

        GraphicsNode _symbolNode;
        
        AdnInteractionManager _interactionManager;

        AdnClientGraphicsManager _clientGraphicsMng;

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Creates a new GraphicsNode that contains our graphic symbol 
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        GraphicsNode DrawSymbol(
            UnitVector normal, 
            Point center, 
            double radius)
        {
            Circle circle = _Tg.CreateCircle(center, normal, radius);

            double[] up = new double[]{center.X, center.Y + radius, 0};
            double[] down = new double[]{center.X, center.Y - radius, 0};
            double[] right = new double[]{center.X + radius, center.Y, 0};
            double[] left = new double[]{center.X - radius, center.Y, 0};

            GraphicsNode node = _clientGraphicsMng.CreateNewGraphicsNode();
            
            node.Selectable = true;

            CurveGraphics curve = _clientGraphicsMng.DrawCurve(circle, node);
            LineGraphics lineVert = _clientGraphicsMng.DrawLine(up, down, node);
            LineGraphics lineHorz = _clientGraphicsMng.DrawLine(left, right, node);

            return node;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Set position of a GraphicsNode
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void SetNodePosition(GraphicsNode node, Point position)
        {
            Matrix transfo = node.Transformation;

            Vector Tx = _Tg.CreateVector(
                position.X, position.Y, position.Z);

            transfo.SetTranslation(Tx, false);

            node.Transformation = transfo;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // The public Demo method
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        public static void Demo()
        {
            DrawingGraphics instance = new DrawingGraphics();
            instance.DoDemo();
        }

        void DoDemo()
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            if (InvApp.ActiveDocument == null || !(InvApp.ActiveDocument is DrawingDocument))
            {
                System.Windows.Forms.MessageBox.Show(
                    "A Drawing Document must be active...",
                    "Invalid Document Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);

                return;
            }

            _Tg = InvApp.TransientGeometry;

            _interactionManager = new AdnInteractionManager(InvApp);

            _interactionManager.Initialize();

            _interactionManager.OnTerminateEvent +=
               new AdnInteractionManager.OnTerminateHandler(OnTerminateEvent);

            _interactionManager.MouseEvents.MouseMoveEnabled = true;

            _interactionManager.MouseEvents.OnMouseDown +=
                new MouseEventsSink_OnMouseDownEventHandler(MouseEvents_OnMouseDown);

            _interactionManager.MouseEvents.OnMouseMove +=
                new MouseEventsSink_OnMouseMoveEventHandler(MouseEvents_OnMouseMove);

            _interactionManager.Start("Place Symbol: ");

            _clientGraphicsMng = new AdnClientGraphicsManager(
                AdnInventorUtilities.InvApplication,
                AdnInventorUtilities.AddInGuid);

            _clientGraphicsMng.Transacting = true;

            _clientGraphicsMng.SetGraphicsSource(
                _interactionManager.InteractionEvents);

            _symbolNode = null;

            DrawingDocument document = InvApp.ActiveDocument as DrawingDocument;

            _sheet = document.ActiveSheet;

            _symbolNode = null;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // OnMouseMove Event is used to transform  current GraphicsNode moved by the user
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void MouseEvents_OnMouseMove(
            MouseButtonEnum Button,
            ShiftStateEnum ShiftKeys,
            Inventor.Point ModelPosition,
            Point2d ViewPosition,
            Inventor.View View)
        {
            if (_symbolNode == null)
            {
                // Define symbol inputs: center, normal, radius
                Point center = _Tg.CreatePoint(0, 0, 0);

                UnitVector normal = _Tg.CreateUnitVector(0, 0, 1);

                double radius = _sheet.Width / 30;

                _symbolNode = DrawSymbol(normal, center, radius);
            }

            SetNodePosition(_symbolNode, ModelPosition);

            _clientGraphicsMng.UpdateView();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // OnMouseDown Event: we create symbol in the sheet graphics by using SetGraphicsSource(sheet)
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

            // Define symbol inputs: center, normal, radius

            Point center = ModelPosition;

            UnitVector normal = _Tg.CreateUnitVector(0, 0, 1);
            
            double radius = _sheet.Width / 30;

            // Add new symbol to the sheet graphics...
            _clientGraphicsMng.SetGraphicsSource(_sheet);

            DrawSymbol(normal, center, radius);

            _clientGraphicsMng.UpdateView();

            _clientGraphicsMng.SetGraphicsSource(
               _interactionManager.InteractionEvents);

            _symbolNode = null;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Trigger final view update when Interaction is terminated
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void OnTerminateEvent(object o, AdnInteractionManager.OnTerminateEventArgs e)
        {
            _clientGraphicsMng.UpdateView();
        }
    }
}
