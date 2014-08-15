using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Input;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;

public class InventorCamera
{
    enum Mode
    {
        NONE,
        MOVE,
        ORBIT,
        FINE_ZOOM
    }

    private Mode currentMode = Mode.NONE;
    private Vector2 dragStart;
    private Vector3 diffOld;
    private float width = 100, height = 100;

    private Matrix4 pose = Matrix4.Identity;
    private float offset = -100;

    public void translate()
    {
        GL.Translate(0, 0, offset);
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

    public void mouseDown(object source, MouseButtonEventArgs args)
    {
        keyStateChange(null, null); // Force update current mode
        dragStart = new Vector2(args.X, args.Y);
    }

    public void keyStateChange(object source, KeyboardKeyEventArgs args)
    {
        var mouseState = Mouse.GetState();
        var keyboardState = Keyboard.GetState();
        if (keyboardState[Key.F4] || (mouseState.IsButtonDown(MouseButton.Middle) && keyboardState[Key.ShiftLeft]))
            currentMode = Mode.ORBIT;
        else if (keyboardState[Key.F3])
            currentMode = Mode.FINE_ZOOM;
        else if (keyboardState[Key.F2] || Mouse.GetState().IsButtonDown(MouseButton.Middle))
            currentMode = Mode.MOVE;
        else
            currentMode = Mode.NONE;
    }

    public void mouseWheel(object source, MouseWheelEventArgs args)
    {
        offset -= args.DeltaPrecise * 10f;
    }

    public void mouseMoved(object source, MouseMoveEventArgs args)
    {
        var keyboardState = Keyboard.GetState();
        var mouseState = Mouse.GetState();
        if ((mouseState.IsButtonDown(MouseButton.Left) && keyboardState[Key.F4]) || (mouseState.IsButtonDown(MouseButton.Middle) && keyboardState[Key.ShiftLeft]))
        {
            float radius = Math.Min(width, height) * 0.3f;
            Vector3 diffStart = new Vector3(dragStart.X - (width / 2), dragStart.Y - (height / 2), 0);
            float diffLen = diffStart.LengthFast;
            if (diffLen > radius)
            {
                Vector3 diffCurrent = new Vector3(args.X - (width / 2), args.Y - (height / 2), 0);
                diffCurrent.NormalizeFast();
                float dir = Math.Sign((diffCurrent.X - diffOld.X) * (diffCurrent.Y * diffOld.Y) * (args.Y - (height/2)));
                float angle = (float) Math.Acos(Vector3.Dot(diffCurrent, diffOld));
                diffOld = diffCurrent;
                // Rotating
                pose *= Matrix4.CreateRotationZ(dir * angle * 0.1f);
            }
            else
            {
                // Orbiting.
                Vector3 rotationAxis = new Vector3(args.YDelta, args.XDelta, 0);
                pose *= Matrix4.CreateFromAxisAngle(rotationAxis, rotationAxis.LengthFast / 100.0f);
            }
        }
        else if (mouseState.IsButtonDown(MouseButton.Left) && keyboardState[Key.F3])
        {
            offset += args.YDelta;
        }
        else if ((mouseState.IsButtonDown(MouseButton.Left) && keyboardState[Key.F2]) || mouseState.IsButtonDown(MouseButton.Middle))
        {
            pose *= Matrix4.CreateTranslation(args.XDelta / 25f, -args.YDelta / 25f, 0);
        }
    }
}
