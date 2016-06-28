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
    //////////////////////////////////////////////////////////////////////////////////////////////
    // Description: Illutrates use of slice GraphicsNode functionality
    //
    //////////////////////////////////////////////////////////////////////////////////////////////
    class SliceGraphics
    {
        //////////////////////////////////////////////////////////////////////////////////////////////
        // Class Members
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        AdnInteractionManager _interactionManager;

        AdnClientGraphicsManager _clientGraphicsMng;

        List<GraphicsNode> _nodes1;
        List<GraphicsNode> _nodes2;

        Dictionary<SurfaceBody, SurfaceBody> _surfaceBodies;

        ComponentDefinition _compDef;

        //////////////////////////////////////////////////////////////////////////////////////////////
        // The public static method to start the demo
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        public static void Demo()
        {
            SliceGraphics instance = new SliceGraphics();
            instance.DoDemo();
        }
        
        void DoDemo()
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            _compDef = AdnInventorUtilities.GetCompDefinition(InvApp.ActiveDocument);

            _surfaceBodies = AdnInventorUtilities.GetTransientBodies(_compDef);

            _interactionManager = new AdnInteractionManager(InvApp);

            _interactionManager.Initialize();

            _interactionManager.OnTerminateEvent +=
               new AdnInteractionManager.OnTerminateHandler(OnTerminateEvent);

            _clientGraphicsMng = new AdnClientGraphicsManager(
               InvApp,
               AdnInventorUtilities.AddInGuid);

            _clientGraphicsMng.SetGraphicsSource(_interactionManager.InteractionEvents);

     
            _interactionManager.SelectEvents.SingleSelectEnabled = true;

            _interactionManager.AddSelectionFilter(SelectionFilterEnum.kPartFacePlanarFilter);
            _interactionManager.AddSelectionFilter(SelectionFilterEnum.kWorkPlaneFilter);
                
            _interactionManager.SelectEvents.OnSelect += 
                new SelectEventsSink_OnSelectEventHandler(SelectEvents_OnSelect);

            _interactionManager.Start("Select planar face or workplane: ");
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // First selection will be a planar face or workplane used as slicing plane
        // Subsequent selections will be graphic node, so we switch nodes visibility 
        //   to reverse the normal of slicing plane
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void SelectEvents_OnSelect(
            ObjectsEnumerator JustSelectedEntities, 
            SelectionDeviceEnum SelectionDevice, 
            Point ModelPosition, 
            Point2d ViewPosition, 
            View View)
        {
            if (JustSelectedEntities[1] is GraphicsNode)
            {
                SwitchNodesVisibility();
                return;
            }

            _interactionManager.ClearSelectionFilters();

            SetModelVisibility(false);

            switch (AdnInventorUtilities.InvApplication.ActiveDocument.DocumentType)
            { 
                case DocumentTypeEnum.kAssemblyDocumentObject:

                    foreach (ComponentOccurrence occurrence in _compDef.Occurrences)
                    {
                        occurrence.Visible = false;
                    }
                    break;

                case DocumentTypeEnum.kPartDocumentObject:

                    foreach (KeyValuePair<SurfaceBody, SurfaceBody> pair in _surfaceBodies)
                    {
                        pair.Key.Visible = false;
                    }
                    break;

                default:
                    return;
            }

            Plane plane1 = AdnInventorUtilities.GetPlane(JustSelectedEntities[1]);

            _nodes1 = CreateSlicedNodes(plane1, false);

            Vector normal2 = plane1.Normal.AsVector();
            normal2.ScaleBy(-1.0);

            Plane plane2 = 
                AdnInventorUtilities.InvApplication.TransientGeometry.CreatePlane(
                    plane1.RootPoint, 
                    normal2);

            _nodes2 = CreateSlicedNodes(plane2, true);

            _clientGraphicsMng.UpdateView();
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Create a list of GraphicsNode, sliced by the plane argument and set their visibility.
        // A list of nodes is used here, so we can attribute each node a different render style
        // if needed.
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        List<GraphicsNode> CreateSlicedNodes(Plane plane, bool visibility)
        {
            List<GraphicsNode> nodes = new List<GraphicsNode>();

            foreach (KeyValuePair<SurfaceBody, SurfaceBody> pair in _surfaceBodies)
            {
                SurfaceGraphics surfGraph = _clientGraphicsMng.DrawSurface(pair.Value);

                GraphicsNode node = surfGraph.Parent;

                StyleSourceTypeEnum styleSourceType;
                node.RenderStyle = pair.Key.GetRenderStyle(out styleSourceType);

                nodes.Add(node);
            }

            ObjectCollection slicingPlanes =
                AdnInventorUtilities.InvApplication.TransientObjects.CreateObjectCollection(null);

            slicingPlanes.Add(plane);

            foreach(GraphicsNode node in nodes)
            {
                node.SliceGraphics(true, slicingPlanes, false, null);

                node.Visible = visibility;

                node.Selectable = true;
            }

            return nodes;
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Set visibilty of occurrences or SurfaceBodies depending if assembly or part
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void SetModelVisibility(bool visibility)
        {
            switch (AdnInventorUtilities.InvApplication.ActiveDocument.DocumentType)
            {
                case DocumentTypeEnum.kAssemblyDocumentObject:

                    foreach (ComponentOccurrence occurrence in _compDef.Occurrences)
                    {
                        occurrence.Visible = visibility;
                    }
                    break;

                case DocumentTypeEnum.kPartDocumentObject:

                    foreach (KeyValuePair<SurfaceBody, SurfaceBody> pair in _surfaceBodies)
                    {
                        pair.Key.Visible = visibility;
                    }
                    break;

                default:
                    return;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Switch visibility of graphics nodes
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void SwitchNodesVisibility()
        {
            foreach (GraphicsNode node in _nodes1)
            {
                node.Visible = !node.Visible;
            }

            foreach (GraphicsNode node in _nodes2)
            {
                node.Visible = !node.Visible;
            }
        }

        //////////////////////////////////////////////////////////////////////////////////////////////
        // Trigger final view update when Interaction is terminated
        //
        //////////////////////////////////////////////////////////////////////////////////////////////
        void OnTerminateEvent(object o, AdnInteractionManager.OnTerminateEventArgs e)
        {
            SetModelVisibility(true);

            _clientGraphicsMng.UpdateView();
        }
    }
}
