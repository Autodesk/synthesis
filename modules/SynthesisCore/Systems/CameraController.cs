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
        public static float SensitivityX { get => 2; } // TODO: integrate with preference manager
        public static float SensitivityY { get => 2; }
        public static float SensitivityZoom { get => 3; }

        private bool inFreeRoamMode = true;

        private bool isMouseDragging = false;

        private Vector3D focusPoint = new Vector3D(); // Default focus point
        private Vector3D offset = new Vector3D();

        private Entity? cameraEntity = null;
        private Transform? cameraTransform = null;

        /// <summary>
        /// An optional target to focus on
        /// </summary>
        private static Selectable? SelectedTarget = null;
        private static Selectable? LastSelectedTarget = null;

        private float xMod = 0, yMod = 0, zMod = 0, lastXMod = 0, lastYMod = 0, lastZMod = 0; // Used for accelerating the camera movement speed

        private const double MinDistance = 0.25;
        private const double MaxDistance = 50;
        private const double MinHeight = 0.25;
        private const double FreeRoamCameraMoveDelta = 0.03;

        public override void Setup()
        {
            if (cameraEntity == null)
            {
                cameraEntity = EnvironmentManager.AddEntity();
                cameraEntity?.AddComponent<Camera>();
                cameraTransform = cameraEntity?.AddComponent<Transform>();
                SetNewFocus(new Vector3D());
            }
            // Bind controls for free roam
            InputManager.AssignDigital("CameraForward", (KeyDigital)"W", CameraForward);
            InputManager.AssignDigital("CameraLeft", (KeyDigital)"A", CameraLeft);
            InputManager.AssignDigital("CameraBackward", (KeyDigital)"S", CameraBackward);
            InputManager.AssignDigital("CameraRight", (KeyDigital)"D", CameraRight);
            InputManager.AssignDigital("CameraUp", (KeyDigital)"Space", CameraUp);
            InputManager.AssignDigital("CameraDown", (KeyDigital)"LeftShift", CameraDown);

            // Bind controls for orbit
            InputManager.AssignDigital("UseOrbit", (KeyDigital)"Mouse0", StartMouseDrag); // TODO put control settings in preference manager
            InputManager.AssignAxis("ZoomCamera", (DualAxis)"Mouse ScrollWheel");
        }

        public void CameraForward(IEvent e)
        {
            if (inFreeRoamMode && cameraTransform != null) // TODO allow free roam movement in orbit, but still LootAt focus?
            {
                if (e is DigitalStateEvent de)
                {
                    if (de.KeyState == DigitalState.Held)
                        cameraTransform.Position += new Vector3D(0, 0, -FreeRoamCameraMoveDelta); // TODO make relative to forward not world
                }
                else
                {
                    throw new System.Exception();
                }
            }
        }

        public void CameraBackward(IEvent e)
        {
            if (inFreeRoamMode && cameraTransform != null)
            {
                if (e is DigitalStateEvent de)
                {
                    if(de.KeyState == DigitalState.Held)
                        cameraTransform.Position += new Vector3D(0, 0, FreeRoamCameraMoveDelta);
                }
                else
                {
                    throw new System.Exception();
                }
            }
        }

        public void CameraLeft(IEvent e)
        {
            if (inFreeRoamMode && cameraTransform != null)
            {
                if (e is DigitalStateEvent de)
                {
                    if (de.KeyState == DigitalState.Held)
                        cameraTransform.Position += new Vector3D(FreeRoamCameraMoveDelta, 0, 0);
                }
                else
                {
                    throw new System.Exception();
                }
            }
        }

        public void CameraRight(IEvent e)
        {
            if (inFreeRoamMode && cameraTransform != null)
            {
                if (e is DigitalStateEvent de)
                {
                    if (de.KeyState == DigitalState.Held)
                        cameraTransform.Position += new Vector3D(-FreeRoamCameraMoveDelta, 0, 0);
                }
                else
                {
                    throw new System.Exception();
                }
            }
        }

        public void CameraUp(IEvent e)
        {
            if (inFreeRoamMode && cameraTransform != null)
            {
                if (e is DigitalStateEvent de)
                {
                    if (de.KeyState == DigitalState.Held)
                        cameraTransform.Position += new Vector3D(0, FreeRoamCameraMoveDelta, 0);
                }
                else
                {
                    throw new System.Exception();
                }
            }
        }

        public void CameraDown(IEvent e)
        {
            if (inFreeRoamMode && cameraTransform != null)
            {
                if (e is DigitalStateEvent de)
                {
                    if (de.KeyState == DigitalState.Held)
                        cameraTransform.Position += new Vector3D(0, -FreeRoamCameraMoveDelta, 0);
                }
                else
                {
                    throw new System.Exception();
                }
            }
        }

        /// <summary>
        /// Function used to switch orbit control on and off
        /// </summary>
        /// <param name="e"></param>
        public void StartMouseDrag(IEvent e)
        {
            if (e is DigitalStateEvent de)
            {
                if (de.KeyState == DigitalState.Down)
                {
                    isMouseDragging = true;
                    // TODO cursor stuff
                    // Cursor.lockState = CursorLockMode.Locked; // Hide and lock cursor so the mouse doesn't leave the screen
                    // Cursor.visible = false;
                }
                else if (de.KeyState == DigitalState.Up)
                {
                    isMouseDragging = false;
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
            if (zMod != 0)
            {
                // Prevent from moving too close or too far away from focus
                if ((offset.Length > MinDistance && zMod < 0) || (offset.Length < MaxDistance && zMod > 0))
                {
                    offset += offset.Normalize().ToVector3D().ScaleBy(-zMod);
                }
            }
        }

        private void ProcessOrbit()
        {
            if (isMouseDragging)
            {
                // Rotate horizontally (i.e. around y-axis)
                var xDelta = offset.Rotate(UnitVector3D.YAxis, Angle.FromDegrees(-xMod)) - offset;

                // Rotate vertically
                var verticalRotationAxis = offset.CrossProduct(UnitVector3D.YAxis).Normalize();
                Vector3D yDelta = offset.Rotate(verticalRotationAxis, Angle.FromDegrees(-yMod)) - offset;

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
            }
        }

        private void UpdateOrbitCameraPosition()
        {
            if (cameraTransform != null) {
                cameraTransform.Position = focusPoint + offset;
                cameraTransform.LookAt(focusPoint);
            }
        }

        private void ProcessFreeRoam()
        {
            if (cameraTransform != null)
            {
                if (isMouseDragging)
                {
                    cameraTransform.Rotate(cameraTransform.Forward.CrossProduct(UnitVector3D.YAxis), yMod);
                    cameraTransform.Rotate(UnitVector3D.YAxis, xMod, true);
                }
                cameraTransform.Position += zMod * cameraTransform.Forward;
            }
        }

        private void SetNewFocus(Vector3D newFocusPoint)
        {
            focusPoint = newFocusPoint;
            offset = new Vector3D(0, 5, 5); // TODO make teleportation more fluid
            UpdateOrbitCameraPosition();
        }

        private void UpdateMouseInput()
        {
            zMod = InputManager.GetAxisValue("ZoomCamera") * SensitivityZoom;

            if (zMod != 0 && (zMod < 0) == (lastZMod < 0))
                zMod += lastZMod * 0.5f;

            lastZMod = zMod == 0 ? lastZMod : zMod;

            if (isMouseDragging)
            {
                // Add an intertial effect to camera movement (TODO use actual last cameraTransform.Position delta instead?)
                xMod = -InputManager.GetAxisValue("MouseX") * SensitivityX;
                yMod = InputManager.GetAxisValue("MouseY") * SensitivityY;

                if (xMod != 0 && (xMod < 0) == (lastXMod < 0))
                    xMod += lastXMod * 0.3f;
                if (yMod != 0 && (yMod < 0) == (lastYMod < 0))
                    yMod += lastYMod * 0.3f;

                lastXMod = xMod == 0 ? lastXMod : xMod;
                lastYMod = yMod == 0 ? lastYMod : yMod;
            }
        }

        public override void OnUpdate()
        {
            UpdateMouseInput();

            SelectedTarget = Selectable.Selected;
            inFreeRoamMode = SelectedTarget == null;
            if (inFreeRoamMode) // Free roam mode
            {
                ProcessFreeRoam();
            }
            else // Orbit mode
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

                ProcessZoom();
                ProcessOrbit();
                UpdateOrbitCameraPosition();
            }
        }
        public override void OnPhysicsUpdate() { }
    }
}