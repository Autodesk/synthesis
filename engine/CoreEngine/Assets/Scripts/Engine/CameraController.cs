using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Synthesis.Simulator.Input;
using Synthesis.Simulator.Interaction;
using Synthesis.UI.Style;
using System.Xml;
using System.IO;

namespace Synthesis.Simulator
{

    public class CameraController : MonoBehaviour
    {
        /* TODO: Modify this to work with the event bus when that is merged in
        
        public static float SensitivityX { get => 5; } // TODO: Setup some sort of class for storing preferences
        public static float SensitivityY { get => 3; }

        private bool OrbitActive = false;

        /// <summary>
        /// Orientation used to define the resting place of the camera while orbiting
        /// </summary>
        /// <returns></returns>
        private Vector3 CameraEuler = new Vector3(-15, 45, 0);
        private float Distance = 5;

        /// <summary>
        /// An optional target to focus on
        /// </summary>
        public static ISelectable SelectedTarget = null;

        private void Start()
        {
            // Bind controls
            InputHandler.MappedDigital[UseOrbit] = (KeyDigital)KeyCode.Mouse2;
            InputHandler.MappedAxes["ZoomCamera"] = (DualAxis)"Mouse ScrollWheel";
        }

        /// <summary>
        /// Function used to switch orbit control on and off
        /// </summary>
        /// <param name="sender"><see cref="KeyDigital"/> object that called the method</param>
        /// <param name="state">State the key was in when the method was called</param>
        private void UseOrbit(object sender, KeyAction state)
        {
            if (state == KeyAction.Down)
            {
                OrbitActive = true;
				// Debug.Log("Orbit Active");
                Cursor.lockState = CursorLockMode.Locked; // Hide and lock cursor so the mouse doesn't leave the screen
                Cursor.visible = false;
            }
            else if (state == KeyAction.Up)
            {
                OrbitActive = false;
				// Debug.Log("Orbit Deactive");
                Cursor.lockState = CursorLockMode.None; // Show and unlock cursor when done
                Cursor.visible = true;
            }
        }

        public bool updatePrint = false;

        private float lastXMod = 0, lastYMod = 0, lastDistMod = 0; // Used for accelerating the camera orbit speed
        private void Update()
        {
            if (!updatePrint)
            {
                Debug.Log(Time.realtimeSinceStartup);
                updatePrint = true;
            }

			// TODO: Accelerate the scroll wheel even after the user briefly stops scrolling
			float distMod = -InputHandler.MappedAxes["ZoomCamera"].GetValue();
			if (distMod != 0) distMod += lastDistMod * 0.3f;
			Distance += distMod;
			lastDistMod = distMod;
			Distance = Mathf.Clamp(Distance, 0.25f, 50);
            Vector3 pos = Vector3.zero; // Default focus point
            if (SelectedTarget != null)
            { // Adjust defaults if a target is selected
                pos = SelectedTarget.Position;
            }
            if (OrbitActive)
            { // Adjust Camera Euler to reorientate the camera
              // use mouse axes to adjust orientation
                float yMod = InputHandler.MappedAxes["MouseX"].GetValue();
                float xMod = InputHandler.MappedAxes["MouseY"].GetValue();

                // Tinkering for the feel of the camera
                if (xMod != 0) xMod += lastXMod * 0.3f;
                if (yMod != 0) yMod += lastYMod * 0.3f;
                lastXMod = xMod;
                lastYMod = yMod;

                // Get new Camera Orientation
                CameraEuler.x += xMod * SensitivityX;
                CameraEuler.y += yMod * SensitivityY;

                // Clamp the x so shenanigans don't happen
                CameraEuler.x = Mathf.Clamp(CameraEuler.x, -80, -2.5f);
            }

            // Smooth translation to targetPosition
            Vector3 targetPosition = pos + (Quaternion.Euler(CameraEuler) * new Vector3(0, 0, Distance));
            Vector3 deltaPos = targetPosition - transform.position;
            transform.position += deltaPos * 0.4f;

            // Have the camera look at the focus point
            // This does cause some weird effects in some cases so we may want to make this a smooth rotation as well
            transform.LookAt(pos, new Vector3(0, 1, 0));
        }
        */
    }

}