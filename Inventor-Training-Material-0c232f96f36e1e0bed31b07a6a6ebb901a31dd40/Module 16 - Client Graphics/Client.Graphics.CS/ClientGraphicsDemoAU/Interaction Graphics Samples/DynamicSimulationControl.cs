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
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Inventor;
using BulletWrapper;
using Autodesk.ADN.Utility.WinUtils;
using Autodesk.ADN.Utility.Graphics;
using Autodesk.ADN.Utility.Interaction;
using Autodesk.ADN.Utility.InventorUtils;

namespace ClientGraphicsDemoAU.InteractionSamples
{
    //////////////////////////////////////////////////////////////////////////////////////////////
    // Description: Performs dynamic simulation inside an assembly using transacted
    //              or non-transacted ClientGraphics. 
    //              The dynamic simulation engine is powered by Bullet, 
    //              a professional free 3D Game Multiphysics Library, 
    //              visit http://bulletphysics.org/ for more details about Bullet.
    //
    //////////////////////////////////////////////////////////////////////////////////////////////
    public partial class DynamicSimulationControl : Form
    {

        static bool _bActive;

        DynamicsWorld _dynamicsWorld;

        Dictionary<ComponentOccurrence, RigidBody> _mapOccurrencesToBodies;

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // 
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private DynamicSimulationControl()
        {
            InitializeComponent();

            _picBox.Image = Resources.bullet;

            _mapOccurrencesToBodies = new Dictionary<ComponentOccurrence, RigidBody>();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // 
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        public static void DisplayControl()
        {
            DynamicSimulationControl control = new DynamicSimulationControl();

            control.Show(new WindowWrapper((IntPtr)AdnInventorUtilities.InvApplication.MainFrameHWND));
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // 
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Initialize()
        { 
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            if(!(InvApp.ActiveDocument is AssemblyDocument))
                return;

	        AssemblyDocument doc = InvApp.ActiveDocument as AssemblyDocument;

            _dynamicsWorld = new DynamicsWorld();

            _mapOccurrencesToBodies.Clear();

	        //create dynamic bodies

            double[] transfo = new double[16];

	        for(int idx=1; idx < doc.ComponentDefinition.Occurrences.Count; ++idx)   
	        {
                ComponentOccurrence occurrence = doc.ComponentDefinition.Occurrences[idx];

                PartComponentDefinition compDef = occurrence.Definition as PartComponentDefinition;

                if (occurrence.Grounded)
                {

                }
                else
                {
                    occurrence.Transformation.GetMatrixData(ref transfo);

                    int vertexCount;
                    int facetCount;
                    double[] vertexCoords = new double[]{};
                    double[] normals = new double[]{};
                    int[] indices = new int[]{};

                    compDef.SurfaceBodies[1].CalculateFacets(0.01, 
                        out vertexCount, 
                        out facetCount, 
                        out vertexCoords,
                        out normals, 
                        out indices);

                    RigidBody body = new RigidBody(facetCount, vertexCoords, normals, indices, occurrence.MassProperties.Mass, transfo);

                    ValueTypeEnum type;
                    int vx = 0, vy = 0, vz = 0;
                    object ovx = null, ovy = null, ovz = null;

                    if (AdnInventorUtilities.ReadAttribute((object)occurrence, "Simulation", "xVelInit", out ovx, out type))
                        vx = (int)ovx;

                    if(AdnInventorUtilities.ReadAttribute((object)occurrence, "Simulation", "yVelInit", out ovy, out type))
                        vy = (int)ovy;

                    if(AdnInventorUtilities.ReadAttribute((object)occurrence, "Simulation", "zVelInit", out ovz, out type))
                        vz = (int)ovz;

                    body.SetLinearVelocity(vx, vy, vz);

                    _mapOccurrencesToBodies.Add(occurrence, body);

                    _dynamicsWorld.AddRigidBody(body);
                }
	        }

            ComponentOccurrence groundOcc = 
                doc.ComponentDefinition.Occurrences[doc.ComponentDefinition.Occurrences.Count];

            double[] pos = new double[]
            {
                groundOcc.Transformation.Translation.X,
                groundOcc.Transformation.Translation.Y,
                groundOcc.Transformation.Translation.Z
            };

            RigidBody groundBody = new RigidBody(pos);

            _dynamicsWorld.AddRigidBody(groundBody);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Direct Simulation, no ClientGraphics involved only moving occurrences in assembly context
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void Run()
        {
            Initialize();

            System.Windows.Forms.Application.AddMessageFilter(new MsgFilter());

            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            AssemblyDocument doc = InvApp.ActiveDocument as AssemblyDocument;
            AssemblyComponentDefinition compDef = doc.ComponentDefinition;

            Inventor.View view = InvApp.ActiveView;

            InvApp.UserInterfaceManager.UserInteractionDisabled = true;

            Transaction Tx = InvApp.TransactionManager.StartGlobalTransaction(doc as _Document, "Simulation");

            ObjectCollection colOccurrences = InvApp.TransientObjects.CreateObjectCollection(null);
            ObjectCollection colTransformations = InvApp.TransientObjects.CreateObjectCollection(null);

            foreach (ComponentOccurrence occurrence in compDef.Occurrences)
            {
                colOccurrences.Add(occurrence);
                colTransformations.Add(occurrence.Transformation);
            }

            AdnTimer timer = new AdnTimer();

            double[] transfo = new double[16];

            _bActive = true;

            while (_bActive)
            {
                System.Windows.Forms.Application.DoEvents();

                double dT = timer.ElapsedSeconds;

                _dynamicsWorld.StepSimulation(dT, 10);

                int idx = 1;

                foreach (RigidBody body in _mapOccurrencesToBodies.Values)
                {
                    body.GetWorldTransform(ref transfo);

                    Matrix matrix = colTransformations[idx] as Matrix;

                    matrix.PutMatrixData(ref transfo);

                    ++idx;
                }

			    compDef.TransformOccurrences(colOccurrences, colTransformations, true); 

			    view.Update();

                ComputeFrameRate(dT);
            }

            _dynamicsWorld.CleanUp();

            Tx.End();

            InvApp.UserInterfaceManager.UserInteractionDisabled = false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // ClientGraphics simulation with or without transacting mode
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void RunWithGraphics(bool transacting)
        {
            Initialize();

            System.Windows.Forms.Application.AddMessageFilter(new MsgFilter());

            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            AssemblyDocument doc = InvApp.ActiveDocument as AssemblyDocument;
            AssemblyComponentDefinition compDef = doc.ComponentDefinition;

            InvApp.UserInterfaceManager.UserInteractionDisabled = true;

            Transaction Tx = InvApp.TransactionManager.StartGlobalTransaction(doc as _Document, "Simulation");

            Matrix matrix = InvApp.TransientGeometry.CreateMatrix();

            AdnClientGraphicsManager clientGraphicsMng = new AdnClientGraphicsManager(
                InvApp,
                AdnInventorUtilities.AddInGuid);

            clientGraphicsMng.SetGraphicsSource(doc as Document);

            clientGraphicsMng.Transacting = transacting;

            List<GraphicsNode> nodes = new List<GraphicsNode>();

            foreach (ComponentOccurrence occurrence in compDef.Occurrences)
            {
                SurfaceBody body = occurrence.Definition.SurfaceBodies[1];

                SurfaceGraphics surfGraph = clientGraphicsMng.DrawSurface(body, null);

                GraphicsNode node = surfGraph.Parent;

                if(occurrence.RenderStyle != null)
                    node.RenderStyle = occurrence.RenderStyle;

                nodes.Add(node);

                occurrence.Visible = false;
            }

            AdnTimer timer = new AdnTimer();

            double[] transfo = new double[16];

            _bActive = true;

            while (_bActive)
            {
                System.Windows.Forms.Application.DoEvents();

                double dT = timer.ElapsedSeconds;

                _dynamicsWorld.StepSimulation(dT, 10);

                int idx = 0;

                foreach (RigidBody body in _mapOccurrencesToBodies.Values)
                {
                    body.GetWorldTransform(ref transfo);

                    matrix.PutMatrixData(ref transfo);

                    nodes[idx].Transformation = matrix;

                    ++idx;
                }

                clientGraphicsMng.UpdateView();

                ComputeFrameRate(dT);
            }

            clientGraphicsMng.DeleteGraphics(doc as Document, true);

            _dynamicsWorld.CleanUp();

            foreach (ComponentOccurrence occurrence in compDef.Occurrences)
            {
                occurrence.Visible = true;
            }

            Tx.End();

            InvApp.UserInterfaceManager.UserInteractionDisabled = false;
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // ClientGraphics simulation running within InteractionEvents using overlay or preview graphics
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void RunWithInteraction(bool overlay)
        {
            Initialize();

            System.Windows.Forms.Application.AddMessageFilter(new MsgFilter());

            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            AssemblyDocument doc = InvApp.ActiveDocument as AssemblyDocument;
            AssemblyComponentDefinition compDef = doc.ComponentDefinition;

            Inventor.View view = InvApp.ActiveView;

            Matrix matrix = InvApp.TransientGeometry.CreateMatrix();

            AdnInteractionManager interactionManager = new AdnInteractionManager(InvApp);

            interactionManager.Initialize();

            AdnClientGraphicsManager clientGraphicsMng = new AdnClientGraphicsManager(
                InvApp,
                AdnInventorUtilities.AddInGuid);

            clientGraphicsMng.InteractionGraphicsMode = (overlay ?
                AdnInteractionGraphicsModeEnum.kOverlayGraphics :
                AdnInteractionGraphicsModeEnum.kPreviewGraphics);

            foreach (ComponentOccurrence occurrence in compDef.Occurrences)
            {
                occurrence.Visible = false;
            }

            interactionManager.Start("Simulation with InteractionEvents");

            clientGraphicsMng.SetGraphicsSource(interactionManager.InteractionEvents);

            List<GraphicsNode> nodes = new List<GraphicsNode>();

            foreach (ComponentOccurrence occurrence in compDef.Occurrences)
            {
                SurfaceBody body = occurrence.Definition.SurfaceBodies[1];

                SurfaceGraphics surfGraph = clientGraphicsMng.DrawSurface(body, null);

                GraphicsNode node = surfGraph.Parent;

                if (occurrence.RenderStyle != null)
                    node.RenderStyle = occurrence.RenderStyle;

                nodes.Add(node);
            }
            
            AdnTimer timer = new AdnTimer();

            double[] transfo = new double[16];

            _bActive = true;

            while (_bActive)
            {
                System.Windows.Forms.Application.DoEvents();

                double dT = timer.ElapsedSeconds;

                _dynamicsWorld.StepSimulation(dT, 10);

                int idx = 0;

                foreach (RigidBody body in _mapOccurrencesToBodies.Values)
                {
                    body.GetWorldTransform(ref transfo);

                    matrix.PutMatrixData(ref transfo);

                    nodes[idx].Transformation = matrix;

                    ++idx;
                }

                clientGraphicsMng.UpdateView();

                ComputeFrameRate(dT);
            }

            _dynamicsWorld.CleanUp();

            interactionManager.Stop();

            foreach (ComponentOccurrence occurrence in compDef.Occurrences)
            {
                occurrence.Visible = true;
            }
        }
        
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Message hook to intercept escape keydown and terminate the simulation
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        class MsgFilter : System.Windows.Forms.IMessageFilter
        {
            const int WM_KEYDOWN = 0x0100;

            public bool PreFilterMessage(ref System.Windows.Forms.Message msg)
            {
                if (msg.Msg == WM_KEYDOWN)
                {
                    if (msg.WParam == (IntPtr)(27))
                    {
                        _bActive = false;

                        return true;
                    }
                }

                return false;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Run direct simulation button click
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void bRun_Click(object sender, EventArgs e)
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            if (InvApp.ActiveDocument == null || !(InvApp.ActiveDocument is AssemblyDocument))
            {
                System.Windows.Forms.MessageBox.Show(
                    "An Assembly Document must be active...",
                    "Invalid Document Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);

                return;
            }

            Run();
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  Run with graphics simulation button click
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void bRunGraphics_Click(object sender, EventArgs e)
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            if (InvApp.ActiveDocument == null || !(InvApp.ActiveDocument is AssemblyDocument))
            {
                System.Windows.Forms.MessageBox.Show(
                    "An Assembly Document must be active...",
                    "Invalid Document Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);

                return;
            }

            RunWithGraphics(cbTransacting.Checked);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        //  Run with interaction simulation button click
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void bRunInteract_Click(object sender, EventArgs e)
        {
            Inventor.Application InvApp = AdnInventorUtilities.InvApplication;

            if (InvApp.ActiveDocument == null || !(InvApp.ActiveDocument is AssemblyDocument))
            {
                System.Windows.Forms.MessageBox.Show(
                    "An Assembly Document must be active...",
                    "Invalid Document Error",
                    System.Windows.Forms.MessageBoxButtons.OK,
                    System.Windows.Forms.MessageBoxIcon.Information);

                return;
            }

            RunWithInteraction(rbOverlay.Checked);
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Display Frame per Seconds (FPS) in Form Ccption bar
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private double _fpsElapsed = 0.0;
        private int _fps = 0;

        private void ComputeFrameRate(double dT)
        {
            _fpsElapsed += dT;

            _fps++;

            if (_fpsElapsed > 1.0)
            {
                _fpsElapsed = 0.0;

                this.Text = "Dynamic Simulation [FPS: " + _fps.ToString() + "]";

                _fps = 0;
            }
        }

        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        // Update overlay/preview graphics radio buttons
        //
        ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
        private void rbPreview_CheckedChanged(object sender, EventArgs e)
        {
            rbOverlay.Checked = !rbPreview.Checked;
        }

        private void rbOverlay_CheckedChanged(object sender, EventArgs e)
        {
            rbPreview.Checked = !rbOverlay.Checked;
        }
    }
}
