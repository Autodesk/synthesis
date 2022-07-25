
using System;
using System.Linq;
using SynthesisAPI.InputManager;
using SynthesisAPI.InputManager.Inputs;
using UnityEngine;
using Input = UnityEngine.Input;

public class FreeCameraMode : ICameraMode
{
    private float _targetZoom = 15.0f;
    private float _targetPitch = 10.0f;
    private float _targetYaw = 135.0f;
    private float _actualZoom = 5.0f;
    private float _actualPitch = 0.0f;
    private float _actualYaw = 0.0f;

    private const string FORWARD_KEY = "input/FREECAM_FORWARD";
    private const string BACK_KEY = "input/FREECAM_BACK";
    private const string LEFT_KEY = "input/FREECAM_LEFT";
    private const string RIGHT_KEY = "input/FREECAM_RIGHT";
    private const string UP_KEY = "input/FREECAM_UP";
    private const string DOWN_KEY = "input/FREECAM_DOWN";
    private const string LEFT_YAW_KEY = "input/FREECAM_LEFT_YAW";
    private const string RIGHT_YAW_KEY = "input/FREECAM_RIGHT_YAW";
    private const string DOWN_PITCH_KEY = "input/FREECAM_DOWN_PITCH";
    private const string UP_PITCH_KEY = "input/FREECAM_UP_PITCH";

    public void Start(CameraController cam)
    {
        // only assign inputs once
        if (!InputManager.MappedDigitalInputs.ContainsKey(FORWARD_KEY))
        {
            InputManager.AssignDigitalInput(FORWARD_KEY, new Digital("UpArrow"));
            InputManager.AssignDigitalInput(BACK_KEY, new Digital("DownArrow"));
            InputManager.AssignDigitalInput(LEFT_KEY, new Digital("LeftArrow"));
            InputManager.AssignDigitalInput(RIGHT_KEY, new Digital("RightArrow"));
            InputManager.AssignDigitalInput(UP_KEY, new Digital("Space"));
            InputManager.AssignDigitalInput(DOWN_KEY, new Digital("LeftShift"));
            InputManager.AssignValueInput(LEFT_YAW_KEY, new Digital("Q"));
            InputManager.AssignValueInput(RIGHT_YAW_KEY, new Digital("E"));
            InputManager.AssignValueInput(DOWN_PITCH_KEY, new Digital("Z"));
            InputManager.AssignValueInput(UP_PITCH_KEY, new Digital("X"));
        }
    }
    
    public void Update(CameraController cam)
    {
        float p = 0.0f;
        float y = 0.0f;
        float z = 0.0f;
        
        // in old synthesis freecam mode, scrolling down zooms in and scrolling up zooms out
        z = cam.ZoomSensitivity * Input.mouseScrollDelta.y;
        
        float yawMod = InputManager.MappedValueInputs.ContainsKey(LEFT_YAW_KEY) && InputManager.MappedValueInputs.ContainsKey(RIGHT_YAW_KEY) ? 
            cam.YawSensitivity / 8 * (InputManager.MappedValueInputs[RIGHT_YAW_KEY].Value - InputManager.MappedValueInputs[LEFT_YAW_KEY].Value) : 0;
        float pitchMod = InputManager.MappedValueInputs.ContainsKey(UP_PITCH_KEY) && InputManager.MappedValueInputs.ContainsKey(DOWN_PITCH_KEY) ? 
            cam.PitchSensitivity / 8 * (InputManager.MappedValueInputs[UP_PITCH_KEY].Value - InputManager.MappedValueInputs[DOWN_PITCH_KEY].Value) : 0;
        
        p -= pitchMod;
        y += yawMod;

        if (Input.GetKey(KeyCode.Mouse0))
        {
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
    }

    public void LateUpdate(CameraController cam)
    {
        var t = cam.transform;

        float speed = 10.0F;
        
        // transform forwards and backwards when forward and backward inputs are pressed
        // left and right when left and right are pressed
        Vector3 forward = t.forward * (InputManager.MappedDigitalInputs[FORWARD_KEY][0].Value -
                                       InputManager.MappedDigitalInputs[BACK_KEY][0].Value) +
                          t.forward * (_targetZoom - _actualZoom) * cam.ZoomSensitivity;
        
        Vector3 right = t.right * (InputManager.MappedDigitalInputs[RIGHT_KEY][0].Value -
                                   InputManager.MappedDigitalInputs[LEFT_KEY][0].Value);
        
        Vector3 up = Vector3.up * (InputManager.MappedDigitalInputs[UP_KEY][0].Value -
                             InputManager.MappedDigitalInputs[DOWN_KEY][0].Value);
        
        t.Translate(Time.deltaTime * speed * (forward + right + up),Space.World);

        // we don't want the user to be able to move the camera under the map or so high they can't see the field
        t.position = new Vector3(t.position.x, Mathf.Clamp(t.position.y, 0, 100), t.position.z);

        t.localRotation = Quaternion.Euler(_actualPitch, _actualYaw, 0.0f);
    }
}