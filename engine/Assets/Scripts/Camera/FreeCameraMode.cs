
using Synthesis.UI.Dynamic;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;
using Input = UnityEngine.Input;

#nullable enable

public class FreeCameraMode : ICameraMode {
    public float TargetZoom { get; private set; }  = 15.0f;
    public float TargetPitch { get; private set; } = 10.0f;
    public float TargetYaw { get; private set; }   = 135.0f;
    public float ActualZoom { get; private set; }  = 15.0f;
    public float ActualPitch { get; private set; } = 10.0f;
    public float ActualYaw { get; private set; }   = 135.0f;

    private const string FORWARD_KEY    = "FREECAM_FORWARD";
    private const string BACK_KEY       = "FREECAM_BACK";
    private const string LEFT_KEY       = "FREECAM_LEFT";
    private const string RIGHT_KEY      = "FREECAM_RIGHT";
    private const string LEFT_YAW_KEY   = "FREECAM_LEFT_YAW";
    private const string RIGHT_YAW_KEY  = "FREECAM_RIGHT_YAW";
    private const string DOWN_PITCH_KEY = "FREECAM_DOWN_PITCH";
    private const string UP_PITCH_KEY   = "FREECAM_UP_PITCH";

    private bool isActive = false;

    private CameraController _controller;

    public void Start<T>(CameraController cam, T? previousCam)
        where T : ICameraMode {
        // only assign inputs once
        if (!InputManager.MappedDigitalInputs.ContainsKey(FORWARD_KEY)) {
            InputManager.AssignDigitalInput(FORWARD_KEY, new Digital("W"));
            InputManager.AssignDigitalInput(BACK_KEY, new Digital("S"));
            InputManager.AssignDigitalInput(LEFT_KEY, new Digital("A"));
            InputManager.AssignDigitalInput(RIGHT_KEY, new Digital("D"));
        }

        if (previousCam != null) {
            if (previousCam.GetType() == typeof(OrbitCameraMode)) {
                OrbitCameraMode orbitCam = (previousCam as OrbitCameraMode)!;
                TargetPitch              = orbitCam.TargetPitch;
                TargetYaw                = orbitCam.TargetYaw;
                ActualPitch              = orbitCam.ActualPitch;
                ActualYaw                = orbitCam.ActualYaw;
            } else if (previousCam.GetType() == typeof(DriverStationCameraMode)) {
                DriverStationCameraMode driverStationCam = (previousCam as DriverStationCameraMode)!;
                TargetPitch                              = driverStationCam.TargetPitch;
                TargetYaw                                = driverStationCam.TargetYaw;
                ActualPitch                              = driverStationCam.ActualPitch;
                ActualYaw                                = driverStationCam.ActualYaw;
            }
        }

        _controller = cam;
    }

    public void Update(CameraController cam) {
        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            SetActive(true);
        } else if (Input.GetKeyUp(KeyCode.Mouse1)) {
            SetActive(false);
        }

        // don't allow camera movement when a modal is open
        if (DynamicUIManager.ActiveModal != null)
            return;
        float p = 0.0f;
        float y = 0.0f;
        float z = 0.0f;

        if (isActive) {
            p = -CameraController.PitchSensitivity * Input.GetAxis("Mouse Y");
            y = CameraController.YawSensitivity * Input.GetAxis("Mouse X");
        }

        // make it so the user can't rotate the camera upside down
        TargetPitch = Mathf.Clamp(TargetPitch + p, -90, 90);
        TargetYaw += y;
        TargetZoom = Mathf.Clamp(TargetZoom + z, cam.ZoomLowerLimit, cam.ZoomUpperLimit);

        float orbitLerpFactor = Mathf.Clamp((cam.OrbitalAcceleration * Time.deltaTime) / 0.018f, 0.01f, 1.0f);
        ActualPitch           = Mathf.Lerp(ActualPitch, TargetPitch, orbitLerpFactor);
        ActualYaw             = Mathf.Lerp(ActualYaw, TargetYaw, orbitLerpFactor);
        float zoomLerpFactor  = Mathf.Clamp((cam.ZoomAcceleration * Time.deltaTime) / 0.018f, 0.01f, 1.0f);
        ActualZoom            = Mathf.Lerp(ActualZoom, TargetZoom, zoomLerpFactor);

        var t = cam.transform;

        float speed = 10.0F;

        // transform forwards and backwards when forward and backward inputs are pressed
        // left and right when left and right are pressed

        Vector3 forward = Vector3.zero, right = Vector3.zero;

        if (isActive) {
            forward = t.forward * (InputManager.MappedDigitalInputs[FORWARD_KEY][0].Value -
                                      InputManager.MappedDigitalInputs[BACK_KEY][0].Value) +
                      t.forward * (TargetZoom - ActualZoom) * CameraController.ZoomSensitivity;

            right = t.right * (InputManager.MappedDigitalInputs[RIGHT_KEY][0].Value -
                                  InputManager.MappedDigitalInputs[LEFT_KEY][0].Value);

            t.Translate(Time.deltaTime * speed * (forward + right), Space.World);

            // we don't want the user to be able to move the camera under the map or so high they can't see the field
            t.position = new Vector3(t.position.x, Mathf.Clamp(t.position.y, 0, 100), t.position.z);

            t.localRotation = Quaternion.Euler(ActualPitch, ActualYaw, 0.0f);
        }
    }

    public void LateUpdate(CameraController cam) {
        cam.GroundRenderer.material.SetVector("_GridFocusPoint", cam.transform.position);
    }

    public void SetTransform(Vector3 position, Quaternion rotation) {
        _controller.transform.position = position;
        _controller.transform.rotation = rotation;
        var euler                      = rotation.eulerAngles;
        TargetPitch = ActualPitch = euler.x;
        TargetYaw = ActualYaw = euler.y;
    }

    public void SetActive(bool active) {
        isActive = active;
        if (active) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible   = false;
            if (RobotSimObject.CurrentlyPossessedRobot != string.Empty)
                RobotSimObject.GetCurrentlyPossessedRobot().BehavioursEnabled = false;
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible   = true;
            if (RobotSimObject.CurrentlyPossessedRobot != string.Empty)
                RobotSimObject.GetCurrentlyPossessedRobot().BehavioursEnabled = true;
        }
    }

    public void End(CameraController cam) {
        SetActive(false);
    }
}