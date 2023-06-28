using System;
using Synthesis.Gizmo;
using UnityEngine;
using Screen = UnityEngine.Device.Screen;

public class DriverStationCameraMode : ICameraMode {
    private Vector3 _origin = Vector3.zero;
    private Vector3 _target = Vector3.zero;
    private Vector3 _currentTarget = Vector3.zero;
    private Vector3 _offset = Vector3.zero;
    public const float EDGE_BUFFER = 400f; 
    private static readonly AnimationCurve CURVE = AnimationCurve.EaseInOut(0, 0, 1, 1);

    public void Configure<T>(CameraController cam, T previousCam, Action<CameraController, T> onEnd) where T : ICameraMode {
        GizmoManager.SpawnGizmo(
            Vector3.zero,
            t => {},
            t => {
                _origin = t.Position;
                onEnd.Invoke(cam, previousCam);
            });
    }
    
    public void Start<T>(CameraController cam, T previousCam) where T : ICameraMode {
        cam.gameObject.transform.position = _origin;
    }
    
    public void Update(CameraController cam) {
        RobotSimObject currentRobot = RobotSimObject.GetCurrentlyPossessedRobot();
        _target = currentRobot is null ? Vector3.zero : currentRobot.GroundedNode.transform.TransformPoint(currentRobot.GroundedBounds.center);
    }

    public void FixedUpdate(CameraController cam) {
        
    }
    
    public void LateUpdate(CameraController cam) {
        cam.transform.position = _origin;

        var relativePos = _target - cam.transform.position;
        if (relativePos.magnitude == 0) return;
        var targetRotation = Quaternion.LookRotation(relativePos);
        cam.transform.rotation = Quaternion.Lerp(cam.transform.rotation, targetRotation, Time.deltaTime * 2);
    }
    public void End(CameraController cam) {
        
    }
}