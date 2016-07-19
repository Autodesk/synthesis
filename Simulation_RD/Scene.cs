using System;
using System.Drawing;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using BulletSharp;

namespace Simulation_RD
{
    class Scene : GameWindow
    {
        Physics phys;
        float frameTime;
        int fps;
        int VBO;
        int shaderHandle;
        float lastTime;
        PolygonMode drawMode = PolygonMode.Fill;

        //For angle rotation (RMB)
        int aX = 0;
        int aY = 0;
        double aXPost = 0;
        double aYPost = 0;
        double angleX = 0;
        double angleY = 0;

        //For Screen Reposition (LMB)
        int x = 0;
        int y = 0;
        double xPost = 0;
        double yPost = 0;
        double xShift = 0;
        double yShift = 0;

        //For zooming
        float zoomFactor;

        public Scene() : base(750, 750, new OpenTK.Graphics.GraphicsMode(), "RnD Synthesis Test")
        {
            VSync = VSyncMode.Off;
            phys = new Physics();
        }

        protected override void OnLoad(EventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(Color.FromArgb(100, 100, 100, 255));

            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.Lighting);

            shaderHandle = GL.CreateShader(ShaderType.VertexShader);

            this.MouseDown += glControl1_MouseDown;
            this.MouseUp += glControl1_MouseUp;
            this.MouseMove += glControl1_MouseMove;
            this.MouseWheel += glControl1_MouseWheel;
            this.KeyDown += glControl1_KeyDown;

            phys.World.DebugDrawer = new BulletDebugDrawer();
            base.OnLoad(e);            
        }


        protected override void OnUnload(EventArgs e)
        {
            phys.ExitPhysics();
            base.OnUnload(e);
        }

        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            phys.Update((float)e.Time);
        }

        protected override void OnRenderFrame(FrameEventArgs e)
        {
            frameTime += (float)e.Time;
            fps++;
            if (frameTime >= 1)
            {
                frameTime = 0;
                Title = "FPS = " + fps.ToString();
                fps = 0;
            }

            GL.PolygonMode(MaterialFace.FrontAndBack, drawMode);
            GL.Viewport(0, 0, Width, Height);

            float aspect_ratio = Width / (float)Height;
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver3, aspect_ratio, 0.1f, 1000);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perspective);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            Matrix4 lookat = Matrix4.LookAt(new Vector3((float)xShift, (float)yShift, zoomFactor), new Vector3((float)xShift, (float)yShift, 0), Vector3.UnitY);
            GL.LoadMatrix(ref lookat);
            //GL.Translate(-xShift, -yShift, 0.0);
            GL.Rotate(angleX, 0.0f, 1.0f, 0.0f);
            GL.Rotate(angleY, -1, 0, 0);

            lastTime = (float)e.Time;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            for (int i = 0; i < phys.f.VisualMeshes.Count; i++)
            {
                //phys.f.VisualMeshes[i].Draw(phys.f.Bodies[i]);
            }

            phys.World.DebugDrawWorld();

            #region Old Stuff
            //phys.World.DebugDrawWorld();
            //foreach (RigidBody body in phys.World.CollisionObjectArray)
            //{
            //    Matrix4 modelLookAt = body.MotionState.WorldTransform * lookat;
            //    GL.LoadMatrix(ref modelLookAt);

            //    if ("Ground".Equals(body.UserObject))
            //    {
            //        DrawCube(Color.DarkGray, 50.0f);
            //        continue;
            //    }
            //    if (body.ActivationState == ActivationState.ActiveTag)
            //        DrawCube2(Color.LightCyan);
            //    else
            //        DrawCube2(Color.White);
            //}
            #endregion

            //UninitCube();

            SwapBuffers();
        }

        #region Old Stuff
        private void DrawCube(Color color, float size)
        {
            GL.Begin(PrimitiveType.Quads);

            GL.Color3(color);
            GL.Vertex3(-size, -size, -size);
            GL.Vertex3(-size, size, -size);
            GL.Vertex3(size, size, -size);
            GL.Vertex3(size, -size, -size);

            GL.Vertex3(-size, -size, -size);
            GL.Vertex3(size, -size, -size);
            GL.Vertex3(size, -size, size);
            GL.Vertex3(-size, -size, size);

            GL.Vertex3(-size, -size, -size);
            GL.Vertex3(-size, -size, size);
            GL.Vertex3(-size, size, size);
            GL.Vertex3(-size, size, -size);

            GL.Vertex3(-size, -size, size);
            GL.Vertex3(size, -size, size);
            GL.Vertex3(size, size, size);
            GL.Vertex3(-size, size, size);

            GL.Vertex3(-size, size, -size);
            GL.Vertex3(-size, size, size);
            GL.Vertex3(size, size, size);
            GL.Vertex3(size, size, -size);

            GL.Vertex3(size, -size, -size);
            GL.Vertex3(size, size, -size);
            GL.Vertex3(size, size, size);
            GL.Vertex3(size, -size, size);

            GL.End();
        }

        private void drawTriangles(BulletTriangle[] triangles)
        {

            GL.Begin(PrimitiveType.Triangles);

            GL.Color3(Color.DeepPink);


        }

        float[] vertices = new float[] {1,1,1,  -1,1,1,  -1,-1,1,  1,-1,1,
            1,1,1,  1,-1,1,  1,-1,-1,  1,1,-1,
            1,1,1,  1,1,-1,  -1,1,-1,  -1,1,1,
            -1,1,1,  -1,1,-1,  -1,-1,-1,  -1,-1,1,
            -1,-1,-1,  1,-1,-1,  1,-1,1,  -1,-1,1,
            1,-1,-1,  -1,-1,-1,  -1,1,-1,  1,1,-1};

        float[] normals = new float[] {0,0,1,  0,0,1,  0,0,1,  0,0,1,
            1,0,0,  1,0,0,  1,0,0, 1,0,0,
            0,1,0,  0,1,0,  0,1,0, 0,1,0,
            -1,0,0,  -1,0,0, -1,0,0,  -1,0,0,
            0,-1,0,  0,-1,0,  0,-1,0,  0,-1,0,
            0,0,-1,  0,0,-1,  0,0,-1,  0,0,-1};

        byte[] indices = {0,1,2,3,
            4,5,6,7,
            8,9,10,11,
            12,13,14,15,
            16,17,18,19,
            20,21,22,23};

        void InitCube()
        {
            GL.EnableClientState(ArrayCap.NormalArray);
            GL.EnableClientState(ArrayCap.VertexArray);
            GL.NormalPointer(NormalPointerType.Float, 0, normals);
            GL.VertexPointer(3, VertexPointerType.Float, 0, vertices);
        }

        void UninitCube()
        {
            GL.DisableClientState(ArrayCap.VertexArray);
            GL.DisableClientState(ArrayCap.NormalArray);
        }

        void DrawCube2(Color color)
        {
            GL.Color3(color);
            GL.DrawElements(PrimitiveType.Quads, 24, DrawElementsType.UnsignedByte, indices);
        }
        #endregion

        private void glControl1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Mouse.RightButton == ButtonState.Pressed)
            {
                aX = e.X;
                aY = e.Y;
            }
            else if (e.Mouse.LeftButton == ButtonState.Pressed)
            {
                x = e.X;
                y = e.Y;
            }
        }

        private void glControl1_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Mouse.RightButton == ButtonState.Released)
            {
                aXPost = angleX;
                aYPost = angleY;
            }
            if (e.Mouse.LeftButton == ButtonState.Released)
            {
                xPost = xShift;
                yPost = yShift;
            }
        }

        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Mouse.RightButton == ButtonState.Pressed)
            {
                angleX = aXPost + (e.X - aX) * .5;
                angleY = aYPost + (e.Y - aY) * .5;
            }
            else if (e.Mouse.LeftButton == ButtonState.Pressed)
            {
                xShift = xPost + (e.X - x) * .4;
                yShift = yPost + (e.Y - y) * .4;
            }
        }

        private void glControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            zoomFactor = e.Mouse.Wheel * 7;
        }

        private void glControl1_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Space:
                    drawMode = ((drawMode - PolygonMode.Point) + 1) % 3 + PolygonMode.Point;
                    break;
                case Key.Escape:
                    this.Close();
                    break;
                default:
                    break;
            }
        }
    }
}
