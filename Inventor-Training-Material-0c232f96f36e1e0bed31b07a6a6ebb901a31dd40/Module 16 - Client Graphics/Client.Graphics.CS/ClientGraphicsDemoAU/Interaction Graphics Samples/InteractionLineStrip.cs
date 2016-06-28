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
using Autodesk.ADN.Utility.Graphics;
using Autodesk.ADN.Utility.Interaction;
using Autodesk.ADN.Utility.InventorUtils;

namespace ClientGraphicsDemoAU.InteractionSamples
{
    //////////////////////////////////////////////////////////////////////////////////////////////
    // Description: An interaction example using a LinStripsGraphics and MouseEvents.
    //
    //////////////////////////////////////////////////////////////////////////////////////////////
    class InteractionLineStrip
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // Class Members
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        AdnClientGraphicsManager _clientGraphicsMng;

        AdnInteractionManager _interactionManager;

        LineStripGraphics _lineStripGraph = null;

        Random _rd;

        //////////////////////////////////////////////////////////////////////////////////////////////
        // The public static method to start the demo
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        public static void Demo()
        {
            InteractionLineStrip instance = new InteractionLineStrip();
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

            _interactionManager.Start("Click points anywhere in model window: ");

            _clientGraphicsMng = new AdnClientGraphicsManager(
                AdnInventorUtilities.InvApplication,
                AdnInventorUtilities.AddInGuid);

            _clientGraphicsMng.SetGraphicsSource(
                _interactionManager.InteractionEvents);

            _lineStripGraph = null;
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

            if (_lineStripGraph == null)
            {
                double[] front = AdnInventorUtilities.ToArray(ModelPosition);
                double[] back = AdnInventorUtilities.ToArray(ModelPosition);
                double[] combined = front.Concat(back).ToArray();

                _rd = new Random();

                _lineStripGraph = _clientGraphicsMng.DrawLineStrip(
                    combined,
                    null);

                _lineStripGraph.ColorBinding = ColorBindingEnum.kPerItemColors;

                int id = _clientGraphicsMng.WorkingGraphics.GetDataSetFreeId();

                _lineStripGraph.ColorSet =
                    _clientGraphicsMng.WorkingGraphics.GraphicsDataSets.CreateColorSet(id);

                _lineStripGraph.ColorSet.Add(
                    _lineStripGraph.ColorSet.Count + 1, 
                    (byte)_rd.Next(0, 255), 
                    (byte)_rd.Next(0, 255),
                    (byte)_rd.Next(0, 255));

                _lineStripGraph.LineWeight = 5;

                _interactionManager.MouseEvents.MouseMoveEnabled = true;

                _interactionManager.MouseEvents.OnMouseMove +=
                    new MouseEventsSink_OnMouseMoveEventHandler(MouseEvents_OnMouseMove);
            }
            else
            {
                _lineStripGraph.CoordinateSet.Add(
                    _lineStripGraph.CoordinateSet.Count + 1,
                    ModelPosition);

                 _lineStripGraph.ColorSet.Add(
                    _lineStripGraph.ColorSet.Count + 1, 
                    (byte)_rd.Next(0, 255), 
                    (byte)_rd.Next(0, 255),
                    (byte)_rd.Next(0, 255));
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
            int count = _lineStripGraph.CoordinateSet.Count;

            _lineStripGraph.CoordinateSet[count] = ModelPosition;

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
