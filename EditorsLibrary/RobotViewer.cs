using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OGLViewer;

namespace EditorsLibrary
{
    public partial class RobotViewer : UserControl
    {

        public bool isLoaded { get; private set; }

        private int vaoID;

        private IndexedVBO vbo;
        private int vboID;

        public RobotViewer()
        {
            InitializeComponent();
        }

        private void glControl1_Load(object sender, EventArgs e)
        {
            vaoID = GL.GenVertexArray();
            GL.BindVertexArray(vaoID);

            double[] verts = new double[]
            {
                0, 0, 0,
                1, 0, 0,
                0, 1, 0
            };

            vboID = GL.GenBuffer();
            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.BufferData(BufferTarget.ArrayBuffer, new IntPtr(verts.Length * 8), verts, BufferUsageHint.StaticDraw);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);
            
            GL.ClearColor(Color.SkyBlue);

            GL.BindVertexArray(0);

            isLoaded = true;
        }

        private void glControl1_Paint(object sender, PaintEventArgs e)
        {
            if (!isLoaded)
                return;
            
            GL.BindVertexArray(vaoID);
            GL.UseProgram(OGLViewer.ShaderLoader.Shader);

            GL.Clear(ClearBufferMask.ColorBufferBit | ClearBufferMask.DepthBufferBit);
            GL.EnableVertexAttribArray(0);

            GL.BindBuffer(BufferTarget.ArrayBuffer, vboID);
            GL.VertexAttribPointer(0, 3, VertexAttribPointerType.UnsignedByte, false, 3 * 8, 0);
            GL.DrawArrays(BeginMode.Triangles, 0, 3);
            GL.BindBuffer(BufferTarget.ArrayBuffer, 0);

            GL.DisableVertexAttribArray(0);

            GL.UseProgram(0);
            GL.BindVertexArray(0);

            glControl1.SwapBuffers();
        }

    }
}
