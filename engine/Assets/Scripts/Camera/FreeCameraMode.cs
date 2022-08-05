
using Synthesis.UI.Dynamic;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;
using Input = UnityEngine.Input;

public class FreeCameraMode : ICameraMode
{
    private float _targetZoom = 15.0f;
    private float _targetPitch = 10.0f;
    private float _targetYaw = 135.0f;
    private float _actualZoom = 15.0f;
    private float _actualPitch = 10.0f;
    private float _actualYaw = 135.0f;

    private const string FORWARD_KEY = "FREECAM_FORWARD";
    private const string BACK_KEY = "FREECAM_BACK";
    private const string LEFT_KEY = "FREECAM_LEFT";
    private const string RIGHT_KEY = "FREECAM_RIGHT";
    private const string LEFT_YAW_KEY = "FREECAM_LEFT_YAW";
    private const string RIGHT_YAW_KEY = "FREECAM_RIGHT_YAW";
    private const string DOWN_PITCH_KEY = "FREECAM_DOWN_PITCH";
    private const string UP_PITCH_KEY = "FREECAM_UP_PITCH";

    private bool isActive = false;

    public void Start(CameraController cam) {
        
        // only assign inputs once
        if (!InputManager.MappedDigitalInputs.ContainsKey(FORWARD_KEY)) {
            InputManager.AssignDigitalInput(FORWARD_KEY, new Digital("W"));
            InputManager.AssignDigitalInput(BACK_KEY, new Digital("S"));
            InputManager.AssignDigitalInput(LEFT_KEY, new Digital("A"));
            InputManager.AssignDigitalInput(RIGHT_KEY, new Digital("D"));
            // InputManager.AssignValueInput(LEFT_YAW_KEY, new Digital("Q"));
            // InputManager.AssignValueInput(RIGHT_YAW_KEY, new Digital("E"));
            // InputManager.AssignValueInput(DOWN_PITCH_KEY, new Digital("Z"));
            // InputManager.AssignValueInput(UP_PITCH_KEY, new Digital("X"));
        }
    }
    
    public void Update(CameraController cam) {
        if (Input.GetKeyDown(KeyCode.Mouse1)) {
            SetActive(true);
        } else if (Input.GetKeyUp(KeyCode.Mouse1)) {
            SetActive(false);
        }

        // don't allow camera movement when a modal is open
        if (DynamicUIManager.ActiveModal != null) return;
        float p = 0.0f;
        float y = 0.0f;
        float z = 0.0f;
        
        // in old synthesis freecam mode, scrolling down zooms in and scrolling up zooms out
        // z = cam.ZoomSensitivity * Input.mouseScrollDelta.y;
        
        // float yawMod = InputManager.MappedValueInputs.ContainsKey(LEFT_YAW_KEY) && InputManager.MappedValueInputs.ContainsKey(RIGHT_YAW_KEY) ? 
        //     cam.YawSensitivity / 8 * (InputManager.MappedValueInputs[RIGHT_YAW_KEY].Value - InputManager.MappedValueInputs[LEFT_YAW_KEY].Value) : 0;
        // float pitchMod = InputManager.MappedValueInputs.ContainsKey(UP_PITCH_KEY) && InputManager.MappedValueInputs.ContainsKey(DOWN_PITCH_KEY) ? 
        //     cam.PitchSensitivity / 4 * (InputManager.MappedValueInputs[UP_PITCH_KEY].Value - InputManager.MappedValueInputs[DOWN_PITCH_KEY].Value) : 0;
        
        // p -= pitchMod;
        // y += yawMod;

        if (isActive) {
            p = -cam.PitchSensitivity * Input.GetAxis("Mouse Y");
            y = cam.YawSensitivity * Input.GetAxis("Mouse X");
        }

        // make it so the user can't rotate the camera upside down
        _targetPitch = Mathf.Clamp(_targetPitch + p, -90, 90);
        _targetYaw += y;
        _targetZoom = Mathf.Clamp(_targetZoom + z, cam.ZoomLowerLimit, cam.ZoomUpperLimit);
        
        float orbitLerpFactor = Mathf.Clamp((cam.OrbitalAcceleration * Time.deltaTime) / 0.018f, 0.01f, 1.0f);
        _actualPitch = Mathf.Lerp(_actualPitch, _targetPitch, orbitLerpFactor);
        _actualYaw = Mathf.Lerp(_actualYaw, _targetYaw, orbitLerpFactor);
        float zoomLerpFactor = Mathf.Clamp((cam.ZoomAcceleration * Time.deltaTime) / 0.018f, 0.01f, 1.0f);
        _actualZoom = Mathf.Lerp(_actualZoom, _targetZoom, zoomLerpFactor);

        var t = cam.transform;

        float speed = 10.0F;
        
        // transform forwards and backwards when forward and backward inputs are pressed
        // left and right when left and right are pressed

        Vector3 forward = Vector3.zero, right = Vector3.zero;

        if (isActive) {
            forward = t.forward * (InputManager.MappedDigitalInputs[FORWARD_KEY][0].Value -
                                        InputManager.MappedDigitalInputs[BACK_KEY][0].Value) +
                            t.forward * (_targetZoom - _actualZoom) * cam.ZoomSensitivity;
            
            right = t.right * (InputManager.MappedDigitalInputs[RIGHT_KEY][0].Value -
                                    InputManager.MappedDigitalInputs[LEFT_KEY][0].Value);
        }

        t.Translate(Time.deltaTime * speed * (forward + right),Space.World);

        // we don't want the user to be able to move the camera under the map or so high they can't see the field
        t.position = new Vector3(t.position.x, Mathf.Clamp(t.position.y, 0, 100), t.position.z);

        t.localRotation = Quaternion.Euler(_actualPitch, _actualYaw, 0.0f);
    }

    public void LateUpdate(CameraController cam) {

        cam.GroundRenderer.material.SetVector("FOCUS_POINT", cam.transform.position);

        // // don't allow camera movement when a modal is open
        // if (DynamicUIManager.ActiveModal != null) return;
        
    }

    public void SetActive(bool active) {
        isActive = active;
        if (active) {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            if (RobotSimObject.CurrentlyPossessedRobot != string.Empty)
                RobotSimObject.GetCurrentlyPossessedRobot().BehavioursEnabled = false;
        } else {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            if (RobotSimObject.CurrentlyPossessedRobot != string.Empty)
                RobotSimObject.GetCurrentlyPossessedRobot().BehavioursEnabled = true;
        }
    }

    public void End(CameraController cam) {
        SetActive(false);
        // RobotSimObject.GetCurrentlyPossessedRobot().InputFrozen = _wasFrozen;
    }
}