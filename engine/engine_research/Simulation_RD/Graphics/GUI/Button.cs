using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK.Input;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Simulation_RD.Graphics.GUI
{
    class Button
    {
        public event EventHandler<EventArgs> Click;

        public Vector2 topLeft, dimensions;

        public string text;

        public void Draw()
        {
            GL.Begin(PrimitiveType.Quads);
            GL.Vertex2(topLeft);
            GL.Vertex2(topLeft + new Vector2(dimensions.X, 0));
            GL.Vertex2(topLeft + dimensions);
            GL.Vertex2(topLeft + new Vector2(0, dimensions.Y));
        }
    }
}
