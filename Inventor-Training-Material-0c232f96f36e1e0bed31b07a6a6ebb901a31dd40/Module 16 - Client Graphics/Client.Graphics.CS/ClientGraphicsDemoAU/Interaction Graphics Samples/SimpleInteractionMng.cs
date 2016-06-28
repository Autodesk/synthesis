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
    // Description: A simple interaction example using a LineGraphics and MouseEvents, but with
    //              the AdnClientGraphicsManager and AdnInteractionManager utilities
    //
    //////////////////////////////////////////////////////////////////////////////////////////////////
    class SimpleInteractionMng
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // Class Members
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        AdnClientGraphicsManager _clientGraphicsMng;

        AdnInteractionManager _interactionManager;

        LineGraphics _lineGraph = null;

        //////////////////////////////////////////////////////////////////////////////////////////////
        // The public static method to start the demo
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        public static void Demo()
        {
            SimpleInteractionMng instance = new SimpleInteractionMng();
            instance.DoDemo();
        }

        void DoDemo()
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            _interactionManager = new AdnInteractionManager(InvApp);

            _interactionManager.Initialize();

            _interactionManager.OnTerminateEvent +=
               new AdnInteractionManager.OnTerminateHandler(OnTerminateEvent);

            _interactionManager.MouseEvents.OnMouseDown += 
                new MouseEventsSink_OnMouseDownEventHandler(MouseEvents_OnMouseDown);

            _interactionManager.Start("Select Start Point: ");

            _clientGraphicsMng = new AdnClientGraphicsManager(
                AdnInventorUtilities.InvApplication,
                AdnInventorUtilities.AddInGuid);

            _clientGraphicsMng.SetGraphicsSource(
                _interactionManager.InteractionEvents);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // OnMouseDown event, creates and start drawing the line graphics
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

            _lineGraph = _clientGraphicsMng.DrawLine(
                AdnInventorUtilities.ToArray(ModelPosition),
                AdnInventorUtilities.ToArray(ModelPosition),
                null);

            _interactionManager.MouseEvents.MouseMoveEnabled = true;

            _interactionManager.MouseEvents.OnMouseDown -=
                new MouseEventsSink_OnMouseDownEventHandler(MouseEvents_OnMouseDown);

            _interactionManager.MouseEvents.OnMouseMove +=
                new MouseEventsSink_OnMouseMoveEventHandler(MouseEvents_OnMouseMove);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // OnMouseMove events we update end point of the graphic line with current mouse position
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void MouseEvents_OnMouseMove(
            MouseButtonEnum Button, 
            ShiftStateEnum ShiftKeys, 
            Inventor.Point ModelPosition, 
            Point2d ViewPosition, 
            Inventor.View View)
        {
            _lineGraph.CoordinateSet[2] = ModelPosition;

            _clientGraphicsMng.UpdateView();
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
