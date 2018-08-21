using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Graphics.OpenGL;
using OpenTK.Graphics;
using OpenTK;

namespace OGLViewer
{
    public class OGLDrawing
    {
        /// <summary>
        /// Draws a cross hair at the origin
        /// </summary>
        /// <param name="axis">The crosshair plane is normal to this</param>
        /// <param name="bias">Use this to orient the crosshair</param>
        /// <param name="size">The radius of the crosshair</param>
        public static void drawCrossHair(BXDVector3 axis, BXDVector3 bias, float size)
        {
            BXDVector3 dir1 = BXDVector3.CrossProduct(axis, bias);
            BXDVector3 dir2 = BXDVector3.CrossProduct(axis, dir1);

            GL.Begin(PrimitiveType.Lines);
            foreach (BXDVector3 seg in new BXDVector3[] { dir1, dir2 })
            {
                GL.Vertex3(-seg.x * size, -seg.y * size, -seg.z * -size);
                GL.Vertex3(seg.x * size, seg.y * size, seg.z * -size);
            }
            GL.End();
        }

        /// <summary>
        /// Draws an arc at the origin, starting at bias and rotating around axis.
        /// </summary>
        /// <param name="axis">The axis to rotate around</param>
        /// <param name="minAngle">The starting angle</param>
        /// <param name="maxAngle">The ending angle</param>
        /// <param name="radius">The radius of the arc</param>
        /// <param name="steps">The number of steps to use</param>
        public static void drawArc(BXDVector3 axis, BXDVector3 bias, float minAngle, float maxAngle, float radius, Color4 start, Color4 end, int steps = 100)
        {
            GL.Begin(PrimitiveType.LineStrip);
            // Arcthing
            Matrix4 stepMatrix = Matrix4.CreateFromAxisAngle(axis.ToTK(), -(maxAngle - minAngle) / steps);
            BXDVector3 tempVec = bias.Copy().Multiply(radius / bias.Magnitude());
            for (float f = 0; f < 1.0f; f += 1f / (float)steps)
            {
                GL.Color4(Interpolate(start, end, f));
                GL.Vertex3(tempVec.x, tempVec.y, tempVec.z);
                tempVec = stepMatrix.Multiply(tempVec);
            }
            GL.End();
        }

        public static Color4 Interpolate(Color4 start, Color4 end, float val)
        {
            return new Color4(start.R * (1f - val) + end.R * val, start.G * (1f - val) + end.G * val,
                start.B * (1f - val) + end.B * val, start.A * (1f - val) + end.A * val);
        }
    }
}
