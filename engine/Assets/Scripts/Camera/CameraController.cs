using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    
    [SerializeField, Range(1f, 15.0f)] public float PitchSensitivity;
    [SerializeField, Range(1f, 15.0f)] public float YawSensitivity;
    [SerializeField] public float PitchLowerLimit;
    [SerializeField] public float PitchUpperLimit;
    [SerializeField] public float OrbitDistance;
    [SerializeField, Range(0.005f, 1.0f)] public float OrbitalAcceleration;
    [SerializeField] public Transform FollowTransform;
    
    private float _targetPitch = 10.0f;
    private float _targetYaw = 135.0f;
    private float _actualPitch = 0.0f;
    private float _actualYaw = 0.0f;
    
    public void Update() {
        if (FollowTransform != null && transform.parent != FollowTransform)
            transform.parent = FollowTransform;

        var pitchTest = PitchUpperLimit - PitchLowerLimit;
        if (pitchTest < 0)
            Debug.LogError("No range exists for pitch to reside in");
        else if (pitchTest == 0)
            Debug.Log("Pitch is locked");

        float p = 0.0f;
        float y = 0.0f;
        if (Input.GetKeyDown(KeyCode.Mouse0)) {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
        } else if (Input.GetKey(KeyCode.Mouse0)) {
            p = -PitchSensitivity * Input.GetAxis("Mouse Y");
            y = YawSensitivity * Input.GetAxis("Mouse X");
        } else if (Input.GetKeyUp(KeyCode.Mouse0)) {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
        }

        _targetPitch = Mathf.Clamp(_targetPitch + p, PitchLowerLimit, PitchUpperLimit);
        _targetYaw += y;
        
        float lerpFactor = Mathf.Clamp((OrbitalAcceleration * Time.deltaTime) / 0.018f, 0.01f, 1.0f);
        _actualPitch = Mathf.Lerp(_actualPitch, _targetPitch, lerpFactor);
        _actualYaw = Mathf.Lerp(_actualYaw, _targetYaw, lerpFactor);
    }

    public void LateUpdate() {
        // Construct orientation of the camera
        var t = transform;
        t.localPosition = Vector3.zero;
        t.localRotation = Quaternion.identity;

        var up = t.up;
        t.localRotation = Quaternion.Euler(_actualPitch, 0.0f, 0.0f);
        t.RotateAround(t.position, up, _actualYaw);
        t.localPosition = up * 0.5f + t.forward * -OrbitDistance;
    }
}
