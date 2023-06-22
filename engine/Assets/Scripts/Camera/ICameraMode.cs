using UnityEngine;

#nullable enable

// clang-format off
public interface ICameraMode {
    public void Start<T>(CameraController cam, T? previousCam) where T : ICameraMode; 
    public void Update(CameraController cam); 
    public void LateUpdate(CameraController cam); 
    public void End(CameraController cam); 
}
    // clang-format on
