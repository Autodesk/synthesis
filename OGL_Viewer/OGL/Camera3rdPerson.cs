using System;
using System.Windows.Forms;
using System.Windows.Input;
using Tao.OpenGl;

public class Camera3rdPerson {
	private const float yawMilli = .1f;
    private const float pitchMilli = .1f;
    private const float moveMilli = .1f;
	private const float centerMilli = 1f;

	private float pitch = 10, yaw = 0, centerDist = 100;
	private long lastMoveProc = -1;

    private BXDVector3 place = new BXDVector3();

	public void modPitch(float val) {
		if (pitch + val >= 0 && pitch + val <= 90) {
			pitch += val;
		}
	}

	public void modYaw(float val) {
		yaw += val;
		if (yaw < 0)
			yaw += 360;
		if (yaw >= 360)
			yaw -= 360;
	}

	public float getYaw() {
		return yaw;
	}

	public float getPitch() {
		return pitch;
	}

	public void translate() {
		if (lastMoveProc != -1) {
			long passed = System.Environment.TickCount - lastMoveProc;
            if (OGL_Viewer.KEY_STATES['w'])
				modPitch(passed * pitchMilli);
            else if (OGL_Viewer.KEY_STATES['s'])
				modPitch(-passed * pitchMilli);
            if (OGL_Viewer.KEY_STATES['a'])
				modYaw(-passed * yawMilli);
            else if (OGL_Viewer.KEY_STATES['d'])
				modYaw(passed * yawMilli);
            if (OGL_Viewer.KEY_STATES['q'])
				centerDist -= passed * centerMilli;
            else if (OGL_Viewer.KEY_STATES['e'])
				centerDist += passed * centerMilli;

            if (OGL_Viewer.KEY_STATES['k'])
                place.y += passed * moveMilli;
            else if (OGL_Viewer.KEY_STATES['i'])
                place.y -= passed * moveMilli;
            if (OGL_Viewer.KEY_STATES['j'])
            {
                place.x += passed * moveMilli * (float) Math.Cos(yaw * 3.14 / 180.0);
                place.z -= passed * moveMilli * (float) Math.Sin(yaw * 3.14 / 180.0);
            }
            else if (OGL_Viewer.KEY_STATES['l'])
            {
                place.x -= passed * moveMilli * (float) Math.Cos(yaw * 3.14 / 180.0);
                place.z += passed * moveMilli * (float) Math.Sin(yaw * 3.14 / 180.0);
            }
            if (OGL_Viewer.KEY_STATES['u'])
            {
                place.z += passed * moveMilli * (float) Math.Cos(yaw * 3.14 / 180.0);
                place.x += passed * moveMilli * (float) Math.Sin(yaw * 3.14 / 180.0);
            }
            else if (OGL_Viewer.KEY_STATES['o'])
            {
                place.z -= passed * moveMilli * (float) Math.Cos(yaw * 3.14 / 180.0);
                place.x -= passed * moveMilli * (float) Math.Sin(yaw * 3.14 / 180.0);
            }
		}
		lastMoveProc = System.Environment.TickCount;
		Gl.glTranslatef(0, 0, -centerDist);
		Gl.glRotatef(pitch, 1, 0, 0);
		Gl.glRotatef(360 - yaw, 0, 1, 0);
        Gl.glTranslatef(place.x, place.y, place.z);
	}

	public override string ToString() {
		return "3rd Person Camera: Yaw: " + yaw + " Pitch: " + pitch + " Mag: "
				+ centerDist;
	}
}