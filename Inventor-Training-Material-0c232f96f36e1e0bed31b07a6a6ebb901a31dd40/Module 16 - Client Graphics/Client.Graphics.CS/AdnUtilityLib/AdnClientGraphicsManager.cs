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
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using System.Text;

using Inventor;
using Autodesk.ADN.Utility.InventorUtils;

namespace Autodesk.ADN.Utility.Graphics
{
    //////////////////////////////////////////////////////////////////////////////////////////////
    // Description: Public Enums used by AdnClientGraphicsManager
    //
    //////////////////////////////////////////////////////////////////////////////////////////////
    public enum AdnGraphicModeEnum
    {
        kDocumentGraphics,
        kInteractionGraphics,
        kClientFeatureGraphics,
        kDrawingViewGraphics,
        kDrawingSheetGraphics,
        kFlatPatternGraphics
    }

    public enum AdnInteractionGraphicsModeEnum
    { 
        kPreviewGraphics,
        kOverlayGraphics
    }

    //////////////////////////////////////////////////////////////////////////////////////////////
    // Description: a Utility class to manipulate ClientGraphics
    //
    //////////////////////////////////////////////////////////////////////////////////////////////
    public class AdnClientGraphicsManager
    {
        private Inventor.Application _Application;
        private string _clientId;

        private InteractionEvents _workingInteraction;
        private ClientFeature _workingFeature;
        private DrawingView _workingView;
        private FlatPattern _workingFlat;
        private Document _workingDocument;
        private Sheet _workingSheet;

        private AdnGraphicModeEnum _mode;

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Set whether or not ClientGraphics and GraphicsData are transacted or not
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public bool Transacting
        {
            get;
            set;
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Set InteractionGraphics to Preview or Overlay
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public AdnInteractionGraphicsModeEnum InteractionGraphicsMode
        {
            get;
            set;
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Returns the current AdnGraphicsData depending of the graphic source
        //      (Document, InteractionEvents, ClientFeature, ...)
        //////////////////////////////////////////////////////////////////////////////////////
        public AdnGraphics WorkingGraphics
        {
            get
            {
                switch (_mode)
                {
                    case AdnGraphicModeEnum.kDocumentGraphics:
                        return new AdnGraphics(_workingDocument, _clientId, Transacting);

                    case AdnGraphicModeEnum.kInteractionGraphics:
                        return new AdnGraphics(_workingInteraction, InteractionGraphicsMode);

                    case AdnGraphicModeEnum.kClientFeatureGraphics:
                        return new AdnGraphics(_workingFeature, _clientId, true);

                    case AdnGraphicModeEnum.kDrawingViewGraphics:
                        return new AdnGraphics(_workingView, _clientId, Transacting);

                    case AdnGraphicModeEnum.kDrawingSheetGraphics:
                        return new AdnGraphics(_workingSheet, _clientId, Transacting);

                    case AdnGraphicModeEnum.kFlatPatternGraphics:
                        return new AdnGraphics(_workingFlat, _clientId, Transacting);

                    default:
                        return null;
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: AdnClientGraphicsManager Constructor
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public AdnClientGraphicsManager(Inventor.Application Application, string clientId)
        {
            _Application = Application;

            _clientId = clientId;

            _workingDocument = _Application.ActiveDocument;

            Transacting = true;

            _mode = AdnGraphicModeEnum.kDocumentGraphics;

            InteractionGraphicsMode = AdnInteractionGraphicsModeEnum.kPreviewGraphics;
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Overloaded methods to define the graphic source
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public void SetGraphicsSource(Document document)
        {
            _workingDocument = document;

            _mode = AdnGraphicModeEnum.kDocumentGraphics;
        }

        public void SetGraphicsSource(DrawingView drawingView)
        {
            _workingView = drawingView;

            _mode = AdnGraphicModeEnum.kDrawingViewGraphics;
        }

        public void SetGraphicsSource(Sheet sheet)
        {
            _workingSheet = sheet;

            _mode = AdnGraphicModeEnum.kDrawingSheetGraphics;
        }

        public void SetGraphicsSource(FlatPattern flatPattern)
        {
            _workingFlat = flatPattern;

            _mode = AdnGraphicModeEnum.kFlatPatternGraphics;
        }

        public void SetGraphicsSource(InteractionEvents interactionEvents)
        {
            _workingInteraction = interactionEvents;

            _mode = AdnGraphicModeEnum.kInteractionGraphics;
        }

        public void SetGraphicsSource(ClientFeature feature)
        {
            _workingFeature = feature;

            _mode = AdnGraphicModeEnum.kClientFeatureGraphics;
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Retrieve current graphic source
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public object GetGraphicsSource(out AdnGraphicModeEnum mode)
        {
            mode = _mode;

            switch (_mode)
            {
                case AdnGraphicModeEnum.kDocumentGraphics:
                    return _workingDocument;

                case AdnGraphicModeEnum.kInteractionGraphics:
                    return _workingInteraction;

                case AdnGraphicModeEnum.kClientFeatureGraphics:
                    return _workingFeature;

                case AdnGraphicModeEnum.kDrawingViewGraphics:
                    return _workingView;

                case AdnGraphicModeEnum.kDrawingSheetGraphics:
                    return _workingSheet;

                case AdnGraphicModeEnum.kFlatPatternGraphics:
                    return _workingFlat;

                default:
                    return null;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Update view to display non-interaction or interaction drawn graphics
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public void UpdateView()
        {
            View activeView = _Application.ActiveView;

            if (_mode == AdnGraphicModeEnum.kInteractionGraphics && 
                InteractionGraphicsMode == AdnInteractionGraphicsModeEnum.kOverlayGraphics)

                _workingInteraction.InteractionGraphics.UpdateOverlayGraphics(activeView);

            else

                activeView.Update();
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Delete all graphics for input source created by the AdnClientGraphicsManager
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public void DeleteGraphics(Document document, bool deleteData)
        {
            AdnGraphics dataTx = new AdnGraphics(document, _clientId, true, false);
            dataTx.Delete(deleteData);

            AdnGraphics dataNonTx = new AdnGraphics(document, _clientId, false, false);
            dataNonTx.Delete(deleteData);
        }

        public void DeleteGraphics(DrawingView drawingView, bool deleteData)
        {
            AdnGraphics dataTx = new AdnGraphics(drawingView, _clientId, true);
            dataTx.Delete(deleteData);

            AdnGraphics dataNonTx = new AdnGraphics(drawingView, _clientId, false);
            dataNonTx.Delete(deleteData);
        }

        public void DeleteGraphics(Sheet sheet, bool deleteData)
        {
            AdnGraphics dataTx = new AdnGraphics(sheet, _clientId, true);
            dataTx.Delete(deleteData);

            AdnGraphics dataNonTx = new AdnGraphics(sheet, _clientId, false);
            dataNonTx.Delete(deleteData);
        }

        public void DeleteGraphics(InteractionEvents interactionEvents, bool deleteData)
        {
            AdnGraphics data = new AdnGraphics(interactionEvents, InteractionGraphicsMode);
            data.Delete(deleteData);
        }

        public void DeleteGraphics(ClientFeature feature, bool deleteData)
        {
            AdnGraphics data = new AdnGraphics(feature, _clientId, true);
            data.Delete(deleteData);
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Returns a new GraphicsNode
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public GraphicsNode CreateNewGraphicsNode()
        { 
            AdnGraphics graphicsData = WorkingGraphics;

            GraphicsNode node = graphicsData.ClientGraphics.AddNode(
                    graphicsData.GetGraphicNodeFreeId());

            return node;
        }

        public GraphicsNode CreateNewGraphicsNode(int customId)
        {
            try
            {
                AdnGraphics graphicsData = WorkingGraphics;

                GraphicsNode node = graphicsData.ClientGraphics.AddNode(customId);

                return node;
            }
            catch
            {
                return null;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Draws a LineGraphics primitive
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public LineGraphics DrawLine(
            double[] startPoint, 
            double[] endPoint)
        {
            return DrawLine(startPoint, endPoint, null);
        }
        
        public LineGraphics DrawLine(
            double[] startPoint, 
            double[] endPoint, 
            GraphicsNode node)
        {
            try
            {
                AdnGraphics graphicsData = WorkingGraphics;

                if (node == null)
                {
                    node = graphicsData.ClientGraphics.AddNode(
                        graphicsData.GetGraphicNodeFreeId());
                }

                LineGraphics graphic = node.AddLineGraphics();

                if ((startPoint != null) && (endPoint != null))
                {
                    GraphicsCoordinateSet coordSet =
                        graphicsData.GraphicsDataSets.CreateCoordinateSet(
                            graphicsData.GetDataSetFreeId());

                    double[] coordsArray = startPoint.Concat(endPoint).ToArray();

                    coordSet.PutCoordinates(ref coordsArray);

                    graphic.CoordinateSet = coordSet;
                }

                return graphic;
            }
            catch
            {
                return null;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Draws a LineStripGraphics primitive
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public LineStripGraphics DrawLineStrip(
            double[] coordinates)
        {
            return DrawLineStrip(coordinates, null);
        }

        public LineStripGraphics DrawLineStrip(
            double[] coordinates,
            GraphicsNode node)
        {
            try
            {
                AdnGraphics graphicsData = WorkingGraphics;

                if (node == null)
                {
                    node = graphicsData.ClientGraphics.AddNode(
                        graphicsData.GetGraphicNodeFreeId());
                }

                LineStripGraphics graphic = node.AddLineStripGraphics();

                if (coordinates != null)
                {
                    GraphicsCoordinateSet coordSet =
                        graphicsData.GraphicsDataSets.CreateCoordinateSet(
                            graphicsData.GetDataSetFreeId());

                    coordSet.PutCoordinates(ref coordinates);

                    graphic.CoordinateSet = coordSet;
                }

                return graphic;
            }
            catch
            {
                return null;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Draws a TriangleGraphics primitive
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public TriangleGraphics DrawTriangle(
            double[] v1,
            double[] v2,
            double[] v3)
        {
            return DrawTriangle(v1, v2, v3, null);
        }

        public TriangleGraphics DrawTriangle(
            double[] v1,
            double[] v2,
            double[] v3,
            GraphicsNode node)
        {
            try
            {
                AdnGraphics graphicsData = WorkingGraphics;

                if (node == null)
                {
                    node = graphicsData.ClientGraphics.AddNode(
                        graphicsData.GetGraphicNodeFreeId());
                }

                TriangleGraphics graphic = node.AddTriangleGraphics();

                if ((v1 != null) && (v2 != null) && (v3 != null))
                {
                    GraphicsCoordinateSet coordSet =
                        graphicsData.GraphicsDataSets.CreateCoordinateSet(
                            graphicsData.GetDataSetFreeId());

                    List<double> coordinates = new List<double>();
                    
                    coordinates.AddRange(v1);
                    coordinates.AddRange(v2);
                    coordinates.AddRange(v3);

                    double[] coordsArray = coordinates.ToArray();

                    coordSet.PutCoordinates(ref coordsArray);

                    graphic.CoordinateSet = coordSet;
                }

                return graphic;
            }
            catch
            {
                return null;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Draws a TriangleFanGraphics primitive
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public TriangleFanGraphics DrawTriangleFan(
           double[] coordinates)
        {
            return DrawTriangleFan(coordinates, null);
        }

        public TriangleFanGraphics DrawTriangleFan(
           double[] coordinates,
           GraphicsNode node)
        {
            try
            {
                AdnGraphics graphicsData = WorkingGraphics;

                if (node == null)
                {
                    node = graphicsData.ClientGraphics.AddNode(
                        graphicsData.GetGraphicNodeFreeId());
                }

                TriangleFanGraphics graphic = node.AddTriangleFanGraphics();

                if (coordinates != null)
                {
                    GraphicsCoordinateSet coordSet =
                        graphicsData.GraphicsDataSets.CreateCoordinateSet(
                            graphicsData.GetDataSetFreeId());

                    coordSet.PutCoordinates(ref coordinates);

                    graphic.CoordinateSet = coordSet;
                }

                return graphic;
            }
            catch
            {
                return null;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Draws a TriangleStripGraphics  primitive
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public TriangleStripGraphics DrawTriangleStrip(
          double[] coordinates)
        {
            return DrawTriangleStrip(coordinates, null);
        }

        public TriangleStripGraphics DrawTriangleStrip(
           double[] coordinates,
           GraphicsNode node)
        {
            try
            {
                AdnGraphics graphicsData = WorkingGraphics;

                if (node == null)
                {
                    node = graphicsData.ClientGraphics.AddNode(
                        graphicsData.GetGraphicNodeFreeId());
                }

                TriangleStripGraphics graphic = node.AddTriangleStripGraphics();

                if (coordinates != null)
                {
                    GraphicsCoordinateSet coordSet =
                        graphicsData.GraphicsDataSets.CreateCoordinateSet(
                            graphicsData.GetDataSetFreeId());

                    coordSet.PutCoordinates(ref coordinates);

                    graphic.CoordinateSet = coordSet;
                }

                return graphic;
            }
            catch
            {
                return null;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Draws a CurveGraphics primitive
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public CurveGraphics DrawCurve(
          object curve)
        {
            return DrawCurve(curve, null);
        }

        public CurveGraphics DrawCurve(
            object curve,
            GraphicsNode node)
        {
            try
            {
                AdnGraphics graphicsData = WorkingGraphics;

                if (node == null)
                {
                    node = graphicsData.ClientGraphics.AddNode(
                        graphicsData.GetGraphicNodeFreeId());
                }

                CurveGraphics graphic = node.AddCurveGraphics(curve);

                return graphic;
            }
            catch
            {
                return null;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Draws a SurfaceGraphics primitive
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public SurfaceGraphics DrawSurface(
            object surface)
        {
            return DrawSurface(surface, null);
        }

        public SurfaceGraphics DrawSurface(
            object surface,
            GraphicsNode node)
        {
            try
            {
                AdnGraphics graphicsData = WorkingGraphics;

                if (node == null)
                {
                    node = graphicsData.ClientGraphics.AddNode(
                        graphicsData.GetGraphicNodeFreeId());
                }

                SurfaceGraphics graphic = node.AddSurfaceGraphics(surface);

                return graphic;
            }
            catch
            {
                return null;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Draws a ComponentGraphics primitive
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public ComponentGraphics DrawComponent(
           ComponentDefinition compDef)
        {
            return DrawComponent(compDef, null);
        }

        public ComponentGraphics DrawComponent(
            ComponentDefinition compDef,
            GraphicsNode node)
        {
            try
            {
                AdnGraphics graphicsData = WorkingGraphics;

                if (node == null)
                {
                    node = graphicsData.ClientGraphics.AddNode(
                        graphicsData.GetGraphicNodeFreeId());
                }

                ComponentGraphics graphic = node.AddComponentGraphics(compDef);

                return graphic;
            }
            catch
            {
                return null;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Draws a TextGraphics primitive
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public TextGraphics DrawText(
            string text,
            bool scalable)
        {
            return DrawText(text, scalable, null);
        }

        public TextGraphics DrawText(
            string text,
            bool scalable,
            GraphicsNode node)
        {
            try
            {
                AdnGraphics graphicsData = WorkingGraphics;

                if (node == null)
                {
                    node = graphicsData.ClientGraphics.AddNode(
                        graphicsData.GetGraphicNodeFreeId());
                }

                TextGraphics graphic = (scalable ? node.AddScalableTextGraphics() : node.AddTextGraphics());

                graphic.Text = text;

                return graphic;
            }
            catch
            {
                return null;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Draws a PointGraphics  primitive
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public PointGraphics DrawPoint(
           double[] position)
        {
            return DrawPoint(position, null);
        }

        public PointGraphics DrawPoint(
            double[] position,
            GraphicsNode node)
        {
            try
            {
                AdnGraphics graphicsData = WorkingGraphics;

                if (node == null)
                {
                    node = graphicsData.ClientGraphics.AddNode(
                        graphicsData.GetGraphicNodeFreeId());
                }

                PointGraphics graphic = node.AddPointGraphics();

                if (position != null)
                {
                    GraphicsCoordinateSet coordSet =
                        graphicsData.GraphicsDataSets.CreateCoordinateSet(
                            graphicsData.GetDataSetFreeId());

                    coordSet.PutCoordinates(ref position);

                    graphic.CoordinateSet = coordSet;
                }

                return graphic;
            }
            catch
            {
                return null;
            }
        }
    }

    //////////////////////////////////////////////////////////////////////////////////////
    // Use: A utility class to handle GraphicsDataSets and ClientGraphics in a single 
    //      object
    //////////////////////////////////////////////////////////////////////////////////////
    public class AdnGraphics
    {
        private string _clientId;
  
        private GraphicsDataSets _graphicsData;
        private ClientGraphics _graphics;

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Returns GraphicsDataSets object
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public GraphicsDataSets GraphicsDataSets
        {
            get
            {
                return _graphicsData;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Returns ClientGraphics object
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public ClientGraphics ClientGraphics
        {
            get
            {
                return _graphics;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: AdnGraphicsData constructor for ComponentDefinition graphics 
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public AdnGraphics(
            Document document, 
            string clientId, 
            bool transacting, 
            bool createIfNotExist)
        {
            _clientId = clientId + (transacting ? "-Tx" : "-NonTx");

            _graphicsData = null;
            _graphics = null;

            try
            {
                _graphicsData = document.GraphicsDataSetsCollection[_clientId];
            }
            catch
            {
                if (createIfNotExist)
                {
                    if (transacting)
                    {
                        _graphicsData = document.GraphicsDataSetsCollection.Add(_clientId);
                    }
                    else
                    {
                        _graphicsData = document.GraphicsDataSetsCollection.AddNonTransacting(_clientId);
                    }
                }
            }

            ComponentDefinition compDef = GetCompDefinition(document);

            try
            {
                _graphics = compDef.ClientGraphicsCollection[_clientId];
            }
            catch
            {
                if (createIfNotExist)
                {
                    if (transacting)
                    {
                        _graphics = compDef.ClientGraphicsCollection.Add(_clientId);
                    }
                    else
                    {
                        _graphics = compDef.ClientGraphicsCollection.AddNonTransacting(_clientId);
                    }
                }
            }
        }

        public AdnGraphics(Document document, string clientId, bool transacting):
            this(document, clientId, transacting, true)
        {
            
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // 
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private static ComponentDefinition GetCompDefinition(Document document)
        {
            switch (document.DocumentType)
            {
                case DocumentTypeEnum.kAssemblyDocumentObject:
                    AssemblyDocument asm = document as AssemblyDocument;
                    return asm.ComponentDefinition as ComponentDefinition;

                case DocumentTypeEnum.kPartDocumentObject:
                    PartDocument part = document as PartDocument;
                    return part.ComponentDefinition as ComponentDefinition;

                default:
                    return null;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: AdnGraphicsData constructor for DrawingView graphics 
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public AdnGraphics(DrawingView view, string clientId, bool transacting)
        {
            _clientId = clientId + (transacting ? "-Tx" : "-NonTx");

            _graphicsData = null;
            _graphics = null;

            try
            {
                _graphicsData = view.GraphicsDataSetsCollection[_clientId];
            }
            catch
            {          
                if (transacting)
                {
                    _graphicsData = view.GraphicsDataSetsCollection.Add(_clientId);
                }
                else
                {
                    _graphicsData = view.GraphicsDataSetsCollection.AddNonTransacting(_clientId);
                }
            }

            try
            {
                _graphics = view.ClientGraphicsCollection[_clientId];
            }
            catch
            {
                if (transacting)
                {
                    _graphics = view.ClientGraphicsCollection.Add(_clientId);
                }
                else
                {
                    _graphics = view.ClientGraphicsCollection.AddNonTransacting(_clientId);
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: AdnGraphicsData constructor for Sheet graphics 
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public AdnGraphics(Sheet sheet, string clientId, bool transacting)
        {
            _clientId = clientId + (transacting ? "-Tx" : "-NonTx");

            _graphicsData = null;
            _graphics = null;

            try
            {
                _graphicsData = sheet.GraphicsDataSetsCollection[_clientId];
            }
            catch
            {
                if (transacting)
                {
                    _graphicsData = sheet.GraphicsDataSetsCollection.Add(_clientId);
                }
                else
                {
                    _graphicsData = sheet.GraphicsDataSetsCollection.AddNonTransacting(_clientId);
                }
            }

            try
            {
                _graphics = sheet.ClientGraphicsCollection[_clientId];
            }
            catch
            {
                if (transacting)
                {
                    _graphics = sheet.ClientGraphicsCollection.Add(_clientId);
                }
                else
                {
                    _graphics = sheet.ClientGraphicsCollection.AddNonTransacting(_clientId);
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: AdnGraphicsData constructor for FlatPattern graphics 
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public AdnGraphics(FlatPattern flat, string clientId, bool transacting)
        {
            _clientId = clientId + (transacting ? "-Tx" : "-NonTx");

            _graphicsData = null;
            _graphics = null;

            try
            {
                _graphicsData = flat.GraphicsDataSetsCollection[_clientId];
            }
            catch
            {
                if (transacting)
                {
                    _graphicsData = flat.GraphicsDataSetsCollection.Add(_clientId);
                }
                else
                {
                    _graphicsData = flat.GraphicsDataSetsCollection.AddNonTransacting(_clientId);
                }
            }

            try
            {
                _graphics = flat.ClientGraphicsCollection[_clientId];
            }
            catch
            {
                if (transacting)
                {
                    _graphics = flat.ClientGraphicsCollection.Add(_clientId);
                }
                else
                {
                    _graphics = flat.ClientGraphicsCollection.AddNonTransacting(_clientId);
                }
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: AdnGraphicsData constructor for Interaction graphics 
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public AdnGraphics(
            InteractionEvents InteractionEvents, 
            AdnInteractionGraphicsModeEnum mode)
        {
            _graphicsData = InteractionEvents.InteractionGraphics.GraphicsDataSets;

            switch(mode)
            {
                case AdnInteractionGraphicsModeEnum.kOverlayGraphics:
                    _graphics = InteractionEvents.InteractionGraphics.OverlayClientGraphics;
                    break;
                case AdnInteractionGraphicsModeEnum.kPreviewGraphics:
                    _graphics = InteractionEvents.InteractionGraphics.PreviewClientGraphics;
                    break;
                default:
                    break;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: AdnGraphicsData constructor for ClientFeature graphics 
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public AdnGraphics(ClientFeature feature, string clientId, bool saveWithDoc)
        {
            _clientId = clientId;

            ClientFeatureDefinition cfDef = feature.Definition;

            try
            {
                _graphicsData = cfDef.GraphicsDataSetsCollection[_clientId];
            }
            catch
            {
                _graphicsData = cfDef.GraphicsDataSetsCollection.Add2(_clientId, saveWithDoc);
            }

            try
            {
                _graphics = cfDef.ClientGraphicsCollection[_clientId];
            }
            catch
            {
                _graphics = cfDef.ClientGraphicsCollection.Add(_clientId);
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Returns first free Id for new GraphicsDataSet to be created
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public int GetDataSetFreeId()
        {
            List<int> ids = new List<int>();

            foreach (GraphicsDataSet data in _graphicsData)
            {
                ids.Add(data.Id);
            }

            int freeId = 1;

            while (ids.Contains(freeId))
                ++freeId;

            return freeId;
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Returns first free Id for new GraphicsNode to be created
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public int GetGraphicNodeFreeId()
        {
            List<int> ids = new List<int>();

            foreach (GraphicsNode node in _graphics)
            {
                ids.Add(node.Id);
            }

            int freeId = 1;

            while (ids.Contains(freeId))
                ++freeId;

            return freeId;
        }

        //////////////////////////////////////////////////////////////////////////////////////
        // Use: Delete graphics collections own by that object 
        //
        //////////////////////////////////////////////////////////////////////////////////////
        public void Delete(bool deleteData)
        {
            if(deleteData)
                if(_graphicsData != null)
                    _graphicsData.Delete();

            if (_graphics != null)
                _graphics.Delete();
        }
    }
}
