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
    //////////////////////////////////////////////////////////////////////////////////////////////////
    // Description: Illutrates use of ClientGraphics in Interaction mode, 
    //              by drawing circle on the selected surface (planar face or workplane).
    //
    //////////////////////////////////////////////////////////////////////////////////////////////////
    class CircleJig
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // Class Members
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        private AdnClientGraphicsManager _clientGraphicsMng;

        private AdnInteractionManager _interactionManager;

        private CurveGraphics _curveGraph = null;

        private SelectModeEnum _mode;

        private Plane _plane;

        private Point _center;

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Enum to keep track of current selection mode
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        private enum SelectModeEnum
        { 
            kPlaneSelect,
            kCenterSelect,
            kRadiusSelect
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // The public static method to start the demo
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        public static void Demo()
        {
            CircleJig instance = new CircleJig();
            instance.DoDemo();
        }

        void DoDemo()
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            _interactionManager = new AdnInteractionManager(InvApp);

            _interactionManager.Initialize();

            _interactionManager.SelectEvents.SingleSelectEnabled = true;

            _interactionManager.OnTerminateEvent += 
                new AdnInteractionManager.OnTerminateHandler(OnTerminateEvent);

            _interactionManager.SelectEvents.OnSelect +=
                     new SelectEventsSink_OnSelectEventHandler(SelectEvents_OnSelect);

            _interactionManager.AddSelectionFilter(SelectionFilterEnum.kPartFacePlanarFilter);
            _interactionManager.AddSelectionFilter(SelectionFilterEnum.kWorkPlaneFilter);

            _interactionManager.Start("Select workplane/planar face: ");

            _clientGraphicsMng = new AdnClientGraphicsManager(
                AdnInventorUtilities.InvApplication,
                AdnInventorUtilities.AddInGuid);

            _clientGraphicsMng.SetGraphicsSource(
                _interactionManager.InteractionEvents);

            _curveGraph = null;

            _mode = SelectModeEnum.kPlaneSelect;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // 
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void SelectEvents_OnSelect(
            ObjectsEnumerator JustSelectedEntities, 
            SelectionDeviceEnum SelectionDevice, 
            Point ModelPosition, 
            Point2d ViewPosition, 
            View View)
        {
            switch (_mode)
            {
                case SelectModeEnum.kPlaneSelect:
                    {
                        _plane = AdnInventorUtilities.GetPlane(JustSelectedEntities[1]);

                        _interactionManager.MouseEvents.MouseMoveEnabled = true;

                        _interactionManager.ClearSelectionFilters();

                        _interactionManager.AddSelectionFilter(SelectionFilterEnum.kPartVertexFilter);
                        _interactionManager.AddSelectionFilter(SelectionFilterEnum.kWorkPointFilter);

                        _interactionManager.InteractionEvents.StatusBarText = "Select center: ";

                        _mode = SelectModeEnum.kCenterSelect;

                        break;
                    }
                case SelectModeEnum.kCenterSelect:
                    {
                        _center = AdnInventorUtilities.GetPoint(JustSelectedEntities[1]);

                        Circle circle = AdnInventorUtilities.InvApplication.TransientGeometry.CreateCircle(
                            _center,
                            _plane.Normal,
                            0.001);

                        _curveGraph = _clientGraphicsMng.DrawCurve(circle);

                        _curveGraph.LineWeight = 0.5;

                        _interactionManager.InteractionEvents.StatusBarText = "Select radius: ";

                        _interactionManager.MouseEvents.OnMouseMove +=
                            new MouseEventsSink_OnMouseMoveEventHandler(MouseEvents_OnMouseMove);

                        _interactionManager.SelectEvents.OnSelect -=
                           new SelectEventsSink_OnSelectEventHandler(SelectEvents_OnSelect);

                        _mode = SelectModeEnum.kRadiusSelect;

                        break;
                    }
                default:
                    break;
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
            Point point = AdnInventorUtilities.ProjectOnPlane(ModelPosition, _plane);

            double radius = _center.DistanceTo(
                AdnInventorUtilities.ProjectOnPlane(
                    ModelPosition,
                    _plane));

            Circle circle = AdnInventorUtilities.InvApplication.TransientGeometry.CreateCircle(
                _center,
                _plane.Normal,
                radius);

            _curveGraph.Curve = circle;

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
