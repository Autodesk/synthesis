using UnityEngine;
using System.Collections.Generic;

public class RobotCamera : MonoBehaviour
{
    public List<GameObject> robotCameraList = new List<GameObject>();
    public GameObject CurrentCamera { get; set; }
    public GameObject CameraIndicator;
    private GameObject robotCameraListObject;

    /// <summary>
    /// Switching between different cameras on robot given the specific camera
    /// </summary>
    /// <param name="robotCamera"></param> the specific camera on the robot camera list
    public void SwitchCamera(GameObject robotCamera)
    {
        CurrentCamera.SetActive(false);
        CurrentCamera = robotCamera;
        CurrentCamera.SetActive(true);
    }

    /// <summary>
    /// Switch to the next camera on the list
    /// </summary>
    /// <param name="currentCamera"></param>
    public void ToggleCamera()
    {
        SwitchCamera(robotCameraList[(robotCameraList.IndexOf(CurrentCamera) + 1) % robotCameraList.Count]);
        //CameraIndicator.SetActive(CurrentCamera.activeSelf);
    }

    /// <summary>
    /// Add a new camera to the robot
    /// </summary>
    /// <param name="anchor"></param> The robot node to which the camera attaches
    /// <param name="positionOffset"></param> 
    /// <param name="rotationOffset"></param>
    /// <returns></returns>
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

    /// <summary>
    /// Add a new camera to the robot using the default position and rotation
    /// </summary>
    /// <returns></returns>
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

    public void RemoveCameras()
    {
        foreach (GameObject robotCamera in robotCameraList)
        {
            Destroy(robotCamera);
        }
        CurrentCamera = null;
        robotCameraList.Clear();
    }

    /// <summary>
    /// Return true if the current camera is the last on the list
    /// </summary>
    /// <returns></returns> true if the current camera is the last on the list
    public bool IsLastCamera()
    {
        return robotCameraList.IndexOf(CurrentCamera) == robotCameraList.Count - 1;
    }
    
    public List<GameObject> GetRobotCameraList()
    {
        return robotCameraList;
    }

    public void Start()
    {
        robotCameraListObject = GameObject.Find("RobotCameraList");
        if(CameraIndicator == null)
        {
            CameraIndicator = AuxFunctions.FindObject(robotCameraListObject, "CameraIndicator");
        }
    }
    public void Update()
    {
        if (CameraIndicator.activeSelf)
        {
            CameraIndicator.transform.position = CurrentCamera.transform.position;
            CameraIndicator.transform.rotation = CurrentCamera.transform.rotation;

            CameraIndicator.transform.parent = CurrentCamera.transform;
        }
    }
}