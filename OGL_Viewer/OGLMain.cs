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

    Camera3rdPerson cam = new Camera3rdPerson();

    static float[] l0_position = { 1000f, -1000f, 1000f, 0f };
    static float[] l1_position = { -1000f, 1000f, -1000f, 0f };
    static float[] l_diffuse = { 1f, 1f, 1f, 1f };
    static float[] l_specular = { .1f, .1f, .1f, .1f };
    static float[] ambient = { .125f, .125f, .125f, .125f };

    // Select info
    private int mouseX, mouseY;
    private UInt32 selectedGUID;
    private OGL_RigidNode selectedNode;
    private int selectTextureHandle, selectFBOHandle;

    private void loadSkeleton()
    {
        RigidNode_Base.NODE_FACTORY = delegate()
        {
            return new OGL_RigidNode();
        };
        ControlGroups groups = new ControlGroups();
        RigidNode_Base skeleton = BXDJSkeleton.ReadSkeleton("C:/Users/t_millw/Downloads/Skeletons/TestBotMain_Skeleton/skeleton.bxdj");
        baseNode = (OGL_RigidNode) skeleton;
        nodes = skeleton.ListAllNodes();
        groups.SetSkeleton(skeleton);
        //groups.Show();
        groups.jointPane.SelectedJoint += delegate(RigidNode_Base node)
        {
            foreach (RigidNode_Base ns in nodes)
            {
                ((OGL_RigidNode) ns).highlight = false;
            }
            if (node is OGL_RigidNode)
            {
                ((OGL_RigidNode) node).highlight = true;
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
        base.X = 1920;
        MouseMove += (object o, OpenTK.Input.MouseMoveEventArgs e) =>
        {
            mouseX = e.X;
            mouseY = e.Y;
        };
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
        UInt32 nextGUID = BitConverter.ToUInt32(pixels, 0);
        if (nextGUID != selectedGUID)
        {
            if (selectedNode != null)
            {
                selectedNode.highlight = false;
            }
            selectedNode = OGL_RigidNode.GetNodeByGUID(nextGUID);
            if (selectedNode != null)
            {
                selectedNode.highlight = true;
            }
        }
        selectedGUID = nextGUID;

        GL.Ext.BindFramebuffer(FramebufferTarget.FramebufferExt, 0);
        GL.ClearColor(System.Drawing.Color.Black);
        GL.PopAttrib();
    }

    protected override void OnRenderFrame(FrameEventArgs e)
    {
        GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

        GL.LoadIdentity();
        cam.translate();

        {
            GL.LightModel(LightModelParameter.LightModelAmbient, ambient);
            GL.Light(LightName.Light0, LightParameter.Position, l0_position);
            GL.Light(LightName.Light0, LightParameter.Diffuse, l_diffuse);
            GL.Light(LightName.Light0, LightParameter.Specular, l_specular);

            GL.Light(LightName.Light1, LightParameter.Position, l1_position);
            GL.Light(LightName.Light1, LightParameter.Diffuse, l_diffuse);
            GL.Light(LightName.Light1, LightParameter.Specular, l_specular);
        }

        doSelect();
        renderInternal();
        SwapBuffers();
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

    protected override void OnResize(EventArgs e)
    {
        base.OnResize(e);

        GL.MatrixMode(MatrixMode.Projection);
        GL.LoadIdentity();
        double aspect = (double) Height / (double) Width;
        GL.Frustum(-horizontalTan, horizontalTan, aspect
                * -horizontalTan, aspect * horizontalTan, 1, 100000);
        GL.Viewport(0, 0, Width, Height);
        GL.MatrixMode(MatrixMode.Modelview);
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