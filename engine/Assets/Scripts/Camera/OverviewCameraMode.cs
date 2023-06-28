using Synthesis.UI.Dynamic;
using UnityEngine;

#nullable enable

public class OverviewCameraMode : ICameraMode
{
    public void Start<T>(CameraController cam, T? previousCam) where T : ICameraMode {
        // camera should be positioned above the field looking down
        cam.transform.position = new Vector3(0, 10, 0);
        cam.transform.rotation = Quaternion.Euler(90, 0, 0);
    }

    public void Update(CameraController cam)
    {
        // don't allow camera movement when a modal or panel is open
        if (DynamicUIManager.ActiveModal != null || DynamicUIManager.AnyPanels) return;
        // scrolling up zooms out in all other camera modes
        cam.transform.Translate(0, 0, CameraController.ZoomSensitivity * Input.mouseScrollDelta.y);
        Vector3 position = cam.transform.position;
        
        if (RobotSimObject.CurrentlyPossessedRobot != string.Empty) {
            var robot = RobotSimObject.GetCurrentlyPossessedRobot();
            var focus = robot.GroundedNode.transform.localToWorldMatrix.MultiplyPoint(robot.GroundedBounds.center);
            position.x = focus.x;
            position.z = focus.z;
        }

        cam.GroundRenderer.material.SetVector("FOCUS_POINT", position);

        // user can't go under the field or too far above that they can't see it
        cam.transform.position = new Vector3(position.x, Mathf.Clamp(position.y, 0, 100), position.z);
    }

    public void LateUpdate(CameraController cam)
    {
        
    }

    public void End(CameraController cam)
    {
    }
}