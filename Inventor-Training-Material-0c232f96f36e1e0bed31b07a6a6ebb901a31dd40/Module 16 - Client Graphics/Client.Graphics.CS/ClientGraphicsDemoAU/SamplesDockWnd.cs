using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using Inventor;
using Autodesk.ADN.Utility.InventorUtils;

namespace ClientGraphicsDemoAU
{
    public partial class SamplesDockWnd : Form
    {
        public SamplesDockWnd(
            Inventor.ApplicationAddInSite addInSite,
            Inventor.DockingStateEnum initialDockingState)
        {
            if (addInSite == null) // We can't build the dockable window without the add-in site object.
                throw new ArgumentNullException("addInSite");

            // Initialize the form with the designer code.
            InitializeComponent();

            // Make sure the components object is created. (The designer doesn't always create it.)
            if (components == null)
                components = new Container();

            // Create the DockableWindow using a managed wrapper and add it to the components collection.
            components.Add(
                new DockableWindowWrapper(addInSite, this, initialDockingState),
                typeof(DockableWindowWrapper).Name);
        }

        public void InitializeSamples()
        {
            _picBox.Image = Resources.AUBand;

            _tvSamples.NodeMouseClick += new TreeNodeMouseClickEventHandler(tvSamples_NodeMouseClick);
            _tvSamples.AfterCollapse += new TreeViewEventHandler(AfterCollapse);
            _tvSamples.AfterExpand += new TreeViewEventHandler(AfterExpand);


            //////////////////////////////////////////////////////////////////////////////////////////////
            // Basic Primitives Samples
            //
            //////////////////////////////////////////////////////////////////////////////////////////////
            TreeNode basicPrimitives = _tvSamples.Nodes.Add("KeyBasic", "Basic Primitives Samples", 0, 0);

            TreeNode s1 = basicPrimitives.Nodes.Add("KeyS1", "Line Graphics", 2, 2);
            s1.Tag = new NodeData("Displays a LineGraphics between points [0, 0, 0] and [1, 1, 1].",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.BasicPrimitivesSamples.BasicPrimitives.LineGraphicsDemo));

            TreeNode s2 = basicPrimitives.Nodes.Add("KeyS2", "Index Set", 2, 2);
            s2.Tag = new NodeData("Displays a LineGraphics using Index and Color Sets.",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.BasicPrimitivesSamples.BasicPrimitives.IndexSetDemo));

            TreeNode s3 = basicPrimitives.Nodes.Add("KeyS3", "Point Graphics", 2, 2);
            s3.Tag = new NodeData("Displays a various PointGraphics using custom bitmap image and Inventor Point RenderStyles.",
                new NodeData.DemoMethod[]{ 
                        new NodeData.DemoMethod(ClientGraphicsDemoAU.BasicPrimitivesSamples.BasicPrimitives.PointGraphicsDemo),
                        new NodeData.DemoMethod(ClientGraphicsDemoAU.BasicPrimitivesSamples.BasicPrimitives.PointGraphicsRenderDemo)
                    });

            TreeNode s4 = basicPrimitives.Nodes.Add("KeyS4", "Text Graphics", 2, 2);
            s4.Tag = new NodeData("Displays a TextGraphics with different styles and properties.",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.BasicPrimitivesSamples.BasicPrimitives.TextGraphicsDemo));

            TreeNode s5 = basicPrimitives.Nodes.Add("KeyS5", "Triangle Strip Graphics", 2, 2);
            s5.Tag = new NodeData("Displays a TriangleStrip Graphics.",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.BasicPrimitivesSamples.BasicPrimitives.TriangleStripGraphicsDemo));

            TreeNode s6 = basicPrimitives.Nodes.Add("KeyS6", "Surface Graphics", 2, 2);
            s6.Tag = new NodeData("Copies SurfaceBody 1 of current part and performs boolean operation to displays result as SurfaceGraphics.",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.BasicPrimitivesSamples.BasicPrimitives.SurfaceGraphicsDemo));


            //////////////////////////////////////////////////////////////////////////////////////////////
            // GraphicsManager Samples
            //
            //////////////////////////////////////////////////////////////////////////////////////////////
            TreeNode manager = _tvSamples.Nodes.Add("KeyManager", "Graphics Manager Samples", 0, 0);

            TreeNode r1 = manager.Nodes.Add("KeyR1", "Line Graphics", 2, 2);
            r1.Tag = new NodeData("Displays a LineGraphics between points [0, 0, 0] and [1, 1, 1]." + System.Environment.NewLine +
            "(This sample is implemented with ClientGraphicsManager)",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.GraphicsManagerSamples.BasicLine.Demo));
            
            TreeNode r2 = manager.Nodes.Add("KeyR2", "LineStrip Graphics", 2, 2);
            r2.Tag = new NodeData("Displays a LineStripGraphics between three vertices selected by the user on the model.",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.GraphicsManagerSamples.LineStrip.Demo));

            TreeNode r3 = manager.Nodes.Add("KeyR3", "Curve Graphics", 2, 2);
            r3.Tag = new NodeData("Displays a CurveGraphics based on edges form surface body selected by user.",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.GraphicsManagerSamples.CurveGraphic.Demo));

            // Not displayed in control
            //TreeNode r4_1 = regular.Nodes.Add("KeyR4_1", "ClientFeature Graphics (Simple)", 2, 2);
            //r4_1.Tag = new NodeData("Displays a TriangleGraphics attached to a ClientFeature. The graphics stored inside the ClientFeature are persisted across sessions.",
            //    new NodeData.DemoMethod(ClientGraphicsDemoAU.GraphicsManagerSamples.ClientFeatureGraphics.Demo));

            TreeNode r4 = manager.Nodes.Add("KeyR4", "ClientFeature Graphics", 2, 2);
            r4.Tag = new NodeData("Displays a TriangleGraphics attached to a ClientFeature. The graphics stored inside the ClientFeature are persisted across sessions" + System.Environment.NewLine  +
                "(This sample is implemented with ClientGraphicsManager)",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.GraphicsManagerSamples.ClientFeatureGraphics.DemoMng));

          
            //////////////////////////////////////////////////////////////////////////////////////////////
            // Interaction Graphics Sample
            //
            //////////////////////////////////////////////////////////////////////////////////////////////
            TreeNode interact = _tvSamples.Nodes.Add("KeyInterac", "Interaction Graphics Samples", 0, 0);

            TreeNode i1 = interact.Nodes.Add("KeyI1", "LineGraphics Interaction", 2, 2);
            i1.Tag = new NodeData("A simple interaction example using a LineGraphics and MouseEvents.",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.InteractionSamples.SimpleInteractionMng.Demo));

            TreeNode i2 = interact.Nodes.Add("KeyI2", "TriangleGraphics Interaction", 2, 2);
            i2.Tag = new NodeData("A simple interaction example using a TriangleGraphics and MouseEvents.",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.InteractionSamples.SimpleInteraction.Demo));

            TreeNode i3 = interact.Nodes.Add("KeyI3", "LineStripGraphics Interaction", 2, 2);
            i3.Tag = new NodeData("An interaction example using a LinStripsGraphics and MouseEvents.",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.InteractionSamples.InteractionLineStrip.Demo));

            TreeNode i4 = interact.Nodes.Add("KeyI4", "Drawing Graphics Interaction", 2, 2);
            i4.Tag = new NodeData("Illutrates use of ClientGraphics in Drawing Sheet, using Interaction and MouseEvents to draw a moving symbol on the drawing.",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.InteractionSamples.DrawingGraphics.Demo));

            TreeNode i5 = interact.Nodes.Add("KeyI5", "CircleJig Interaction", 2, 2);
            i5.Tag = new NodeData("Illutrates use of ClientGraphics in Interaction mode, by drawing circle on the selected surface (planar face or workplane).",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.InteractionSamples.CircleJig.Demo));

            TreeNode i6 = interact.Nodes.Add("KeyI6", "ComponentGraphics Interaction", 2, 2);
            i6.Tag = new NodeData("A more advanced example illustrating the use of ComponentGraphics and MouseEvents." + System.Environment.NewLine +
                "The user can select a part or assembly to insert inside the active part, a Non-Parametric base feature is then created.",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.InteractionSamples.Component.Demo));

            TreeNode i7 = interact.Nodes.Add("KeyI7", "Dimension Graphics", 2, 2);
            i7.Tag = new NodeData("Illutrates use of ClientGraphics to store ClientGraphics based dimensions working in Part or Assembly documents.",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.InteractionSamples.GraphicsDimension.CreateDimensionCmd));

            TreeNode i8 = interact.Nodes.Add("KeyI8", "Slice Graphics Interaction", 2, 2);
            i8.Tag = new NodeData("Illutrates use of slice GraphicsNode functionality.",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.InteractionSamples.SliceGraphics.Demo));

            TreeNode i9 = interact.Nodes.Add("KeyI9", "Dynamic Simulation Graphics", 2, 2);
            i9.Tag = new NodeData("Performs dynamic simulation inside an assembly using transacted or non-transacted ClientGraphics. " + System.Environment.NewLine +
                "The dynamic simulation engine is powered by Bullet, a professional free 3D Game Multiphysics Library, visit http://bulletphysics.org/ for more details about Bullet.",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.InteractionSamples.DynamicSimulationControl.DisplayControl));


            //////////////////////////////////////////////////////////////////////////////////////////////
            // Utilities
            //
            //////////////////////////////////////////////////////////////////////////////////////////////
            TreeNode utilities = _tvSamples.Nodes.Add("KeyUtils", "Graphics Utilities", 0, 0);

            TreeNode u1 = utilities.Nodes.Add("KeyU1", "Delete Manager Graphics", 2, 2);
            u1.Tag = new NodeData("Delete all graphics and data created by ClientGraphics Manager.",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.RegularCGDemo.DeleteGraphics.DeleteManagerGraphics));

            TreeNode u2 = utilities.Nodes.Add("KeyU2", "Delete All Graphics", 2, 2);
            u2.Tag = new NodeData("Delete all graphics and data in active document.",
                new NodeData.DemoMethod(ClientGraphicsDemoAU.RegularCGDemo.DeleteGraphics.DeleteAllGraphics));


            _tvSamples.CollapseAll();
        }

        void AfterExpand(object sender, TreeViewEventArgs e)
        {
            e.Node.ImageIndex = 1;
            e.Node.SelectedImageIndex = 1;
        }

        void AfterCollapse(object sender, TreeViewEventArgs e)
        {
            e.Node.ImageIndex = 0;
            e.Node.SelectedImageIndex = 0;
        }

        void tvSamples_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag is NodeData)
            {
                NodeData nodeData = e.Node.Tag as NodeData;

                _tbDesc.Text = System.Environment.NewLine
                    + "Description:" 
                    + System.Environment.NewLine 
                    + System.Environment.NewLine 
                    + nodeData.Description;

                _bExecute.Enabled = true;
                return;
            }

            _bExecute.Enabled = false;
        }

        private void bExecute_Click(object sender, EventArgs e)
        {
            try
            {
                TreeNode node = _tvSamples.SelectedNode;

                if (node.Tag is NodeData)
                {
                    NodeData nodeData = node.Tag as NodeData;
                    nodeData.Methods();

                    //SetFocus((IntPtr)AdnInventorUtilities.InvApplication.MainFrameHWND);
                }
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Sample " + _tvSamples.SelectedNode.Text + " failed..." +
                    System.Environment.NewLine + "Check the document type is appropriate to run this sample", 
                    "Exception generated!", 
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Exclamation);
            }
        }
    }

    class NodeData
    {
        List<DemoMethod> _methods;

        public string Description;

        public delegate void DemoMethod();

        public void Methods()
        {
            foreach (DemoMethod method in _methods)
            {
                method();
            }
        }

        public NodeData(string description, DemoMethod method)
        {
            Description = description;

            _methods = new List<DemoMethod>();

            _methods.Add(method);
        }

        public NodeData(string description, DemoMethod[] methods)
        {
            Description = description;

            _methods = new List<DemoMethod>();

            _methods.AddRange(methods);
        }
    }
}
