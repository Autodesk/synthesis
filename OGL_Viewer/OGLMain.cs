using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK;
using System.Collections.Generic;

/// <summary>
/// 
/// </summary>
public class OGL_Viewer : GameWindow
{
    private const int SELECT_BUFFER_WIDTH = 1920, SELECT_BUFFER_HEIGHT = 1080;

    private static double horizontalTan = Math.Tan(25.0 * 3.14 / 180.0);

    private OGL_RigidNode baseNode;
    private List<RigidNode_Base> nodes;

    InventorCamera cam;

    static float[] l0_position = { 1000f, -1000f, 1000f, 0f };
    static float[] l1_position = { -1000f, 1000f, -1000f, 0f };
    static float[] l_diffuse = { 1f, 1f, 1f, 1f };
    static float[] l_specular = { .1f, .1f, .1f, .1f };
    static float[] ambient = { .125f, .125f, .125f, .125f };

    private ControlGroups editorGUI;

    // Select info
    private int mouseX, mouseY;
    private UInt32 selectedGUID;
    private object selectedObject;
    private int selectTextureHandle, selectFBOHandle;

    private void loadSkeleton()
    {
        RigidNode_Base.NODE_FACTORY = delegate()
        {
            return new OGL_RigidNode();
        };
        editorGUI = new ControlGroups();
        RigidNode_Base skeleton = BXDJSkeleton.ReadSkeleton("C:/Users/t_millw/Downloads/Skeletons/TestBotMain_Skeleton/skeleton.bxdj");
        baseNode = (OGL_RigidNode) skeleton;
        nodes = skeleton.ListAllNodes();
        editorGUI.SetSkeleton(skeleton);
        editorGUI.Show();
        editorGUI.jointPane.SelectedJoint += delegate(RigidNode_Base node)
        {
            foreach (RigidNode_Base ns in nodes)
            {
                ((OGL_RigidNode) ns).highlight &= ~OGL_RigidNode.HighlightState.ACTIVE;
            }
            if (node is OGL_RigidNode)
            {
                ((OGL_RigidNode) node).highlight |= OGL_RigidNode.HighlightState.ACTIVE;
            }
        };
        foreach (RigidNode_Base node in nodes)
        {
            ((OGL_RigidNode) node).loadMeshes("C:/Users/t_millw/Downloads/Skeletons/TestBotMain_Skeleton/" + node.modelFileName);
        }
    }

    public OGL_Viewer()
        : base(1366, 768, new GraphicsMode(32, 0, 0, 4), "Skeleton Viewer")
    {
     //   base.X = 1920;
        MouseMove += (object o, OpenTK.Input.MouseMoveEventArgs e) =>
        {
            mouseX = e.X;
            mouseY = e.Y;
        };
        MouseDown += (object o, OpenTK.Input.MouseButtonEventArgs e) =>
        {
            if (e.Button == OpenTK.Input.MouseButton.Left && selectedObject != null && selectedObject is RigidNode_Base)
            {
                editorGUI.jointPane.SelectJoint((RigidNode_Base) selectedObject);
            }
        };
        cam = new InventorCamera();
        KeyDown += cam.keyStateChange;
        KeyUp += cam.keyStateChange;
        MouseDown += cam.mouseDown;
        MouseMove += cam.mouseMoved;
        MouseWheel += cam.mouseWheel;
    }

    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        loadSkeleton();
        Console.WriteLine("Loaded");

        GL.ClearColor(System.Drawing.Color.Black);
        GL.Enable(EnableCap.Lighting);
        GL.Enable(EnableCap.Light0);
        GL.Enable(EnableCap.Light1);
        GL.Enable(EnableCap.DepthTest);
        int j = ShaderLoader.PartShader;//Loadshader

        setupSelectBuffer();
    }

    private void setupSelectBuffer()
    {
        selectTextureHandle = GL.GenTexture();
        GL.BindTexture(TextureTarget.Texture2D, selectTextureHandle);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int) TextureMinFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int) TextureMagFilter.Nearest);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int) TextureWrapMode.Clamp);
        GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int) TextureWrapMode.Clamp);
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
        GL.DrawBuffer((DrawBufferMode) FramebufferAttachment.ColorAttachment0Ext);
        GL.PushAttrib(AttribMask.ViewportBit);
        GL.Viewport(0, 0, Width, Height);
        GL.Scissor(mouseX, mouseY, 1, 1);
        GL.ClearColor(System.Drawing.Color.White);
        GL.Clear(ClearBufferMask.ColorBufferBit);
        renderInternal(true);

        byte[] pixels = new byte[4];
        GL.ReadPixels(mouseX, Height - mouseY, 1, 1, OpenTK.Graphics.OpenGL.PixelFormat.Rgba, PixelType.UnsignedByte, pixels);
        UInt32 nextGUID = SelectManager.ColorToGUID(pixels);
        if (nextGUID != selectedGUID)
        {
            if (selectedObject != null && selectedObject is OGL_RigidNode)
            {
                ((OGL_RigidNode) selectedObject).highlight &= ~OGL_RigidNode.HighlightState.HOVERING;
            }
            selectedObject = SelectManager.GetByGUID(nextGUID);
            if (selectedObject != null)
            {
                ((OGL_RigidNode) selectedObject).highlight |= OGL_RigidNode.HighlightState.HOVERING;
            }
        }
        selectedGUID = nextGUID;

        GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
        GL.ClearColor(System.Drawing.Color.Black);
        GL.PopAttrib();
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        GL.Enable(EnableCap.Lighting);
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        // Project
        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadIdentity();
        double aspect = (double) Height / (double) Width;
        GL.Frustum(-horizontalTan, horizontalTan, aspect
                * -horizontalTan, aspect * horizontalTan, 1, 100000);
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
        //GL.Disable(EnableCap.DepthTest);
        foreach (RigidNode_Base node in nodes)
        {
            if ((((OGL_RigidNode) node).highlight & OGL_RigidNode.HighlightState.ACTIVE) == OGL_RigidNode.HighlightState.ACTIVE)
            {
                ((OGL_RigidNode) node).renderDebug();
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

        //GL.Enable(EnableCap.DepthTest);
        SwapBuffers();
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

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
        baseNode.compute();
        var keyboard = OpenTK.Input.Keyboard.GetState();
        if (keyboard[OpenTK.Input.Key.Escape])
            Exit();
    }

    protected override void OnUnload(EventArgs e)
    {
        ShaderLoader.PartShader = 0;
    }

    public static void Main(string[] args)
    {
        using (OGL_Viewer viewer = new OGL_Viewer())
        {
            viewer.Run(60);
        }
    }
}