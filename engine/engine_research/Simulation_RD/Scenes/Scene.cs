using System;
using System.Drawing;
using System.Collections.Generic;
using OpenTK;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using BulletSharp;
using Simulation_RD.Graphics;
using Simulation_RD.SimulationPhysics;

namespace Simulation_RD.Scenes
{
    /// <summary>
    /// Does all drawing and contains <see cref="Physics"/>
    /// </summary>
    class SimulationScene : GameWindow
    {
        bool pause = false;
        Physics phys;
        float frameTime;
        int fps;
        int VBO;
        int shaderHandle;
        float lastTime;
        float oldScroll;
        KeyboardKeyEventArgs cachedKeyboard = new KeyboardKeyEventArgs();

        int mxPrev, myPrev;

        PolygonMode drawMode = PolygonMode.Fill;

        Dictionary<Key, Camera_Movement> CameraBindings;

        Camera c;

        /// <summary>
        /// Instantiates all the members not given default values
        /// </summary>
        public SimulationScene() : base(1500, 768, new OpenTK.Graphics.GraphicsMode(), "RnD Synthesis Test")
        {
            VSync = VSyncMode.Off;
            phys = new Physics();
            c = new Camera(100, 200, 100, 0, 1, 0, 0, 0);
            c.Sensitivity = 0.01f;
            CameraBindings = new Dictionary<Key, Camera_Movement>();
            CameraBindings.Add(Key.W, Camera_Movement.forward);
            CameraBindings.Add(Key.S, Camera_Movement.backward);
            CameraBindings.Add(Key.A, Camera_Movement.left);
            CameraBindings.Add(Key.D, Camera_Movement.right);            
        }

        /// <summary>
        /// Sets up OpenGL, Controls, Physics
        /// </summary>
        /// <param name="e"></param>
        protected override void OnLoad(EventArgs e)
        {
            GL.Enable(EnableCap.DepthTest);
            GL.ClearColor(Color.FromArgb(100, 100, 100, 255));

            GL.Enable(EnableCap.ColorMaterial);
            GL.Enable(EnableCap.Light0);
            GL.Enable(EnableCap.Lighting);

            shaderHandle = GL.CreateShader(ShaderType.VertexShader);
            
            this.MouseMove += glControl1_MouseMove;
            this.MouseWheel += glControl1_MouseWheel;
            this.KeyDown += glControl1_KeyDown;

            //Action<EventHandler<KeyboardKeyEventArgs>> AddHandler = (handler) => KeyDown += handler;

            phys.World.DebugDrawer = new BulletDebugDrawer();
            base.OnLoad(e);            
        }

        /// <summary>
        /// cleanup
        /// </summary>
        /// <param name="e"></param>
        protected override void OnUnload(EventArgs e)
        {
            phys.ExitPhysics();
            base.OnUnload(e);
        }

        /// <summary>
        /// Updates physics and other stuff
        /// </summary>
        /// <param name="e"></param>
        protected override void OnUpdateFrame(FrameEventArgs e)
        {
            c.LookAtRobot(phys.Skeleton.BulletObject.WorldTransform.ExtractTranslation());
            c.Movespeed = cachedKeyboard.Shift ? 75 : 750;
            if (CameraBindings.ContainsKey(cachedKeyboard.Key))
                ;// c.ProcessKeyboard(CameraBindings[cachedKeyboard.Key], (float)e.Time);
            if (cachedKeyboard.Key == Key.B)
                pause = !pause;

            if(!pause)
                phys.Update((float)e.Time, cachedKeyboard);

            cachedKeyboard = new KeyboardKeyEventArgs();
        }

        /// <summary>
        /// draws stuff
        /// </summary>
        /// <param name="e"></param>
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
            Matrix4 perspective = Matrix4.CreatePerspectiveFieldOfView(MathHelper.PiOver3, aspect_ratio, 0.1f, 100000);
            GL.MatrixMode(MatrixMode.Projection);
            GL.LoadMatrix(ref perspective);

            GL.MatrixMode(MatrixMode.Modelview);
            GL.LoadIdentity();
            Matrix4 lookat = c.GetViewMatrix();
            GL.LoadMatrix(ref lookat);

            lastTime = (float)e.Time;

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);

            for (int i = 0; i < phys.f.VisualMeshes.Count; i++)
            {
                //phys.f.VisualMeshes[i].Draw(phys.f.Bodies[i]);
            }

            //Comment this out when actual graphics work
            phys.World.DebugDrawWorld();
                        
            SwapBuffers();
        }
  
        /// <summary>
        /// Rotates camera based on mouse
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glControl1_MouseMove(object sender, MouseEventArgs e)
        {           
            if ((e.Mouse.LeftButton & ButtonState.Pressed) > 0)
            {
                float dy = e.Y - myPrev;
                dy /= 10;
                float dx = e.X - mxPrev;
                dx /= 10;
                float udy = Math.Abs(dy);
                float udx = Math.Abs(dx);

                c.ProcessMouseMovement((float)Math.Sqrt(udx * udx * udx) * -Math.Sign(dx), (float)Math.Sqrt(udy * udy * udy) * Math.Sign(dy));
            }

            mxPrev = e.X;
            myPrev = e.Y;
        }

        /// <summary>
        /// Should zoom camera but doesn't
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glControl1_MouseWheel(object sender, MouseEventArgs e)
        {
            c.ProcessMouseScroll(e.Mouse.WheelPrecise - oldScroll);
            oldScroll = e.Mouse.WheelPrecise;
        }

        /// <summary>
        /// Moves the camera in the world using the keyboard
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void glControl1_KeyDown(object sender, KeyboardKeyEventArgs e)
        {
            cachedKeyboard = e;            
                        
            switch (e.Key)
            {
                case Key.Space:
                    drawMode = ((drawMode - PolygonMode.Point) + 1) % 3 + PolygonMode.Point;
                    break;
                case Key.Escape:
                    Close();
                    break;
                default:
                    break;
            }
        }
    }
}
