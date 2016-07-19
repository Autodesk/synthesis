using System;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Windows.Input;

namespace OpenTKbuild
{
    class Scene : GameWindow
    {
        //Shader paths
        public string vShaderPath = @"resources/Shaders/vertexShader.vs", fShaderPath = @"resources/Shaders/fragmentshader.frag", gShaderPath = @"resources/Shaders/geometryShader.geo";
        public string VSP_Lamp = @"resources/Shaders/VS_Lamp.vs", FSP_Lamp = @"resources/Shaders/FS_Lamp.frag";

        //OpenTK variables
        static OpenTK.Graphics.GraphicsMode gMode = new OpenTK.Graphics.GraphicsMode(32, 34, 8, 4);

        //Drawing variables
        static int width = 1200, height = 800;

        //Camera variables
        Camera camera = new Camera(0.0f, 0.0f, 3.0f, 0.0f, 0.0f, 0.0f, -90.0f, 0.0f);
        float lastX = Scene.width  / 2;
        float lastY = Scene.height / 2;
        bool[] keys = new bool[1024];
        
        public Scene() : base (width, height, gMode, "Synthesis R&D")
        {
            //Drawing variables
            GL.Viewport(0, 0, width, height);
            GL.Enable(EnableCap.DepthTest);
            Shader lightingShader = new Shader(vShaderPath, fShaderPath);
            Shader lampShader = new Shader(VSP_Lamp, FSP_Lamp);

            //vertices of each container
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

            //positions of all containers
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

            //Generating Buffers for the Light source and Objects
            int VBO = GL.GenBuffer();
            int containerVAO = GL.GenVertexArray();
            int lightVAO = GL.GenVertexArray();

            //VBO
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.BufferData(BufferTarget.ArrayBuffer, sizeof(float) * 180, vertices, BufferUsageHint.StaticDraw);

            //containerVAO
            GL.BindVertexArray(containerVAO);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindVertexArray(0);

            //lightVAO
            GL.BindVertexArray(lightVAO);
            GL.BindBuffer(BufferTarget.ArrayBuffer, VBO);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), 0);
            GL.EnableVertexAttribArray(0);
            GL.BindVertexArray(0);
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

        //Function callbacks
        void key_callback(GameWindow window, int key, int scancode, int action, int mode)
        {
            var state = OpenTK.Input.Keyboard.GetState();
            if (state.IsKeyDown(OpenTK.Input.Key.Escape))
                this.Close();
            if (key >= 0 && key < 1024)
            {
                if (state.IsAnyKeyDown)
                    keys[key] = true;
                else
                    keys[key] = false;
            }
        }

        void do_movement()
        {
            if (keys[(int)OpenTK.Input.Key.W])
                camera.ProcessKeyboard(Camera_Movement.forward, (float)RenderTime);
            if (keys[(int)OpenTK.Input.Key.A])
                camera.ProcessKeyboard(Camera_Movement.left, (float)RenderTime);
            if (keys[(int)OpenTK.Input.Key.S])
                camera.ProcessKeyboard(Camera_Movement.backward, (float)RenderTime);
            if (keys[(int)OpenTK.Input.Key.D])
                camera.ProcessKeyboard(Camera_Movement.right, (float)RenderTime);
        }

        bool firstMouse = true;

        void mouse_callback(GameWindow window, double xpos, double ypos)
        {
            if(firstMouse)
            {
                lastX = (float)xpos;
                lastY = (float)ypos;
                firstMouse = false;
            }

            float xoffset = (float)xpos - lastX;
            float yoffset = (float)ypos - lastY;

            camera.ProcessMouseMovement(xoffset, yoffset);
        }

        void scroll_callback(GameWindow window, double xoffset, double yoffset)
        {
            camera.ProcessMouseScroll((float)yoffset);
        }
    }
}
