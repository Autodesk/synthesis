using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour {
    
    [SerializeField, Range(1f, 15.0f)] public float PitchSensitivity;
    [SerializeField, Range(1f, 15.0f)] public float YawSensitivity;
    [SerializeField, Range(0.1f, 5f)] public float ZoomSensitivity;
    [SerializeField] public float PitchLowerLimit;
    [SerializeField] public float PitchUpperLimit;
    [SerializeField] public float ZoomLowerLimit;
    [SerializeField] public float ZoomUpperLimit;
    [SerializeField, Range(0.005f, 1.0f)] public float OrbitalAcceleration;
    [SerializeField, Range(0.005f, 1.0f)] public float ZoomAcceleration;
    [SerializeField] public Transform FollowTransform;
    public static bool isOverGizmo = false;

    private float _targetZoom = 5.0f;
    private float _targetPitch = 10.0f;
    private float _targetYaw = 135.0f;
    private float _actualZoom = 5.0f;
    private float _actualPitch = 0.0f;
    private float _actualYaw = 0.0f;
    private bool _useOrbit = false;
    
    public void Update() {
  //      if (FollowTransform != null && transform.parent != FollowTransform)
  //          transform.parent = FollowTransform;

        var pitchTest = PitchUpperLimit - PitchLowerLimit;
        if (pitchTest < 0)
            Debug.LogError("No range exists for pitch to reside in");
        else if (pitchTest == 0)
            Debug.Log("Pitch is locked");

        float p = 0.0f;
        float y = 0.0f;
        float z = 0.0f;
        
        bool isOverUI = EventSystem.current.IsPointerOverGameObject();
        bool enableOrbit = !isOverUI && !isOverGizmo;
        if (enableOrbit) {
            z = ZoomSensitivity * -Input.mouseScrollDelta.y;

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

        if (!Input.GetKey(KeyCode.Mouse0)) {
            _useOrbit = enableOrbit;
        } else {
            if (_useOrbit) {
                p = -PitchSensitivity * Input.GetAxis("Mouse Y");
                y = YawSensitivity * Input.GetAxis("Mouse X");
            }
        }

        _targetPitch = Mathf.Clamp(_targetPitch + p, PitchLowerLimit, PitchUpperLimit);
        _targetYaw += y;
        _targetZoom = Mathf.Clamp(_targetZoom + z, ZoomLowerLimit, ZoomUpperLimit);
        
        float orbitLerpFactor = Mathf.Clamp((OrbitalAcceleration * Time.deltaTime) / 0.018f, 0.01f, 1.0f);
        _actualPitch = Mathf.Lerp(_actualPitch, _targetPitch, orbitLerpFactor);
        _actualYaw = Mathf.Lerp(_actualYaw, _targetYaw, orbitLerpFactor);
        float zoomLerpFactor = Mathf.Clamp((ZoomAcceleration * Time.deltaTime) / 0.018f, 0.01f, 1.0f);
        _actualZoom = Mathf.Lerp(_actualZoom, _targetZoom, zoomLerpFactor);
    }

    public void LateUpdate() {
        // Construct orientation of the camera
        var t = transform;
        t.localPosition = FollowTransform.position;
        t.localRotation = Quaternion.identity;

        var up = t.up;
        t.localRotation = Quaternion.Euler(_actualPitch, 0.0f, 0.0f);
        t.RotateAround(FollowTransform.position, up, _actualYaw);
        t.localPosition = (up * 0.5f + t.forward * -_actualZoom)+t.localPosition;
    }
}
