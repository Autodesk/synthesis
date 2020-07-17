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

        private Vector3D focusPoint = new Vector3D(); // Default focus point
        private Vector3D offset = new Vector3D();

        private Entity? cameraEntity = null;
        private Transform? cameraTransform = null;

        /// <summary>
        /// An optional target to focus on
        /// </summary>
        private static Selectable? SelectedTarget = null;
        private static Selectable? LastSelectedTarget = null;

        private float lastXMod = 0, lastYMod = 0, lastDistMod = 0; // Used for accelerating the camera movement speed

        private const double MinDistance = 0.25;
        private const double MaxDistance = 50;
        private const double MinHeight = 0.25;

        public override void Setup()
        {
            if (cameraEntity == null)
            {
                cameraEntity = EnvironmentManager.AddEntity();
                cameraEntity?.AddComponent<Camera>();
                cameraTransform = cameraEntity?.AddComponent<Transform>();
                SetNewFocus(new Vector3D());
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
            }
            else
            {
                throw new System.Exception();
            }
        }

        private void ProcessZoom()
        {
            // TODO: Accelerate the scroll wheel even after the user briefly stops scrolling
            float distMod = -InputManager.GetAxisValue("ZoomCamera") * SensitivityZoom;
            if (distMod != 0)
            {
                if ((distMod < 0) == (lastDistMod < 0)) // Check that lastDistMod is in the same direction as distMod
                {
                    distMod += lastDistMod * 0.5f; // Give some kind of intertial effect
                }

                // Prevent from moving too close or too far away from focus
                if ((offset.Length > MinDistance && distMod < 0) || (offset.Length < MaxDistance && distMod > 0))
                {
                    offset += offset.Normalize().ToVector3D().ScaleBy(distMod);
                }
            }
            lastDistMod = distMod;
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
                var xDelta = offset.Rotate(UnitVector3D.YAxis, Angle.FromDegrees(xMod * SensitivityX)) - offset;

                // Rotate vertically
                var verticalRotationAxis = offset.CrossProduct(UnitVector3D.YAxis).Normalize();
                Vector3D yDelta = offset.Rotate(verticalRotationAxis, Angle.FromDegrees(yMod * SensitivityY)) - offset;

                // Stop from vertically rotating past directly above the focus point
                var newPosition = offset + yDelta;
                if (newPosition.AngleTo(UnitVector3D.YAxis) <= Angle.FromDegrees(2)) // TODO this doesn't catch cases where it jumps to the otherside and is more than 2 degrees away
                {
                    yDelta = new Vector3D(0, 0, 0);
                }

                offset += xDelta + yDelta;

                // Stop from vertically rotating below the floor 
                var newPos = focusPoint + offset;
                newPos = new Vector3D(newPos.X, Math.Max(newPos.Y, MinHeight), newPos.Z);
                offset = newPos - focusPoint;

                UpdateCameraPosition();
            }
        }

        private void UpdateCameraPosition()
        {
            if (cameraTransform != null) {
                cameraTransform.Position = focusPoint + offset;
                cameraTransform.LookAt(focusPoint);
            }
        }

        private void SetNewFocus(Vector3D newFocusPoint)
        {
            focusPoint = newFocusPoint;
            offset = new Vector3D(0, 5, 5); // TODO make teleportation more fluid
            UpdateCameraPosition();
        }

        public override void OnUpdate()
        {
            SelectedTarget = Selectable.Selected;
            if (SelectedTarget != null)
            {
                var newFocusPoint = SelectedTarget?.Entity?.GetComponent<Transform>()?.Position;
                if (newFocusPoint.HasValue)
                {
                    if (SelectedTarget != LastSelectedTarget) // Set new focus point
                    {
                        LastSelectedTarget = SelectedTarget;
                        SetNewFocus(newFocusPoint.Value);
                    }
                    else // Update possibly moving focus point
                    {
                        focusPoint = newFocusPoint.Value;
                    }
                }
            }

            ProcessZoom();
            ProcessOrbit();
            UpdateCameraPosition();
        }
        public override void OnPhysicsUpdate() { }
    }
}