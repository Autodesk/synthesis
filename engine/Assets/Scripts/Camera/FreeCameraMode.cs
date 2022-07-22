
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

    private const string ForwardKey = "FREECAM_FORWARD";
    private const string BackKey = "FREECAM_BACK";
    private const string LeftKey = "FREECAM_LEFT";
    private const string RightKey = "FREECAM_RIGHT";

    public void Start(CameraController cam)
    {
        InputManager.AssignDigitalInput(ForwardKey, new Digital("UpArrow"));
        InputManager.AssignDigitalInput(BackKey, new Digital("DownArrow"));
        InputManager.AssignDigitalInput(LeftKey, new Digital("LeftArrow"));
        InputManager.AssignDigitalInput(RightKey, new Digital("RightArrow"));
    }
    
    public void Update(CameraController cam)
    {
        float p = 0.0f;
        float y = 0.0f;
        float z = 0.0f;
        
        z = cam.ZoomSensitivity * -Input.mouseScrollDelta.y;

        if (Input.GetKey(KeyCode.Mouse0))
        {
            p = -cam.PitchSensitivity * Input.GetAxis("Mouse Y");
            y = cam.YawSensitivity * Input.GetAxis("Mouse X");
        }
        
        _targetPitch = Mathf.Clamp(_targetPitch + p, cam.PitchLowerLimit, cam.PitchUpperLimit);
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
        t.Translate(Time.deltaTime * speed * (
            t.forward * (InputManager._mappedDigitalInputs[ForwardKey][0].Value - InputManager._mappedDigitalInputs[BackKey][0].Value) +
            t.right * (InputManager._mappedDigitalInputs[RightKey][0].Value - InputManager._mappedDigitalInputs[LeftKey][0].Value)), Space.World);

        t.localRotation = Quaternion.Euler(_actualPitch, _actualYaw, 0.0f);
    }
}