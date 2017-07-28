using UnityEngine;
using System.Collections.Generic;
using BulletUnity;
using BulletSharp;

public class RobotCamera : MonoBehaviour
{
    public List<GameObject> robotCameraList = new List<GameObject>();
    public GameObject CurrentCamera { get; set; }
    public GameObject CameraIndicator;
    private GameObject robotCameraListObject;
    public GameObject SelectedNode;

    public bool SelectingNode = false;
    public bool DefiningCameraNode = false;

    public bool ChangingCameraPosition = false;
    public bool IsChangingHeight = false;

    //x rotates up and down (+up), y rotates left and right (+left), z tilts to left and right (+left)

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
        if (robotCameraList.Count == 0)
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
        if (CameraIndicator == null)
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

        if (SelectingNode)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SetNode();
                Debug.Log("Selecting node");

            }
        }

        if (ChangingCameraPosition)
        {
            
            if (Input.GetMouseButton(1))
            {
                CurrentCamera.transform.Rotate(new Vector3(-Input.GetAxis("CameraVertical") * 10, Input.GetAxis("CameraHorizontal") * 10, 0) * Time.deltaTime);
            }
            else if(!IsChangingHeight)
            {
                CurrentCamera.transform.Translate(new Vector3(Input.GetAxis("CameraHorizontal"), 0, Input.GetAxis("CameraVertical")) * Time.deltaTime);
            }
            else
            {
                CurrentCamera.transform.Translate(new Vector3(0, Input.GetAxis("CameraVertical"), 0) * Time.deltaTime);
            }
        }
    }
    
    public void DefineNode()
    {
        UserMessageManager.Dispatch("Click on a robot node to set it as the attachment node", 5);
        SelectingNode = true;
        //SelectedNode = null;
    }

    public void SetNode()
    {
        //Casts a ray from the camera in the direction the mouse is in and returns the closest object hit
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        BulletSharp.Math.Vector3 start = ray.origin.ToBullet();
        BulletSharp.Math.Vector3 end = ray.GetPoint(200).ToBullet();

        //Creates a callback result that will be updated if we do a ray test with it
        ClosestRayResultCallback rayResult = new ClosestRayResultCallback(ref start, ref end);

        //Retrieves the bullet physics world and does a ray test with the given coordinates and updates the callback object
        BPhysicsWorld world = BPhysicsWorld.Get();
        world.world.RayTest(start, end, rayResult);

        Debug.Log("Selected:" + rayResult.CollisionObject);
        //If there is a collision object and it is dynamic and not a robot part, change the gamepiece to that
        if (rayResult.CollisionObject != null)
        {
            GameObject selectedObject = ((BRigidBody)rayResult.CollisionObject.UserObject).gameObject;
            if (selectedObject.transform.parent != null && selectedObject.transform.parent.name == "Robot")
            {
                string name = selectedObject.name;

                SelectedNode = selectedObject;

                UserMessageManager.Dispatch(name + " has been selected as the node for camera attachment", 2);
            }
            else
            {
                UserMessageManager.Dispatch("Please select a robot node", 2);
            }
        }
    }

    public void ChangeNodeAttachment()
    {
        CurrentCamera.transform.parent = SelectedNode.transform;
    }

}