using SynthesisAPI.InputManager.Digital;
using SynthesisAPI.InputManager.Axis;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Events;
using SynthesisAPI.EventBus;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Modules.Attributes;
using SynthesisAPI.Runtime;
using Utilities;

using Entity = System.UInt32;
using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;

namespace SynthesisCore.Systems
{
    [ModuleExport]
    public class CameraController : SystemBase
    {
        public static float SensitivityX { get => 5; } // TODO: integrate with preference manager
        public static float SensitivityY { get => 2; }
        public static float SensitivityZoom { get => 3; }

        private bool orbitActive = false;

        private Vector3D focusPoint = new Vector3D(); // Default focus point

        private Entity? cameraEntity = null;
        private Transform transform = null;

        /// <summary>
        /// An optional target to focus on
        /// </summary>
        public static Selectable SelectedTarget = null;

        public override void Setup()
        {
            if (cameraEntity == null)
            {
                cameraEntity = EnvironmentManager.AddEntity();
                cameraEntity?.AddComponent<Camera>();
                transform = cameraEntity?.AddComponent<Transform>();
                transform.Position = focusPoint + new Vector3D(0, 5, 5);
                transform.LookAt(focusPoint);
            }

            // Bind controls
            InputManager.AssignDigital("UseOrbit", (KeyDigital)"Mouse0", UseOrbit);
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
                    orbitActive = true;
                    ApiProvider.Log("Orbit active");
                    // TODO cursor stuff
                    // Cursor.lockState = CursorLockMode.Locked; // Hide and lock cursor so the mouse doesn't leave the screen
                    // Cursor.visible = false;
                }
                else if (de.KeyState == DigitalState.Up)
                {
                    orbitActive = false;
                    ApiProvider.Log("Orbit inactive");
                    // Cursor.lockState = CursorLockMode.None; // Show and unlock cursor when done
                    // Cursor.visible = true;
                }
            } else
            {
                throw new System.Exception();
            }
        }

        private float lastXMod = 0, lastYMod = 0, lastDistMod = 0; // Used for accelerating the camera movement speed

        private const double MinDistance = 0.25;
        private const double MaxDistance = 50;
        private const double MinHeight = 0.25;

        private void ProcessZoom(Vector3D vectorToFocus, Vector3D unitVectorToFocus)
        {
            // TODO: Accelerate the scroll wheel even after the user briefly stops scrolling
            float distMod = -InputManager.GetAxisValue("ZoomCamera") * SensitivityZoom;
            if (distMod != 0 && (distMod < 0) == (lastDistMod < 0)) // Check that distMod is not zero and taht lastDistMod is in the same direction
            {
                distMod += lastDistMod * 0.5f; // Give some kind of intertial effect
            }
            lastDistMod = distMod;
            
            if ((vectorToFocus.Length > MinDistance && distMod < 0) || (vectorToFocus.Length < MaxDistance && distMod > 0))
            {
                transform.Position += unitVectorToFocus.ScaleBy(distMod);
            }
        }

        private void ProcessOrbit(Vector3D vectorToFocus, Vector3D _)
        {
            if (orbitActive)
            {
                float xMod = InputManager.GetAxisValue("MouseX");
                float yMod = -InputManager.GetAxisValue("MouseY");

                if (xMod != 0 && (xMod < 0) == (lastXMod < 0))
                    xMod += lastXMod * 0.3f;
                if (yMod != 0 && (yMod < 0) == (lastYMod < 0))
                    yMod += lastYMod * 0.3f;
                lastXMod = xMod;
                lastYMod = yMod;

                transform.Position = transform.Position.Rotate(UnitVector3D.YAxis, Angle.FromDegrees(xMod * SensitivityX));

                var verticalRotationAxis = vectorToFocus.CrossProduct(UnitVector3D.YAxis).Normalize();
                transform.Position = transform.Position.Rotate(verticalRotationAxis, Angle.FromDegrees(yMod * SensitivityY));

                ApiProvider.Log(transform.Position.Y);

                transform.Position = new Vector3D(transform.Position.X, Math.Max(transform.Position.Y, MinHeight), transform.Position.Z);

                transform.LookAt(focusPoint);
            }
        }

        public override void OnUpdate()
        {
            if (SelectedTarget != null)
            { // Adjust defaults if a target is selected
              // focusPoint = SelectedTarget.Position; // TODO
            }

            Vector3D vectorToFocus = transform.Position - focusPoint;
            Vector3D unitVectorToFocus = vectorToFocus.Normalize().ToVector3D();

            ProcessZoom(vectorToFocus, unitVectorToFocus);

            ProcessOrbit(vectorToFocus, unitVectorToFocus);
        }
        public override void OnPhysicsUpdate() { }
    }
}