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
    private const int WIDTH = 1366, HEIGHT = 768;
    private static double horizontalTan = Math.Tan(25.0 * 3.14 / 180.0);

    private OGL_RigidNode baseNode;
    private List<RigidNode_Base> nodes;

    Camera3rdPerson cam = new Camera3rdPerson();

    static float[] l0_position = { 1000f, -1000f, 1000f, 0f };
    static float[] l1_position = { -1000f, 1000f, -1000f, 0f };
    static float[] l_diffuse = { 1f, 1f, 1f, 1f };
    static float[] l_specular = { .1f, .1f, .1f, .1f };
    static float[] ambient = { .125f, .125f, .125f, .125f };

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
        groups.Show();
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
        : base(WIDTH, HEIGHT, new GraphicsMode(32, 0, 0, 4), "Skeleton Viewer")
    {
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

        baseNode.compute();
        foreach (RigidNode_Base node in nodes)
        {
            ((OGL_RigidNode) node).render();
        }

        SwapBuffers();
    }

    protected override void OnUpdateFrame(FrameEventArgs e)
    {
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