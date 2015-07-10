using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

namespace OGLViewer
{
    public class InventorCamera
    {
        public enum Mode
        {
            NONE,
            MOVE,
            ORBIT,
            FINE_ZOOM
        }

        public Mode currentMode = Mode.NONE;
        private float width = 100, height = 100;
        public float offset = -70;

        public Matrix4 pose = Matrix4.Identity;

        public void translate()
        {
            GL.Translate(50, -30, offset);
            GL.MultMatrix(ref pose);
        }

        public void renderOverlay(float width, float height)
        {
            this.width = width;
            this.height = height;

            GL.PushAttrib(AttribMask.AllAttribBits);
            GL.LineWidth(2);
            GL.LogicOp(LogicOp.Invert);
            GL.Enable(EnableCap.ColorLogicOp);
            if (currentMode == Mode.ORBIT)
            {
                float radius = Math.Min(width, height) * 0.3f;
                GL.PushMatrix();
                GL.Translate(width / 2, height / 2, 0);
                OGLDrawing.drawArc(new BXDVector3(0, 0, 1), new BXDVector3(0, 1, 0), 0, 6.28f, radius, Color4.Gray, Color4.Gray);
                GL.Begin(PrimitiveType.Lines);
                // Center crosshairs
                GL.Vertex2(-radius / 4.0f, 0);
                GL.Vertex2(radius / 4.0f, 0);
                GL.Vertex2(0, -radius / 4.0f);
                GL.Vertex2(0, radius / 4.0f);

                //Other things
                GL.Vertex2(-radius, 0);
                GL.Vertex2(-radius - (radius / 8.0f), 0);
                GL.Vertex2(radius, 0);
                GL.Vertex2(radius + (radius / 8.0f), 0);

                GL.Vertex2(0, -radius);
                GL.Vertex2(0, -radius - (radius / 8.0f));
                GL.Vertex2(0, radius);
                GL.Vertex2(0, radius + (radius / 8.0f));

                GL.End();
                GL.PopMatrix();
            }
            GL.PopAttrib();
        }

    }
}
