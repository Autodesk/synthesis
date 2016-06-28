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

namespace ClientGraphicsDemoAU.InteractionSamples
{
    //////////////////////////////////////////////////////////////////////////////////////////////////
    // Description: A simple interaction example using a TriangleGraphics and MouseEvents.
    //
    //////////////////////////////////////////////////////////////////////////////////////////////////
    class SimpleInteraction
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // Class Members
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        InteractionEvents _interactionEvents;

        MouseEvents _mouseEvents;

        TriangleGraphics _triangleGraph = null;

        //////////////////////////////////////////////////////////////////////////////////////////////
        // The public static method to start the demo
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        public static void Demo()
        {
            SimpleInteraction instance = new SimpleInteraction();
            instance.DoDemo();
        }

        void DoDemo()
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            //Create new interaction event
            _interactionEvents = InvApp.CommandManager.CreateInteractionEvents();

            //Store mouse events
            _mouseEvents = _interactionEvents.MouseEvents;

            //Enable mouse move. This is disabled by default for performance reasons
            _mouseEvents.MouseMoveEnabled = true;

            //Listen to OnMouseMove event
            _mouseEvents.OnMouseMove +=
                new MouseEventsSink_OnMouseMoveEventHandler(MouseEvents_OnMouseMove);

            _interactionEvents.StatusBarText = "Select triangle vertex: ";

            //Retrieve InteractionGraphics
            InteractionGraphics ig = _interactionEvents.InteractionGraphics;

            //Create new node
            GraphicsNode node = ig.OverlayClientGraphics.AddNode(1);

            //Add Triangle primitive
            _triangleGraph = node.AddTriangleGraphics();

            //Set up coordinates
            GraphicsCoordinateSet coordSet = ig.GraphicsDataSets.CreateCoordinateSet(1);

            double[] coords = new double[]
            {
                0.0, 0.0, 0.0, //vertex 1
                5.0, 0.0, 0.0, //vertex 2
                2.5, 5.0, 0.0  //vertex 3
            };

            coordSet.PutCoordinates(ref coords);

            _triangleGraph.CoordinateSet = coordSet;

            _interactionEvents.Start();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // OnMouseMove event we update the 3rd vertex of our triangle graphics with current position
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void MouseEvents_OnMouseMove(
            MouseButtonEnum Button,
            ShiftStateEnum ShiftKeys,
            Inventor.Point ModelPosition,
            Point2d ViewPosition,
            Inventor.View View)
        {
            //Retrieve coordinates and update the 3rd vextex of our graphics triangle
            _triangleGraph.CoordinateSet[3] = ModelPosition;

            //Update active view to see result
            _interactionEvents.InteractionGraphics.UpdateOverlayGraphics(
                AdnInventorUtilities.InvApplication.ActiveView);
        }
    }
}
