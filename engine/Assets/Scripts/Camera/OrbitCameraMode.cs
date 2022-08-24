using System;
using Synthesis.UI.Dynamic;
using SynthesisAPI.InputManager;
using UnityEngine;
using UnityEngine.EventSystems;

public class OrbitCameraMode : ICameraMode
{
    // [SerializeField] public Transform FollowTransform;
    public static Func<Vector3> FocusPoint = () => Vector3.zero;

    private float _targetZoom = 15.0f;
    private float _targetPitch = 10.0f;
    private float _targetYaw = 135.0f;
    private float _actualZoom = 5.0f;
    private float _actualPitch = 0.0f;
    private float _actualYaw = 0.0f;
    private bool _useOrbit = false;

    public void Start(CameraController cam)
    {
        
    }

    public void Update(CameraController cam)
    {
        // don't allow camera movement when a modal is open
        if (DynamicUIManager.ActiveModal != null) return;
        var pitchTest = cam.PitchUpperLimit - cam.PitchLowerLimit;
        if (pitchTest < 0)
            Debug.LogError("No range exists for pitch to reside in");
        else if (pitchTest == 0)
            Debug.Log("Pitch is locked");

        float p = 0.0f;
        float y = 0.0f;
        float z = 0.0f;
        
        bool isOverUI = EventSystem.current.IsPointerOverGameObject();
        bool enableOrbit = !isOverUI && !CameraController.isOverGizmo;
        bool isGodMode = InputManager.MappedValueInputs.ContainsKey(GodMode.ENABLED_GOD_MODE_INPUT)
            ? InputManager.MappedValueInputs[GodMode.ENABLED_GOD_MODE_INPUT].Value == 1.0F
            : false;
        
        if (enableOrbit && !isGodMode) {
            z = CameraController.ZoomSensitivity * -Input.mouseScrollDelta.y;

            //UNCOMMENT OUT TO ENABLE CURSOR-LOCKING WHEN ORBITING
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
        // Construct orientation of the camera
        Vector3 focus = FocusPoint == null ? Vector3.zero : FocusPoint();

        cam.GroundRenderer.material.SetVector("FOCUS_POINT", focus);

        var t = cam.transform;
        t.localPosition = focus;
        t.localRotation = Quaternion.identity;

        var up = t.up;
        t.localRotation = Quaternion.Euler(_actualPitch, 0.0f, 0.0f);
        t.RotateAround(focus, up, _actualYaw);
        t.localPosition = (/*up * 0.5f +*/ t.forward * -_actualZoom) + t.localPosition;
    }
    
    public void End(CameraController cam) {}
}