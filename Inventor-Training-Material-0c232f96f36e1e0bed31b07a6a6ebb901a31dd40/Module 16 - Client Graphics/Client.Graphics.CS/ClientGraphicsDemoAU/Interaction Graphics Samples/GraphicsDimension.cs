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
using Autodesk.ADN.Utility.Interaction;
using Autodesk.ADN.Utility.InventorUtils;

namespace ClientGraphicsDemoAU.InteractionSamples
{
    ///////////////////////////////////////////////////////////////////////////////////////////////////
    // Description: Illutrates use of ClientGraphics to store ClientGraphics based dimensions 
    //              working in Part or Assembly documents.
    //
    //////////////////////////////////////////////////////////////////////////////////////////////////
    public class GraphicsDimension
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // Class Members
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        AdnClientGraphicsManager _clientGraphicsMng;

        AdnInteractionManager _interactionManager;

        ModeEnum _mode = ModeEnum.kPoint1;

        Point _point1;
        Point _point2;

        TransientBRep _TBrep;
        TransientGeometry _Tg;

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Selection mode enum
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        private enum ModeEnum
        {
            kPoint1,
            kPoint2,
            kDimText
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // An internal structure that holds data for a dimension
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        private struct DimData
        {
            public Point Point1;
            public Point Point2;

            public LineGraphics ExtLine1;
            public LineGraphics ExtLine2;
            public LineGraphics DimLine;

            public GraphicsNode DimNode;

            public DimData(
                Point point1,
                Point point2, 
                LineGraphics extLine1, 
                LineGraphics extLine2, 
                LineGraphics dimLine,
                GraphicsNode node)
            {
                Point1 = point1;
                Point2 = point2;

                ExtLine1 = extLine1;
                ExtLine2 = extLine2;
                DimLine = dimLine;

                DimNode = node;
            }
        }

        DimData _dimData;

        //////////////////////////////////////////////////////////////////////////////////////////////
        // public static method to start command
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        public static void CreateDimensionCmd()
        {
            GraphicsDimension dimension = new GraphicsDimension();
            dimension.StartCommand();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Starts dimension command
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void StartCommand()
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            _Tg = InvApp.TransientGeometry;
            _TBrep = InvApp.TransientBRep;

            _interactionManager = new AdnInteractionManager(InvApp);

            _interactionManager.Initialize();

            _interactionManager.OnTerminateEvent +=
              new AdnInteractionManager.OnTerminateHandler(OnTerminateEvent);

            _interactionManager.AddPreSelectionFilter(ObjectTypeEnum.kVertexObject);
            _interactionManager.AddPreSelectionFilter(ObjectTypeEnum.kVertexProxyObject);
            _interactionManager.AddPreSelectionFilter(ObjectTypeEnum.kWorkPointObject);
            _interactionManager.AddPreSelectionFilter(ObjectTypeEnum.kWorkPointProxyObject);

            _interactionManager.SelectEvents.SingleSelectEnabled = true;

            _interactionManager.SelectEvents.OnSelect +=
                     new SelectEventsSink_OnSelectEventHandler(SelectEvents_OnSelect);

            _interactionManager.Start("Select dimension first point: ");

            _clientGraphicsMng = new AdnClientGraphicsManager(
                AdnInventorUtilities.InvApplication,
                AdnInventorUtilities.AddInGuid);

            _clientGraphicsMng.SetGraphicsSource(
                _interactionManager.InteractionEvents);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Trigger final view update when Interaction is terminated
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void OnTerminateEvent(object o, AdnInteractionManager.OnTerminateEventArgs e)
        {
            _clientGraphicsMng.UpdateView();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // SelectEvent lets the user pick up vertex or workpoint
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void SelectEvents_OnSelect(ObjectsEnumerator JustSelectedEntities,
           SelectionDeviceEnum SelectionDevice,
           Point ModelPosition,
           Point2d ViewPosition,
           View View)
        {
            switch (_mode)
            {
                case ModeEnum.kPoint1:
                    {
                        _point1 = ModelPosition;
                        _mode = ModeEnum.kPoint2;
                        _interactionManager.InteractionEvents.StatusBarText = "Select dimension second point: ";
                        break;
                    }
                case ModeEnum.kPoint2:
                    {
                        _point2 = ModelPosition;

                        Vector normal = View.Camera.Eye.VectorTo(View.Camera.Target);

                        _dimData = DrawDimension(_point1, _point2, ModelPosition, normal);

                        _clientGraphicsMng.UpdateView();


                        _interactionManager.InteractionEvents.StatusBarText = "Select dimension text position: ";

                        _interactionManager.MouseEvents.MouseMoveEnabled = true;

                        _interactionManager.SelectEvents.OnSelect -=
                            new SelectEventsSink_OnSelectEventHandler(SelectEvents_OnSelect);

                        _interactionManager.MouseEvents.OnMouseDown +=
                            new MouseEventsSink_OnMouseDownEventHandler(MouseEvents_OnMouseDown);

                        _interactionManager.MouseEvents.OnMouseMove +=
                            new MouseEventsSink_OnMouseMoveEventHandler(MouseEvents_OnMouseMove);

                        _mode = ModeEnum.kDimText;

                        break;
                    }
 
                default:
                    break;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // OnMouseDown event we create the client feature that holds the dimension graphics
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

            Vector normal = View.Camera.Eye.VectorTo(View.Camera.Target);

            CreateClientFeature(
                _dimData.Point1, 
                _dimData.Point2, 
                ModelPosition, 
                normal);

            _interactionManager.Terminate();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // OnMouseMove event we update the position of our dimension lines and text
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void MouseEvents_OnMouseMove(
            MouseButtonEnum Button,
            ShiftStateEnum ShiftKeys,
            Inventor.Point ModelPosition,
            Point2d ViewPosition,
            Inventor.View View)
        {
            Vector normal = View.Camera.Eye.VectorTo(View.Camera.Target);

            UpdateDimension(_dimData, ModelPosition, normal);

            _clientGraphicsMng.UpdateView(); 
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Draws dimension graphics
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        DimData DrawDimension(Point point1, Point point2, Point dimText, Vector normal)
        {
            // Compute extension points
            Vector xAxis = point1.VectorTo(point2);

            Vector upVector = normal.CrossProduct(xAxis);
            upVector.Normalize();

            Plane plane = _Tg.CreatePlane(point1, normal);

            Point dimTextProj = AdnInventorUtilities.ProjectOnPlane(dimText, plane);

            double dotP1 = point1.VectorTo(dimTextProj).DotProduct(upVector);
            double dotP2 = point2.VectorTo(dimTextProj).DotProduct(upVector);

            Point extP1 = _Tg.CreatePoint(
                point1.X + upVector.X * dotP1,
                point1.Y + upVector.Y * dotP1,
                point1.Z + upVector.Z * dotP1);

            Point extP2 = _Tg.CreatePoint(
                point2.X + upVector.X * dotP2,
                point2.Y + upVector.Y * dotP2,
                point2.Z + upVector.Z * dotP2);

            double dimValue = extP1.DistanceTo(extP2);

           
            GraphicsNode node = _clientGraphicsMng.CreateNewGraphicsNode();

            LineGraphics extLine1 = _clientGraphicsMng.DrawLine(
                   AdnInventorUtilities.ToArray(point1),
                   AdnInventorUtilities.ToArray(extP1),
                   node);

            LineGraphics extLine2 = _clientGraphicsMng.DrawLine(
                   AdnInventorUtilities.ToArray(point2),
                   AdnInventorUtilities.ToArray(extP2),
                   node);

            LineGraphics dimLine = _clientGraphicsMng.DrawLine(
                  AdnInventorUtilities.ToArray(extP1),
                  AdnInventorUtilities.ToArray(extP2),
                  node);

            extLine1.LineType = LineTypeEnum.kDashedLineType;
            extLine2.LineType = LineTypeEnum.kDashedLineType;

            UnitVector v = extP1.VectorTo(extP2).AsUnitVector();

            double length = 20.0;
            double radius = 7.0;

            Point bottom1 = _Tg.CreatePoint(
                extP1.X + length * v.X,
                extP1.Y + length * v.Y,
                extP1.Z + length * v.Z);

            Point bottom2 = _Tg.CreatePoint(
               extP2.X - length * v.X,
               extP2.Y - length * v.Y,
               extP2.Z - length * v.Z);

            SurfaceBody cone1 = _TBrep.CreateSolidCylinderCone(
                bottom1, extP1,
                radius, radius, 0.0, null);

            SurfaceBody cone2 = _TBrep.CreateSolidCylinderCone(
                bottom2, extP2,
                radius, radius, 0.0, null);

            GraphicsNode dimNode = _clientGraphicsMng.CreateNewGraphicsNode();

            SurfaceGraphics arrow1 = _clientGraphicsMng.DrawSurface(cone1, dimNode);
            SurfaceGraphics arrow2 = _clientGraphicsMng.DrawSurface(cone2, dimNode);

            arrow1.SetTransformBehavior(extP1,
                DisplayTransformBehaviorEnum.kPixelScaling,
                1.0);

            arrow2.SetTransformBehavior(extP2,
               DisplayTransformBehaviorEnum.kPixelScaling,
               1.0);

            
            TextGraphics text = _clientGraphicsMng.DrawText(
                AdnInventorUtilities.GetStringFromAPILength(dimValue),
                false,
                dimNode);

            text.Font = "Arial";
            text.Bold = false;
            text.Italic = false;
            text.FontSize = 20;
            text.PutTextColor(221, 0, 0);
            text.VerticalAlignment = VerticalTextAlignmentEnum.kAlignTextMiddle;
            text.HorizontalAlignment = HorizontalTextAlignmentEnum.kAlignTextLeft;

            Point txtPos = _Tg.CreatePoint(
                (extP1.X + extP2.X) * 0.5,
                (extP1.Y + extP2.Y) * 0.5,
                (extP1.Z + extP2.Z) * 0.5);

            text.Anchor = txtPos ;

            text.SetTransformBehavior(txtPos,
              DisplayTransformBehaviorEnum.kFrontFacingAndPixelScaling,
              1.0);

            node.Selectable = true;
            dimNode.Selectable = true;

            return new DimData(point1, point2, extLine1, extLine2, dimLine, dimNode);
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Update the position of dimension lines and text
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void UpdateDimension(DimData dimData, Point dimText, Vector normal)
        { 
            Vector xAxis = dimData.Point1.VectorTo(dimData.Point2);

            Vector upVector = normal.CrossProduct(xAxis);
            upVector.Normalize();

            Plane plane = _Tg.CreatePlane(dimData.Point1, normal);

            Point dimTextProj = AdnInventorUtilities.ProjectOnPlane(dimText, plane);

            double dotP1 = dimData.Point1.VectorTo(dimTextProj).DotProduct(upVector);
            double dotP2 = dimData.Point2.VectorTo(dimTextProj).DotProduct(upVector);

            Point extP1 = _Tg.CreatePoint(
                dimData.Point1.X + upVector.X * dotP1,
                dimData.Point1.Y + upVector.Y * dotP1,
                dimData.Point1.Z + upVector.Z * dotP1);
            
            Point extP2 = _Tg.CreatePoint(
                dimData.Point2.X + upVector.X * dotP2,
                dimData.Point2.Y + upVector.Y * dotP2,
                dimData.Point2.Z + upVector.Z * dotP2);

            dimData.ExtLine1.CoordinateSet[1] = dimData.Point1;
            dimData.ExtLine1.CoordinateSet[2] = extP1;

            dimData.ExtLine2.CoordinateSet[1] = dimData.Point2;
            dimData.ExtLine2.CoordinateSet[2] = extP2;

            dimData.DimLine.CoordinateSet[1] = extP1;
            dimData.DimLine.CoordinateSet[2] = extP2;

            Point midPoint = _Tg.CreatePoint(
                 (extP1.X + extP2.X - dimData.Point1.X - dimData.Point2.X) * 0.5,
                 (extP1.Y + extP2.Y - dimData.Point1.Y - dimData.Point2.Y) * 0.5,
                 (extP1.Z + extP2.Z - dimData.Point1.Z - dimData.Point2.Z) * 0.5);

            SetNodePosition(dimData.DimNode, midPoint);
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
        // Creates ClientFeature that holds our dimension graphics
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void CreateClientFeature(Point point1, Point point2, Point dimText, Vector normal)
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            Document document = InvApp.ActiveDocument;

            ComponentDefinition compDef =
                AdnInventorUtilities.GetCompDefinition(document);
               
            object features =
                 AdnInventorUtilities.GetProperty(compDef, "Features");

            ClientFeatures clientFeatures =
                 AdnInventorUtilities.GetProperty(features, "ClientFeatures")
                    as ClientFeatures;

            ClientFeatureDefinition cfDef = 
                clientFeatures.CreateDefinition(
                    "Dimension Feature", 
                    null, null, null);

            ClientFeature clientFeature = 
                clientFeatures.Add(cfDef, AdnInventorUtilities.AddInGuid);

            cfDef = clientFeature.Definition;

            cfDef.HighlightClientGraphicsWithFeature = true;

            NativeBrowserNodeDefinition nodeDef =
               clientFeature.BrowserNode.BrowserNodeDefinition as NativeBrowserNodeDefinition;

            stdole.IPictureDisp pic =
                PictureDispConverter.ToIPictureDisp(Resources.dimlinear);

            ClientNodeResource res =
                document.BrowserPanes.ClientNodeResources.Add(
                    AdnInventorUtilities.AddInGuid,
                    document.BrowserPanes.ClientNodeResources.Count + 1,
                    pic);

            nodeDef.OverrideIcon = res;

            _clientGraphicsMng.SetGraphicsSource(clientFeature);

           DrawDimension(point1, point2, dimText, normal);

            _clientGraphicsMng.UpdateView();
        }
    }
}
