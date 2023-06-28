using System;
using UnityEditor;
using UnityEngine;

#nullable enable

public interface ICameraMode
{
    public void Configure<T>(CameraController cam, T? previousCam, Action<CameraController, T?>? onEnd = null) where T : ICameraMode {
        onEnd?.Invoke(cam, previousCam);
    }
    public void Start<T>(CameraController cam, T? previousCam) where T : ICameraMode;
    public void Update(CameraController cam);
    public void FixedUpdate(CameraController cam);
    public void LateUpdate(CameraController cam);
    public void End(CameraController cam);
}