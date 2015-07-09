using System;
using System.Windows.Forms;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace OGLViewer
{
    public class Camera3rdPerson
    {
        private const float yawMilli = .1f;
        private const float pitchMilli = .1f;
        private const float moveMilli = .1f;
        private const float centerMilli = 1f;

        private float pitch = 10, yaw = 0, centerDist = 100;
        private long lastMoveProc = -1;

        private BXDVector3 place = new BXDVector3();

        public void modPitch(float val)
        {
            if (pitch + val >= 0 && pitch + val <= 90)
            {
                pitch += val;
            }
        }

        public void modYaw(float val)
        {
            yaw += val;
            if (yaw < 0)
                yaw += 360;
            if (yaw >= 360)
                yaw -= 360;
        }

        public float getYaw()
        {
            return yaw;
        }

        public float getPitch()
        {
            return pitch;
        }

        public void translate()
        {
            if (lastMoveProc != -1)
            {
                var keyboardState = Keyboard.GetState();
                long passed = System.Environment.TickCount - lastMoveProc;
                if (keyboardState[Key.W])
                    modPitch(passed * pitchMilli);
                else if (keyboardState[Key.S])
                    modPitch(-passed * pitchMilli);
                if (keyboardState[Key.A])
                    modYaw(-passed * yawMilli);
                else if (keyboardState[Key.D])
                    modYaw(passed * yawMilli);
                if (keyboardState[Key.Q])
                    centerDist -= passed * centerMilli;
                else if (keyboardState[Key.E])
                    centerDist += passed * centerMilli;

                if (keyboardState[Key.K])
                    place.y += passed * moveMilli;
                else if (keyboardState[Key.I])
                    place.y -= passed * moveMilli;
                if (keyboardState[Key.J])
                {
                    place.x += passed * moveMilli * (float)Math.Cos(yaw * 3.14 / 180.0);
                    place.z -= passed * moveMilli * (float)Math.Sin(yaw * 3.14 / 180.0);
                }
                else if (keyboardState[Key.L])
                {
                    place.x -= passed * moveMilli * (float)Math.Cos(yaw * 3.14 / 180.0);
                    place.z += passed * moveMilli * (float)Math.Sin(yaw * 3.14 / 180.0);
                }
                if (keyboardState[Key.U])
                {
                    place.z += passed * moveMilli * (float)Math.Cos(yaw * 3.14 / 180.0);
                    place.x += passed * moveMilli * (float)Math.Sin(yaw * 3.14 / 180.0);
                }
                else if (keyboardState[Key.O])
                {
                    place.z -= passed * moveMilli * (float)Math.Cos(yaw * 3.14 / 180.0);
                    place.x -= passed * moveMilli * (float)Math.Sin(yaw * 3.14 / 180.0);
                }
            }
            lastMoveProc = System.Environment.TickCount;
            GL.Translate(0, 0, -centerDist);
            GL.Rotate(pitch, 1, 0, 0);
            GL.Rotate(360 - yaw, 0, 1, 0);
            GL.Translate(place.x, place.y, place.z);
        }

        public override string ToString()
        {
            return "3rd Person Camera: Yaw: " + yaw + " Pitch: " + pitch + " Mag: "
                    + centerDist;
        }
    }
}