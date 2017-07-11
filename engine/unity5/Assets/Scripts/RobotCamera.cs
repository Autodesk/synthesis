using UnityEngine;
using System.Collections.Generic;

public class RobotCamera : MonoBehaviour
{
    private List<GameObject> robotCameraList = new List<GameObject>();
    public GameObject CurrentCamera { get; set; }
    

    public void SwitchCamera(GameObject robotCamera)
    {
        CurrentCamera.SetActive(false);
        CurrentCamera = robotCamera;
        CurrentCamera.SetActive(true);
    }

    public GameObject AddCamera(Transform anchor, Vector3 positionOffset, Vector3 rotationOffset)
    {
        GameObject newCamera = new GameObject("RobotCamera_" + robotCameraList.Count);
        newCamera.AddComponent<Camera>();
        newCamera.transform.parent = anchor;
        newCamera.transform.localPosition = positionOffset;
        newCamera.transform.localRotation = Quaternion.Euler(rotationOffset);

        newCamera.SetActive(false);
        if(robotCameraList.Count == 0)
            CurrentCamera = newCamera;

        robotCameraList.Add(newCamera);

        return newCamera;
    }

    public GameObject AddCamera()
    {
        GameObject newCamera = new GameObject("RobotCamera_" + robotCameraList.Count);
        newCamera.AddComponent<Camera>();
        newCamera.transform.localPosition = new Vector3(0f, 0f, 0f);
        newCamera.transform.localRotation = Quaternion.identity;
        newCamera.SetActive(false);
        if (robotCameraList.Count == 0)
            CurrentCamera = newCamera;

        robotCameraList.Add(newCamera);
        CurrentCamera = newCamera;
        return newCamera;
    }


    void LateUpdate()
    {
        if (Input.GetKeyDown(Controls.ControlKey[(int)Controls.Control.CameraToggle]))
        {
            SwitchCamera(robotCameraList[(robotCameraList.IndexOf(CurrentCamera) + 1) % robotCameraList.Count]);
        }
    }
    
}