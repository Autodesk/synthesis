using UnityEngine;

public class OverviewCameraMode : ICameraMode
{
    public void Start(CameraController cam)
    {
        // camera should be positioned above the field looking down
        cam.transform.position = new Vector3(0, 10, 0);
        cam.transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    public void Update(CameraController cam)
    {
        // scrolling up zooms out in all other camera modes
        cam.transform.Translate(0, 0, cam.ZoomSensitivity * Input.mouseScrollDelta.y);
        Vector3 position = cam.transform.position;
        // user can't go under the field or too far above that they can't see it
        cam.transform.position = new Vector3(position.x, Mathf.Clamp(position.y, 0, 100), position.z);
    }

    public void LateUpdate(CameraController cam)
    {
        
    }
}