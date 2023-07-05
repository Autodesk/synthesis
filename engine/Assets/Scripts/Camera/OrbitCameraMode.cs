using System;
using Synthesis.UI.Dynamic;
using SynthesisAPI.InputManager;
using UnityEngine;
using UnityEngine.EventSystems;

#nullable enable

public class OrbitCameraMode : ICameraMode {
    // [SerializeField] public Transform FollowTransform;
    public static Func<Vector3> FocusPoint = () => Vector3.zero;

    public float TargetZoom { get; private set; }  = 8.0f;
    public float TargetPitch { get; private set; } = 10.0f;
    public float TargetYaw { get; private set; }   = 135.0f;
    public float ActualZoom { get; private set; }  = 4.0f;
    public float ActualPitch { get; private set; } = 0.0f;

    public float ActualYaw { get; private set; } = 0.0f;
    private bool _useOrbit                       = false;

    public void Start<T>(CameraController cam, T? previousCam)
        where T : ICameraMode {
        if (previousCam != null) {
            if (previousCam.GetType() == typeof(FreeCameraMode)) {
                FreeCameraMode freeCam = (previousCam as FreeCameraMode)!;
                ActualPitch            = freeCam.ActualPitch;
                ActualYaw              = freeCam.ActualYaw;
            } else if (previousCam.GetType() == typeof(DriverStationCameraMode)) {
                DriverStationCameraMode driverCam = (previousCam as DriverStationCameraMode)!;
                ActualPitch                       = driverCam.ActualPitch;
                ActualYaw                         = driverCam.ActualYaw;
            }
        }
    }

    public void Update(CameraController cam) {
        // don't allow camera movement when a modal is open
        if (DynamicUIManager.ActiveModal != null)
            return;
        var pitchTest = cam.PitchUpperLimit - cam.PitchLowerLimit;
        if (pitchTest < 0)
            Debug.LogError("No range exists for pitch to reside in");
        else if (pitchTest == 0)
            Debug.Log("Pitch is locked");

        float p = 0.0f;
        float y = 0.0f;
        float z = 0.0f;

        bool isOverUI    = EventSystem.current.IsPointerOverGameObject();
        bool enableOrbit = !isOverUI && !CameraController.isOverGizmo;
        bool isGodMode   = InputManager.MappedValueInputs.ContainsKey(GodMode.ENABLED_GOD_MODE_INPUT)
                               ? InputManager.MappedValueInputs[GodMode.ENABLED_GOD_MODE_INPUT].Value == 1.0F
                               : false;

        if (enableOrbit && !isGodMode) {
            z = CameraController.ZoomSensitivity * -Input.mouseScrollDelta.y;

            // UNCOMMENT OUT TO ENABLE CURSOR-LOCKING WHEN ORBITING
            /*
            if (Input.GetKeyDown(KeyCode.Mouse0)) {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            } else if (Input.GetKeyUp(KeyCode.Mouse0)) {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }*/
        }

        if (!Input.GetKey(KeyCode.Mouse0) || isGodMode) {
            _useOrbit = enableOrbit;
        } else {
            if (_useOrbit) {
                p = -CameraController.PitchSensitivity * Input.GetAxis("Mouse Y");
                y = CameraController.YawSensitivity * Input.GetAxis("Mouse X");
            }
        }

        TargetPitch = Mathf.Clamp(TargetPitch + p, cam.PitchLowerLimit, cam.PitchUpperLimit);
        TargetYaw += y;
        TargetZoom = Mathf.Clamp(TargetZoom + z, cam.ZoomLowerLimit, cam.ZoomUpperLimit);

        float orbitLerpFactor = Mathf.Clamp((cam.OrbitalAcceleration * Time.deltaTime) / 0.018f, 0.01f, 1.0f);
        ActualPitch           = Mathf.Lerp(ActualPitch, TargetPitch, orbitLerpFactor);
        ActualYaw             = Mathf.Lerp(ActualYaw, TargetYaw, orbitLerpFactor);
        float zoomLerpFactor  = Mathf.Clamp((cam.ZoomAcceleration * Time.deltaTime) / 0.018f, 0.01f, 1.0f);
        ActualZoom            = Mathf.Lerp(ActualZoom, TargetZoom, zoomLerpFactor);
    }

    public void LateUpdate(CameraController cam) {
        // Construct orientation of the camera
        Vector3 focus = FocusPoint == null ? Vector3.zero : FocusPoint();

        cam.GroundRenderer.material.SetVector("_GridFocusPoint", focus);

        var t           = cam.transform;
        t.localPosition = focus;
        t.localRotation = Quaternion.identity;

        var up          = t.up;
        t.localRotation = Quaternion.Euler(ActualPitch, 0.0f, 0.0f);
        t.RotateAround(focus, up, ActualYaw);
        t.localPosition = (/*up * 0.5f +*/ t.forward * -ActualZoom) + t.localPosition;
    }

    public void End(CameraController cam) {}
}