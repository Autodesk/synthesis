using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OGLViewer;

namespace StandaloneViewer
{

    /// <summary>
    /// The control that renders the model and handles mouse control
    /// </summary>
    public partial class RobotViewer : UserControl
    {

        public delegate void RobotViewerEvent(RigidNode_Base node, bool clearExisting);

        public event RobotViewerEvent NodeSelected;

        /// <summary>
        /// Whether or not the GLControl has loaded yet
        /// </summary>
        public bool IsLoaded { get; private set; }

        /// <summary>
        /// Whether or not a model has been loaded into the viewer
        /// </summary>
        public bool ModelLoaded { get; private set; }

        /// <summary>
        /// The width and height of the node selection FBO
        /// </summary>
        public int SELECT_BUFFER_WIDTH = LaunchParams.WindowSize.Width, SELECT_BUFFER_HEIGHT = LaunchParams.WindowSize.Height;

        /// <summary>
        /// The list of nodes on the robot model
        /// </summary>
        private List<RigidNode_Base> nodes;

        /// <summary>
        /// The base node that is actually used to render the model
        /// </summary>
        OGL_RigidNode baseNode;

        /// <summary>
        /// The local keyboard state
        /// </summary>
        private KeyboardState keyboardState;

        /// <summary>
        /// The local mouse state
        /// </summary>
        private MouseState mouseState;

        /// <summary>
        /// The viewer's camera
        /// </summary>
        InventorCamera cam;

        /// <summary>
        /// The speed multiplier for the camera
        /// </summary>
        private float cameraMult = 1.0f;

        /// <summary>
        /// Whether or not the viewer should display camera debug information
        /// </summary>
        /// <remarks>
        /// This information includes:
        /// Camera translation
        /// Camera rotation
        /// Camera mode
        /// </remarks>
        private bool cameraDebug;

        /// <summary>
        /// The position of light 0
        /// </summary>
        static float[] l0_position = { 1000f, -1000f, 1000f, 0f };

        /// <summary>
        /// The position of light 1
        /// </summary>
        static float[] l1_position = { -1000f, 1000f, -1000f, 0f };

        /// <summary>
        /// The diffuse color of the lights
        /// </summary>
        static float[] l_diffuse = { 1f, 1f, 1f, 1f };

        /// <summary>
        /// The specular color of the lights
        /// </summary>
        static float[] l_specular = { .1f, .1f, .1f, .1f };

        /// <summary>
        /// The ambient color of the lights
        /// </summary>
        static float[] ambient = { .125f, .125f, .125f, .125f };

        /// <summary>
        /// The GUID of the selected object
        /// </summary>
        private UInt32 selectedGUID;

        /// <summary>
        /// The currently selected object
        /// </summary>
        private object selectedObject;

        private List<RigidNode_Base> activeObjects;

        /// <summary>
        /// The bindings for the selection texture and FBO;
        /// </summary>
        private int selectTextureHandle, selectFBOHandle;

        /// <summary>
        /// The local copy of the global viewer settings
        /// </summary>
        private ViewerSettingsForm.ViewerSettingsValues settings;

        /// <summary>
        /// Create a new RobotViewer and initialize all components
        /// </summary>
        public RobotViewer()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Load a robot model from a rigid node and list of meshes
        /// </summary>
        /// <param name="node">The node to base the model off of</param>
        /// <param name="meshes">The meshes to render</param>
        public void LoadModel(RigidNode_Base node, List<BXDAMesh> meshes)
        {
            ModelLoaded = false;

            if (node == null || meshes == null) return;

            baseNode = new OGL_RigidNode(node);

            nodes = baseNode.ListAllNodes();

            for (int i = 0; i < meshes.Count; i++)
            {
                ((OGL_RigidNode)nodes[i]).LoadMeshes(meshes[i]);
            }
            Matrix4 mattest = Matrix4.Identity;
            try
            {
                cam.pose = Matrix4.Identity;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString());
                throw;
            }
            activeObjects = new List<RigidNode_Base>();

            ModelLoaded = true;
        }

        /// <summary>
        /// Load settings for the viewer
        /// </summary>
        /// <param name="newSettings">The viewer settings to load</param>
        public void LoadSettings(ViewerSettingsForm.ViewerSettingsValues newSettings)
        {
            settings = newSettings;
            settings.cameraDebugMode = false;

            cameraMult = (float)settings.cameraSensitivity / 3f;
            cameraDebug = settings.cameraDebugMode;
        }

        /// <summary>
        /// Select a joint in the viewer that was selected in the joint editor
        /// </summary>
        /// <remarks>
        /// This method is subscribed to the SelectedJoint event in JointEditorPane
        /// </remarks>
        /// <param name="node">The selected node</param>
        public void SelectJoints(List<RigidNode_Base> selectNodes)
        {
            foreach (RigidNode_Base ns in nodes)
            {
                ((OGL_RigidNode)ns).highlight &= ~OGL_RigidNode.HighlightState.ACTIVE; //Unselect all currently active nodes
            }

            activeObjects.Clear();

            if (!settings.modelHighlight || selectNodes == null) return;

            foreach (RigidNode_Base node in selectNodes)
            {
                if (node is OGL_RigidNode)
                {
                    ((OGL_RigidNode)node).highlight |= OGL_RigidNode.HighlightState.ACTIVE;

                    activeObjects.Add(node);
                }
            }
        }

        /// <summary>
        /// The method called when the GLControl loads
        /// </summary>
        /// <param name="sender">Object sending the event</param>
        /// <param name="e">Event arguments</param>
        private void GLControl1_Load(object sender, EventArgs e)
        {
            cam = new InventorCamera();
            glControl1.KeyDown += Viewer_KeyDown;
            glControl1.KeyUp += Viewer_KeyUp;
            glControl1.MouseDown += Viewer_MouseDown;
            glControl1.MouseUp += Viewer_MouseUp;
            glControl1.MouseMove += Viewer_MouseMoved;
            glControl1.MouseWheel += Viewer_MouseWheel;

            GL.ClearColor(System.Drawing.Color.LightGreen);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.Light1);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Back);
            int j = ShaderLoader.PartShader;//Loadshader

            SetupSelectBuffer();

            Application.Idle += delegate (object send, EventArgs ea)
            {
                while (glControl1.IsIdle)
                {
                    GLControl1_Paint(null, null);
                }
            };
            IsLoaded = true;
        }

        /// <summary>
        /// Set up the node selection texture and FBO
        /// </summary>
        private void SetupSelectBuffer()
        {
            selectTextureHandle = GL.GenTexture();
            GL.BindTexture(TextureTarget.Texture2D, selectTextureHandle);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Nearest);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Clamp);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Clamp);
            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba8, SELECT_BUFFER_WIDTH, SELECT_BUFFER_HEIGHT, 0, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, IntPtr.Zero);
            GL.BindTexture(TextureTarget.Texture2D, 0);

            selectFBOHandle = GL.Ext.GenFramebuffer();
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, selectFBOHandle);
            GL.Ext.FramebufferTexture2D(FramebufferTarget.FramebufferExt, FramebufferAttachment.ColorAttachment0Ext, TextureTarget.Texture2D, selectTextureHandle, 0);

            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
            GL.DrawBuffer(DrawBufferMode.Back);
        }

        /// <summary>
        /// Select nodes that the mouse is hovering over
        /// </summary>
        private void DoSelect()
        {
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, selectFBOHandle);
            GL.DrawBuffer((DrawBufferMode)FramebufferAttachment.ColorAttachment0Ext);

            GL.PushAttrib(AttribMask.ViewportBit);
            {
                GL.Viewport(0, 0, Width, Height);
                GL.Scissor((int)mouseState.lastPos.X, (int)mouseState.lastPos.Y, 1, 1);
                GL.ClearColor(System.Drawing.Color.White);
                GL.Clear(ClearBufferMask.ColorBufferBit);

                RenderInternal(true);

                byte[] pixels = new byte[4];
                GL.ReadPixels((int)mouseState.lastPos.X, Height - (int)mouseState.lastPos.Y, 1, 1, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
                UInt32 nextGUID = SelectManager.ColorToGUID(pixels);
                if (nextGUID != selectedGUID && settings.modelTint && settings.modelHighlight)
                {
                    if (selectedObject != null && selectedObject is OGL_RigidNode)
                    {
                        ((OGL_RigidNode)selectedObject).highlight &= ~OGL_RigidNode.HighlightState.HOVERING;
                    }

                    selectedObject = SelectManager.GetByGUID(nextGUID);
                    if (selectedObject != null)
                    {
                        ((OGL_RigidNode)selectedObject).highlight |= OGL_RigidNode.HighlightState.HOVERING;
                    }
                }

                selectedGUID = nextGUID;

                GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
                GL.ClearColor(System.Drawing.Color.LightGreen);
            }
            GL.PopAttrib();
        }

        /// <summary>
        /// Render the camera overlay
        /// </summary>
        private void RenderOverlay()
        {
            cam.renderOverlay(Width, Height);
        }

        /// <summary>
        /// Render each rigid node with any selection information that may exist
        /// </summary>
        /// <param name="selectState"></param>
        private void RenderInternal(bool selectState = false)
        {
            foreach (RigidNode_Base node in nodes)
            {
                ((OGL_RigidNode)node).Render(selectState, settings.modelHighlightColor, settings.modelTintColor);
            }
        }

        /// <summary>
        /// Render the robot model and paint the GLControl
        /// </summary>
        /// <param name="sender">Object sending the event</param>
        /// <param name="e">Paint event arguments</param>
        private void GLControl1_Paint(object sender, PaintEventArgs e)
        {
            GL.Enable(EnableCap.Lighting);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (!IsLoaded || !ModelLoaded)
            {
                glControl1.SwapBuffers();
                return;
            }

            UpdateCameraMode();
            baseNode.Compute(true);

            // Project
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            double aspect = (double)Width / (double)Height;
            Matrix4d perspective = Matrix4d.CreatePerspectiveFieldOfView(Math.PI / 2, aspect, 0.1, 1000);
            GL.MultMatrix(ref perspective);
            GL.Viewport(0, 0, Width, Height);

            // Transform
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            cam.translate();

            #region LIGHTS
            GL.LightModel(LightModelParameter.LightModelAmbient, ambient);
            GL.Light(LightName.Light0, LightParameter.Position, l0_position);
            GL.Light(LightName.Light0, LightParameter.Diffuse, l_diffuse);
            GL.Light(LightName.Light0, LightParameter.Specular, l_specular);

            GL.Light(LightName.Light1, LightParameter.Position, l1_position);
            GL.Light(LightName.Light1, LightParameter.Diffuse, l_diffuse);
            GL.Light(LightName.Light1, LightParameter.Specular, l_specular);
            #endregion

            DoSelect();
            RenderInternal();

            // Overlay:
            foreach (RigidNode_Base node in nodes)
            {
                if ((((OGL_RigidNode)node).highlight & OGL_RigidNode.HighlightState.ACTIVE) == OGL_RigidNode.HighlightState.ACTIVE)
                {
                    ((OGL_RigidNode)node).RenderDebug(settings.modelDrawAxes);
                }
            }

            #region OVERLAY
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadIdentity();
            GL.Ortho(0, Width, Height, 0, 0, 10);
            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            GL.UseProgram(0);
            GL.Disable(EnableCap.Lighting);
            RenderOverlay();
            #endregion

            glControl1.SwapBuffers();
        }

        /// <summary>
        /// Called when the viewer is resized.
        /// </summary>
        /// <remarks>
        /// Used to setup the select buffer again to make sure that node selection isn't broken
        /// </remarks>
        /// <param name="sender">The object sending the event</param>
        /// <param name="e">The event arguments</param>
        private void RobotViewer_Resize(object sender, EventArgs e)
        {
            if (!IsLoaded) return;

            SELECT_BUFFER_WIDTH = Width;
            SELECT_BUFFER_HEIGHT = Height;

            SetupSelectBuffer();
        }

        /// <summary>
        /// Update the camera's mode
        /// </summary>
        private void UpdateCameraMode()
        {
            if (keyboardState.F4Down || (mouseState.middleButtonDown && keyboardState.LShiftDown))
                cam.currentMode = InventorCamera.Mode.ORBIT;
            else if (keyboardState.F3Down)
                cam.currentMode = InventorCamera.Mode.FINE_ZOOM;
            else if (keyboardState.F2Down || mouseState.middleButtonDown)
                cam.currentMode = InventorCamera.Mode.MOVE;
            else
                cam.currentMode = InventorCamera.Mode.NONE;

            if (cameraDebug)
            {
                Vector3 pos = cam.pose.ExtractTranslation();
                Vector4 rot = cam.pose.ExtractRotation().ToAxisAngle();
            }
        }

        public void HighlightAll()
        {
            for(int i = 1; i < nodes.Count; i++)
            {
                ((OGL_RigidNode)nodes[i]).highlight = OGL_RigidNode.HighlightState.ACTIVE;
            }
        }

        public void FixLimits()
        {
            //foreach(OGL_RigidNode node in baseNode.ListAllNodes())
            //{
            //    if(node.GetSkeletalJoint() is RotationalJoint_Base joint && joint.hasAngularLimit)
            //    {
            //        joint.angularLimitHigh += 1.57f;
            //        joint.angularLimitLow += 1.57f;
            //    }
            //}
        }
        #region INPUT
        /// <summary>
        /// The keyboard's state
        /// </summary>
        private struct KeyboardState
        {
            public bool LShiftDown;
            public bool LControlDown;
            public bool F4Down;
            public bool F3Down;
            public bool F2Down;
        }

        /// <summary>
        /// The mouse's state
        /// </summary>
        private struct MouseState
        {
            public Vector2 lastPos;
            public Vector2 dragStart;
            public Vector3 diffOld;

            public bool leftButtonDown;
            public bool rightButtonDown;
            public bool middleButtonDown;
        }

        /// <summary>
        /// The event called when a key is pressed
        /// </summary>
        /// <param name="source">The source of the event</param>
        /// <param name="args">Keyboard event arguments</param>
        private void Viewer_KeyDown(object source, KeyEventArgs args)
        {
            if (args.KeyCode == Keys.ShiftKey) keyboardState.LShiftDown = true;
            else if (args.KeyCode == Keys.ControlKey) keyboardState.LControlDown = true;
            else if (args.KeyCode == Keys.F2) keyboardState.F2Down = true;
            else if (args.KeyCode == Keys.F3) keyboardState.F3Down = true;
            else if (args.KeyCode == Keys.F4) keyboardState.F4Down = true;
        }

        /// <summary>
        /// The event called when a key is released
        /// </summary>
        /// <param name="source">The source of the event</param>
        /// <param name="args">Keyboard event arguments</param>
        private void Viewer_KeyUp(object source, KeyEventArgs args)
        {
            if (args.KeyCode == Keys.ShiftKey) keyboardState.LShiftDown = false;
            else if (args.KeyCode == Keys.ControlKey) keyboardState.LControlDown = false;
            else if (args.KeyCode == Keys.F2) keyboardState.F2Down = false;
            else if (args.KeyCode == Keys.F3) keyboardState.F3Down = false;
            else if (args.KeyCode == Keys.F4) keyboardState.F4Down = false;
        }

        /// <summary>
        /// The event called when a mouse button is pressed
        /// </summary>
        /// <param name="source">The source of the event</param>
        /// <param name="args">Mouse event arguments</param>
        private void Viewer_MouseDown(object source, MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left) mouseState.leftButtonDown = true;
            else if (args.Button == MouseButtons.Right) mouseState.rightButtonDown = true;
            else if (args.Button == MouseButtons.Middle) mouseState.middleButtonDown = true;

            mouseState.dragStart = new Vector2(args.X, args.Y);

            if (args.Button == MouseButtons.Left && (selectedObject as OGL_RigidNode) != null && ModelLoaded)
            {
                NodeSelected(selectedObject as RigidNode_Base, !keyboardState.LControlDown);
            }
            else if (args.Button == MouseButtons.Left && !keyboardState.LControlDown && ModelLoaded)
            {
                NodeSelected(null, true);
                activeObjects.Clear();
                SelectJoints(activeObjects);
            }
        }

        /// <summary>
        /// The event called when a mouse button is released
        /// </summary>
        /// <param name="source">The source of the event</param>
        /// <param name="args">Mouse event arguments</param>
        private void Viewer_MouseUp(object source, MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left) mouseState.leftButtonDown = false;
            else if (args.Button == MouseButtons.Right) mouseState.rightButtonDown = false;
            else if (args.Button == MouseButtons.Middle) mouseState.middleButtonDown = false;
        }

        /// <summary>
        /// The event called when the mouse wheel is moved
        /// </summary>
        /// <param name="source">The source of the event</param>
        /// <param name="args">Mouse event arguments</param>
        private void Viewer_MouseWheel(object source, MouseEventArgs args)
        {
            cam.offset -= args.Delta / 25f;
        }

        /// <summary>
        /// The event called when the mouse is moved
        /// </summary>
        /// <param name="source">The source of the event</param>
        /// <param name="args">Mouse event arguments</param>
        private void Viewer_MouseMoved(object source, MouseEventArgs args)
        {
            Vector2 mousePos = new Vector2(args.X, args.Y);
            Vector2 deltaPos = mousePos - mouseState.lastPos;

            if (cam.currentMode == InventorCamera.Mode.ORBIT)
            {
                float radius = Math.Min(Width, Height) * 0.3f;

                Vector3 diffStart = new Vector3(mouseState.dragStart.X - (Width / 2), mouseState.dragStart.Y - (Height / 2), 0);
                float diffLen = diffStart.LengthFast;
                if (diffLen > radius) //Rotating
                {
                    Vector3 diffCurrent = new Vector3(args.X - (Width / 2), args.Y - (Height / 2), 0);
                    diffCurrent.NormalizeFast();
                    float dir = Math.Sign((diffCurrent.X - mouseState.diffOld.X) * (diffCurrent.Y * mouseState.diffOld.Y) * (args.Y - (Height / 2)));
                    float angle = (float)Math.Acos(Vector3.Dot(diffCurrent, mouseState.diffOld));
                    mouseState.diffOld = diffCurrent;

                    cam.pose *= Matrix4.CreateRotationZ(cameraMult * dir * angle * 0.1f);
                }
                else //Orbiting
                {
                    Vector3 rotationAxis = new Vector3(deltaPos.Y, deltaPos.X, 0);

                    if (rotationAxis != Vector3.Zero)
                        cam.pose *= Matrix4.CreateFromAxisAngle(rotationAxis, (cameraMult * rotationAxis.LengthFast) / 100.0f);
                }
            }
            else if (cam.currentMode == InventorCamera.Mode.FINE_ZOOM)
            {
                cam.offset += cameraMult * deltaPos.Y;
            }
            else if (cam.currentMode == InventorCamera.Mode.MOVE)
            {
                cam.pose *= Matrix4.CreateTranslation(cameraMult * deltaPos.X / 10f, cameraMult * -deltaPos.Y / 10f, 0);
            }

            mouseState.lastPos = mousePos;
        }
        #endregion
    }

}
