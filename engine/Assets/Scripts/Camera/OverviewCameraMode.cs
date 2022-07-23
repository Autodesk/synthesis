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
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            cam.transform.Translate(0, cam.ZoomSensitivity * -Input.mouseScrollDelta.y, 0);
        }
    }

    public void LateUpdate(CameraController cam)
    {
        
    }
}