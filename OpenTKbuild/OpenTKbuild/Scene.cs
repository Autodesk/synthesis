using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;

namespace OpenTKbuild
{
    class Scene : GameWindow
    {
        public string vShaderPath = "resources/Shaders/vertexShader.vs", fShaderPath = "resources/Shaders/fragmentshader.frag";

        //openTK variables
        static OpenTK.Graphics.GraphicsMode gMode = new OpenTK.Graphics.GraphicsMode(32, 34, 8, 4);

        //drawing variables
        static int width = 1200, height = 800;
        
        public Scene() : base (width, height, gMode, "Synthesis R&D")
        {
            //drawing variables
            GL.Viewport(0, 0, width, height);
            GL.Enable(EnableCap.DepthTest);
            Shader shader = new Shader(vShaderPath, fShaderPath);

            float[] vertices =
            {
                -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,
                 0.5f, -0.5f, -0.5f,  1.0f, 0.0f,
                 0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
                 0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
                -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
                -0.5f, -0.5f, -0.5f,  0.0f, 0.0f,

                -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
                 0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
                 0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
                 0.5f,  0.5f,  0.5f,  1.0f, 1.0f,
                -0.5f,  0.5f,  0.5f,  0.0f, 1.0f,
                -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,

                -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
                -0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
                -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
                -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
                -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
                -0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

                 0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
                 0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
                 0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
                 0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
                 0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
                 0.5f,  0.5f,  0.5f,  1.0f, 0.0f,

                -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,
                 0.5f, -0.5f, -0.5f,  1.0f, 1.0f,
                 0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
                 0.5f, -0.5f,  0.5f,  1.0f, 0.0f,
                -0.5f, -0.5f,  0.5f,  0.0f, 0.0f,
                -0.5f, -0.5f, -0.5f,  0.0f, 1.0f,

                -0.5f,  0.5f, -0.5f,  0.0f, 1.0f,
                 0.5f,  0.5f, -0.5f,  1.0f, 1.0f,
                 0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
                 0.5f,  0.5f,  0.5f,  1.0f, 0.0f,
                -0.5f,  0.5f,  0.5f,  0.0f, 0.0f,
                -0.5f,  0.5f, -0.5f,  0.0f, 1.0f
            };

            Vector3[] positions =
            {
                new Vector3( 0.0f,  0.0f,  0.0f),
                new Vector3( 2.0f,  5.0f, -15.0f),
                new Vector3(-1.5f, -2.2f, -2.5f),
                new Vector3(-3.8f, -2.0f, -12.3f),
                new Vector3( 2.4f, -0.4f, -3.5f),
                new Vector3(-1.7f,  3.0f, -7.5f),
                new Vector3( 1.3f, -2.0f, -2.5f),
                new Vector3( 1.5f,  2.0f, -2.5f),
                new Vector3( 1.5f,  0.2f, -1.5f),
                new Vector3(-1.3f,  1.0f, -1.5f)
            };

            int VBO = GL.GenBuffer();
            int VAO = GL.GenVertexArray();
            GL.BindVertexArray(VAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);


        }

        protected override void OnLoad(EventArgs e)
        {
            
            GL.ClearColor(Color.FromArgb(100, 100, 100, 255));
            
        }

        protected override void OnUnload(EventArgs e)
        {
            
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {

        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            
        }
 
    }
}
