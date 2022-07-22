using UnityEngine;

public interface ICameraMode
{
    public void Start(CameraController cam);
    public void Update(CameraController cam);
    public void LateUpdate(CameraController cam);
}