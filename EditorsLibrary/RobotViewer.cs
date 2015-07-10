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

namespace EditorsLibrary
{
    public partial class RobotViewer : UserControl
    {

        public bool isLoaded { get; private set; }
        public bool modelLoaded { get; private set; }

        private const int SELECT_BUFFER_WIDTH = 1920, SELECT_BUFFER_HEIGHT = 1080;

        private static double horizontalTan = Math.Tan(25.0 * 3.14 / 180.0);

        private List<RigidNode_Base> nodes;
        OGL_RigidNode baseNode;

        InventorCamera cam;

        static float[] l0_position = { 1000f, -1000f, 1000f, 0f };
        static float[] l1_position = { -1000f, 1000f, -1000f, 0f };
        static float[] l_diffuse = { 1f, 1f, 1f, 1f };
        static float[] l_specular = { .1f, .1f, .1f, .1f };
        static float[] ambient = { .125f, .125f, .125f, .125f };

        // Select info
        private UInt32 selectedGUID;
        private object selectedObject;
        private int selectTextureHandle, selectFBOHandle;

        public RobotViewer()
        {
            InitializeComponent();
        }

        private void setupSelectBuffer()
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

        private void doSelect()
        {
            GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, selectFBOHandle);
            GL.DrawBuffer((DrawBufferMode)FramebufferAttachment.ColorAttachment0Ext);
            GL.PushAttrib(AttribMask.ViewportBit);
            GL.Viewport(0, 0, Width, Height);
            GL.Scissor((int) mouseState.lastPos.X, (int) mouseState.lastPos.Y, 1, 1);
            GL.ClearColor(System.Drawing.Color.White);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            renderInternal(true);

            byte[] pixels = new byte[4];
            GL.ReadPixels((int) mouseState.lastPos.X, Height - (int) mouseState.lastPos.Y, 1, 1, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
            UInt32 nextGUID = SelectManager.ColorToGUID(pixels);
            if (nextGUID != selectedGUID)
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
            GL.ClearColor(System.Drawing.Color.Black);
            GL.PopAttrib();
        }

        private void renderOverlay()
        {
            cam.renderOverlay(Width, Height);
        }

        private void renderInternal(bool selectState = false)
        {
            foreach (RigidNode_Base node in nodes)
            {
                ((OGL_RigidNode) node).render(selectState);
            }
        }

        public void loadModel(RigidNode_Base node, List<BXDAMesh> meshes)
        {
            baseNode = new OGL_RigidNode(node);

            nodes = baseNode.ListAllNodes();

            for (int i = 0; i < meshes.Count; i++)
            {
                ((OGL_RigidNode) nodes[i]).loadMeshes(meshes[i]);
            }

            modelLoaded = true;
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            cam = new InventorCamera();
            KeyDown += viewer_KeyDown;
            KeyUp += viewer_KeyUp;
            MouseDown += viewer_MouseDown;
            MouseMove += viewer_MouseMoved;
            MouseWheel += viewer_MouseWheel;

            GL.ClearColor(System.Drawing.Color.Black);
            GL.Enable(EnableCap.Lighting);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.Light1);
            GL.Enable(EnableCap.DepthTest);
            GL.Enable(EnableCap.CullFace);
            GL.CullFace(CullFaceMode.Front);
            int j = ShaderLoader.PartShader;//Loadshader

            setupSelectBuffer();

            Application.Idle += delegate(object send, EventArgs ea)
            {
                while (glControl1.IsIdle)
                {
                    glControl1_Paint(null, null);
                }
            };
            isLoaded = true;
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            GL.Enable(EnableCap.Lighting);
            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            if (!isLoaded || !modelLoaded)
            {
                glControl1.SwapBuffers();
                return;
            }

            updateCameraMode();
            baseNode.compute();

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
            {
                GL.LightModel(LightModelParameter.LightModelAmbient, ambient);
                GL.Light(LightName.Light0, LightParameter.Position, l0_position);
                GL.Light(LightName.Light0, LightParameter.Diffuse, l_diffuse);
                GL.Light(LightName.Light0, LightParameter.Specular, l_specular);

                GL.Light(LightName.Light1, LightParameter.Position, l1_position);
                GL.Light(LightName.Light1, LightParameter.Diffuse, l_diffuse);
                GL.Light(LightName.Light1, LightParameter.Specular, l_specular);
            }
            #endregion
            
            doSelect();
            renderInternal();
            
            // Overlay:
            foreach (RigidNode_Base node in nodes)
            {
                if ((((OGL_RigidNode)node).highlight & OGL_RigidNode.HighlightState.ACTIVE) == OGL_RigidNode.HighlightState.ACTIVE)
                {
                    ((OGL_RigidNode)node).renderDebug();
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
            renderOverlay();
            #endregion
            
            glControl1.SwapBuffers();
        }

        private void updateCameraMode()
        {
            if (keyboardState.F4Down || (mouseState.middleButtonDown && keyboardState.LShiftDown))
                cam.currentMode = InventorCamera.Mode.ORBIT;
            else if (keyboardState.F3Down)
                cam.currentMode = InventorCamera.Mode.FINE_ZOOM;
            else if (keyboardState.F2Down || mouseState.middleButtonDown)
                cam.currentMode = InventorCamera.Mode.MOVE;
            else
                cam.currentMode = InventorCamera.Mode.NONE;
        }

        #region INPUT
        private struct KeyboardState
        {
            public bool LShiftDown;
            public bool F4Down;
            public bool F3Down;
            public bool F2Down;
        }

        private struct MouseState
        {
            public Vector2 lastPos;
            public Vector2 dragStart;
            public Vector3 diffOld;
            public float offset;

            public bool leftButtonDown;
            public bool rightButtonDown;
            public bool middleButtonDown;
        }

        private KeyboardState keyboardState;
        private MouseState mouseState;

        private void viewer_KeyDown(object source, KeyEventArgs args)
        {
            if (args.KeyCode == Keys.LShiftKey) keyboardState.LShiftDown = true;
            else if (args.KeyCode == Keys.F2) keyboardState.F2Down = true;
            else if (args.KeyCode == Keys.F3) keyboardState.F3Down = true;
            else if (args.KeyCode == Keys.F4) keyboardState.F4Down = true;
        }

        private void viewer_KeyUp(object source, KeyEventArgs args)
        {
            if (args.KeyCode == Keys.LShiftKey) keyboardState.LShiftDown = false;
            else if (args.KeyCode == Keys.F2) keyboardState.F2Down = false;
            else if (args.KeyCode == Keys.F3) keyboardState.F3Down = false;
            else if (args.KeyCode == Keys.F4) keyboardState.F4Down = false;
        }

        private void viewer_MouseDown(object source, MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left) mouseState.leftButtonDown = true;
            else if (args.Button == MouseButtons.Right) mouseState.rightButtonDown = true;
            else if (args.Button == MouseButtons.Middle) mouseState.middleButtonDown = true;

            mouseState.dragStart = new Vector2(args.X, args.Y);
        }

        private void viewer_MouseUp(object source, MouseEventArgs args)
        {
            if (args.Button == MouseButtons.Left) mouseState.leftButtonDown = false;
            else if (args.Button == MouseButtons.Right) mouseState.rightButtonDown = false;
            else if (args.Button == MouseButtons.Middle) mouseState.middleButtonDown = false;
        }

        private void viewer_MouseWheel(object source, MouseEventArgs args)
        {
            mouseState.offset -= args.Delta * 10f;
        }

        private void viewer_MouseMoved(object source, MouseEventArgs args)
        {
            Vector2 mousePos = new Vector2(args.X, args.Y);
            Vector2 deltaPos = mousePos - mouseState.lastPos;

            if ((mouseState.leftButtonDown && keyboardState.F4Down) || (mouseState.middleButtonDown && keyboardState.LShiftDown))
            {
                float radius = Math.Min(Width, Height) * 0.3f;
                Vector3 diffStart = new Vector3(mouseState.dragStart.X - (Width / 2), mouseState.dragStart.Y - (Height / 2), 0);
                float diffLen = diffStart.LengthFast;
                if (diffLen > radius)
                {
                    Vector3 diffCurrent = new Vector3(args.X - (Width / 2), args.Y - (Height / 2), 0);
                    diffCurrent.NormalizeFast();
                    float dir = Math.Sign((diffCurrent.X - mouseState.diffOld.X) * (diffCurrent.Y * mouseState.diffOld.Y) * (args.Y - (Height / 2)));
                    float angle = (float)Math.Acos(Vector3.Dot(diffCurrent, mouseState.diffOld));
                    mouseState.diffOld = diffCurrent;
                    // Rotating
                    cam.pose *= Matrix4.CreateRotationZ(dir * angle * 0.1f);
                }
                else
                {
                    // Orbiting.
                    Vector3 rotationAxis = new Vector3(deltaPos);
                    cam.pose *= Matrix4.CreateFromAxisAngle(rotationAxis, rotationAxis.LengthFast / 100.0f);
                }
            }
            else if (mouseState.leftButtonDown && keyboardState.F3Down)
            {
                mouseState.offset += deltaPos.Y;
            }
            else if ((mouseState.leftButtonDown && keyboardState.F2Down) || mouseState.middleButtonDown)
            {
                cam.pose *= Matrix4.CreateTranslation(deltaPos.X / 25f, -deltaPos.Y / 25f, 0);
            }

            mouseState.lastPos = mousePos;
        }
        #endregion

    }

}
