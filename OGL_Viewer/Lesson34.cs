using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Tao.OpenGl;
using Tao.FreeGlut;

/// <summary>
/// 
/// </summary>
/// <remarks>
/// Make sure to add freeglut.dll to your PATH.  Todo dynamicify with [DllImport kernel32] LoadLibrary()
/// </remarks>
public class OGL_Viewer
{
    public static bool[] KEY_STATES = new bool[512];
    private static double horizontalTan = Math.Tan(25.0 * 3.14 / 180.0);
    private const int WIDTH = 500, HEIGHT = 500;
    private static VBOMesh vboMesh;
    static int shaders = 0;
    static Camera3rdPerson cam = new Camera3rdPerson();

    static float[] l0_position = { 1000f, -1000f, 1000f, 0f };
    static float[] l1_position = { -1000f, -1000f, -1000f, 0f };
    static float[] l_diffuse = { .2f, .2f, .2f, .2f };
    static float[] l_specular = { .1f, .1f, .1f, .1f };
    static float[] ambient = { .125f, .125f, .125f, .125f };

    public static void display()
    {
        Gl.glClear(Gl.GL_COLOR_BUFFER_BIT | Gl.GL_DEPTH_BUFFER_BIT);
        Gl.glMatrixMode(Gl.GL_MODELVIEW);
        Gl.glLoadIdentity();
        cam.translate();

        {
            Gl.glUseProgramObjectARB(shaders);
            Gl.glLightModelfv(Gl.GL_AMBIENT, ambient);
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_POSITION, l0_position);
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_DIFFUSE, l_diffuse);
            Gl.glLightfv(Gl.GL_LIGHT0, Gl.GL_SPECULAR, l_specular);

            Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_POSITION, l1_position);
            Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_DIFFUSE, l_diffuse);
            Gl.glLightfv(Gl.GL_LIGHT1, Gl.GL_SPECULAR, l_specular);
        }


        vboMesh.draw();

        Gl.glFlush();
        Glut.glutSwapBuffers();
    }

    public static void reshape(int width, int height)
    {
        Gl.glMatrixMode(Gl.GL_PROJECTION);
        Gl.glLoadIdentity();
        double aspect = (double) height / (double) width;
        Gl.glFrustum(-horizontalTan, horizontalTan, aspect
                * -horizontalTan, aspect * horizontalTan, 1, 100000);
        Gl.glViewport(0, 0, width, height);
        Gl.glMatrixMode(Gl.GL_MODELVIEW);
    }

    static void keyDown(byte c, int x, int y)
    {
        KEY_STATES[c] = true;
    }
    static void keyUp(byte c, int x, int y)
    {
        KEY_STATES[c] = false;
    }

    public static void Main(string[] args)
    {
        BXDAMesh mesh = new BXDAMesh();
        mesh.ReadFromFile("C:/Users/t_millw/Downloads/Skeleton/node_0.bxda");
        vboMesh = new VBOMesh(mesh.meshes[0]);
        Glut.glutInit();
        Glut.glutInitWindowPosition(0, 0);
        Glut.glutInitWindowSize(WIDTH, HEIGHT);
        Glut.glutInitDisplayMode(Glut.GLUT_SINGLE | Glut.GLUT_RGB);
        Glut.glutCreateWindow("BXDJ Viewer");
        Gl.glClearColor(0f, 0f, 0f, 0f);
        Console.WriteLine(GlExtensionLoader.LoadExtension("GL_ARB_vertex_buffer_object"));
        Console.WriteLine(GlExtensionLoader.LoadExtension("GL_ARB_shader_objects"));
        Console.WriteLine(GlExtensionLoader.LoadExtension("GL_ARB_vertex_shader"));
        Console.WriteLine(GlExtensionLoader.LoadExtension("GL_ARB_fragment_shader"));

        Glut.glutDisplayFunc(display);
        Glut.glutReshapeFunc(reshape);
        Glut.glutKeyboardFunc(keyDown);
        Glut.glutKeyboardUpFunc(keyUp);
        reshape(WIDTH, HEIGHT);
        Glut.glutIdleFunc(Glut.glutPostRedisplay);
        Glut.glutSetOption(Glut.GLUT_ACTION_ON_WINDOW_CLOSE, Glut.GLUT_ACTION_GLUTMAINLOOP_RETURNS);

        Gl.glEnable(Gl.GL_LIGHTING);
        Gl.glEnable(Gl.GL_LIGHT0);
        Gl.glEnable(Gl.GL_LIGHT1);
        Gl.glEnable(Gl.GL_DEPTH_TEST);
        shaders = ShaderLoader.loadShaderPair();

        Glut.glutMainLoop();
        vboMesh.unloadFromGPU();
        //Gl.glDeleteProgramsARB(1, new int[] { shaders });
    }
}