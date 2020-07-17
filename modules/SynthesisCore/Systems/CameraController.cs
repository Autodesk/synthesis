using MathNet.Spatial.Euclidean;
using MathNet.Spatial.Units;
using SynthesisAPI.InputManager.Digital;
using SynthesisAPI.InputManager.Axis;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Events;
using SynthesisAPI.EventBus;
using SynthesisAPI.EnvironmentManager;
using SynthesisAPI.EnvironmentManager.Components;
using SynthesisAPI.Modules.Attributes;
using Utilities;

#nullable enable

using Entity = System.UInt32;

namespace SynthesisCore.Systems
{
    [ModuleExport]
    public class CameraController : SystemBase
    {
        public static float SensitivityX { get => 5; } // TODO: integrate with preference manager
        public static float SensitivityY { get => 2; }
        public static float SensitivityZoom { get => 3; }

        private bool orbitActive = false;

        private Vector3D FocusPoint { get; } = new Vector3D(); // Default focus point

        private Entity? cameraEntity = null;
        private Transform cameraTransform = null;

        /// <summary>
        /// An optional target to focus on
        /// </summary>
        private static Selectable? SelectedTarget = null;
        private static Selectable? LastSelectedTarget = null;

        public override void Setup()
        {
            if (cameraEntity == null)
            {
                cameraEntity = EnvironmentManager.AddEntity();
                cameraEntity?.AddComponent<Camera>();
                cameraTransform = cameraEntity?.AddComponent<Transform>();
                SetFocus(null);
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
                    // TODO cursor stuff
                    // Cursor.lockState = CursorLockMode.Locked; // Hide and lock cursor so the mouse doesn't leave the screen
                    // Cursor.visible = false;
                }
                else if (de.KeyState == DigitalState.Up)
                {
                    orbitActive = false;
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

        private void ProcessZoom()
        {
            // TODO: Accelerate the scroll wheel even after the user briefly stops scrolling
            float distMod = -InputManager.GetAxisValue("ZoomCamera") * SensitivityZoom;
            if (distMod != 0 && (distMod < 0) == (lastDistMod < 0)) // Check that distMod is not zero and taht lastDistMod is in the same direction
            {
                distMod += lastDistMod * 0.5f; // Give some kind of intertial effect
            }
            lastDistMod = distMod;
            
            if ((cameraTransform.Position.Length > MinDistance && distMod < 0) || (cameraTransform.Position.Length < MaxDistance && distMod > 0))
            {
                cameraTransform.Position += cameraTransform.Position.Normalize().ToVector3D().ScaleBy(distMod);
            }
        }

        private void ProcessOrbit()
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

                // Rotate horizontally (i.e. around y-axis)
                var xDelta = cameraTransform.Position.Rotate(UnitVector3D.YAxis, Angle.FromDegrees(xMod * SensitivityX)) - cameraTransform.Position;

                // Rotate vertically
                var verticalRotationAxis = cameraTransform.Position.CrossProduct(UnitVector3D.YAxis).Normalize();
                Vector3D yDelta = cameraTransform.Position.Rotate(verticalRotationAxis, Angle.FromDegrees(yMod * SensitivityY)) - cameraTransform.Position;

                // Stop from vertically rotating past directly above the focus point
                var newPosition = cameraTransform.Position + yDelta;
                if (newPosition.AngleTo(UnitVector3D.YAxis) <= Angle.FromDegrees(2)) // TODO this doesn't catch cases where it jumps to the otherside and is more than 2 degrees away
                {
                    yDelta = new Vector3D(0, 0, 0);
                }

                // Stop from vertically rotating below the floor 

                cameraTransform.Position += xDelta + yDelta;

                cameraTransform.Position = new Vector3D(cameraTransform.Position.X, Math.Max(cameraTransform.Position.Y, MinHeight), cameraTransform.Position.Z);

                cameraTransform.LookAt(cameraTransform.Parent != null ? cameraTransform.Parent.Position + FocusPoint : FocusPoint);
            }
        }

        private void SetFocus(Transform? parent)
        {
            cameraTransform.Parent = parent;
            cameraTransform.Position = new Vector3D(0, 5, 5); // TODO make teleportation more fluid
            cameraTransform.LookAt(cameraTransform.Parent != null ? cameraTransform.Parent.Position + FocusPoint : FocusPoint);
        }

        public override void OnUpdate()
        {
            SelectedTarget = Selectable.Selected;
            if (SelectedTarget != null && SelectedTarget != LastSelectedTarget)
            {
                LastSelectedTarget = SelectedTarget;
                SetFocus(SelectedTarget?.Entity?.GetComponent<Transform>());
            }

            ProcessZoom();
            ProcessOrbit();
        }
        public override void OnPhysicsUpdate() { }
    }
}