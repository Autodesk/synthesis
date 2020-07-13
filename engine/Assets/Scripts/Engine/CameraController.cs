using UnityEngine;
using Synthesis.Simulator.Interaction;
using SynthesisAPI.InputManager.Digital;
using SynthesisAPI.InputManager.Axis;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Events;
using SynthesisAPI.EventBus;

namespace Synthesis.Simulator
{

    public class CameraController : MonoBehaviour // TODO make SystemBase ? 
    {
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

        public void Start()
        {
            // Bind controls
            InputManager.AssignDigital("UseOrbit", (KeyDigital)KeyCode.Mouse0, UseOrbit);
            InputManager.AssignAxis("ZoomCamera", (DualAxis)"Mouse ScrollWheel");
        }

        /// <summary>
        /// Function used to switch orbit control on and off
        /// </summary>
        /// <param name="e"></param>
        public void UseOrbit(IEvent e)
        {
            if (e is DigitalStateEvent de)
            {
                if (de.KeyState == DigitalState.Down)
                {
                    OrbitActive = true;
                    // Debug.Log("Orbit Active");
                    Cursor.lockState = CursorLockMode.Locked; // Hide and lock cursor so the mouse doesn't leave the screen
                    Cursor.visible = false;
                }
                else if (de.KeyState == DigitalState.Up)
                {
                    OrbitActive = false;
                    // Debug.Log("Orbit Deactive");
                    Cursor.lockState = CursorLockMode.None; // Show and unlock cursor when done
                    Cursor.visible = true;
                }
            } else
            {
                throw new System.Exception();
            }
        }

        public bool updatePrint = false;

        private float lastXMod = 0, lastYMod = 0, lastDistMod = 0; // Used for accelerating the camera orbit speed
        public void Update()
        {
            if (!updatePrint)
            {
                Debug.Log(Time.realtimeSinceStartup);
                updatePrint = true;
            }

            // TODO: Accelerate the scroll wheel even after the user briefly stops scrolling
			float distMod = -InputManager.GetAxisValue("ZoomCamera");
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
                float yMod = InputManager.GetAxisValue("MouseX");
                float xMod = InputManager.GetAxisValue("MouseY");

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
    }

}