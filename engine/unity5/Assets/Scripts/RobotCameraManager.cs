using UnityEngine;
using System.Collections.Generic;
using BulletUnity;
using BulletSharp;
using UnityEngine.UI;
using Assets.Scripts.FSM;


public class RobotCameraManager : MonoBehaviour
{
    public List<GameObject> robotCameraList = new List<GameObject>();
    public List<GameObject> tempCameraList = new List<GameObject>();

    public GameObject CurrentCamera { get; set; }
    
    private GameObject robotCameraListObject;
    public GameObject SelectedNode;
    public bool SelectingNode { get; set; }


    private static float positionSpeed = 0.5f;
    private static float rotationSpeed = 25;
    
    public bool ChangingCameraPosition { get; set; }
    public bool IsChangingHeight { get; set; }
    public bool IsShowingAngle { get; set; }
    public bool IsChangingFOV { get; set; }

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
    }

    /// <summary>
    /// Add a new camera to the robot
    /// </summary>
    /// <param name="anchor"></param> The robot node to which the camera attaches
    /// <param name="positionOffset"></param> 
    /// <param name="rotationOffset"></param>
    /// <returns></returns>
    public GameObject AddCamera(Robot robot, Transform anchor, Vector3 positionOffset, Vector3 rotationOffset)
    {
        GameObject newCamera = new GameObject("RobotCamera_" + robotCameraList.Count);
        newCamera.AddComponent<Camera>();

        newCamera.transform.parent = anchor;
        newCamera.transform.localPosition = positionOffset;
        newCamera.transform.localRotation = Quaternion.Euler(rotationOffset);

        RobotCamera configuration = newCamera.AddComponent<RobotCamera>();
        configuration.UpdateConfiguration();
        configuration.SetParentRobot(robot);

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
    public GameObject AddCamera(Robot robot, Transform anchor)
    {
        GameObject newCamera = new GameObject("RobotCamera_" + robotCameraList.Count);
        newCamera.AddComponent<Camera>();

        newCamera.transform.parent = anchor;
        newCamera.transform.localPosition = new Vector3(0f, 0.5f, 0f);
        newCamera.transform.localRotation = Quaternion.identity;

        RobotCamera configuration = newCamera.AddComponent<RobotCamera>();
        configuration.UpdateConfiguration();
        configuration.SetParentRobot(robot);

        newCamera.SetActive(false);
        if (robotCameraList.Count == 0)
            CurrentCamera = newCamera;

        robotCameraList.Add(newCamera);
        if (robotCameraList.Count == 0)
            CurrentCamera = newCamera;
        return newCamera;
    }

    /// <summary>
    /// Remove all existing robot camera and destroy the objects
    /// Use DetachCameras instead if the removal is just temporary
    /// </summary>
    public void RemoveAllCameras()
    {
        CurrentCamera = null;
        foreach(GameObject robotCamera in robotCameraList)
        {
            robotCamera.transform.parent = null;
            Destroy(robotCamera);
        }
        robotCameraList.Clear();
    }

    public void RemoveCameraFromRobot(Robot parent)
    {
        List<GameObject> removingCamera = GetRobotCamerasFromRobot(parent);
        foreach(GameObject camera in removingCamera)
        {
            if (robotCameraList.Contains(camera))
            {
                robotCameraList.Remove(camera);
                Destroy(camera);
            }
        }
        CurrentCamera = robotCameraList[0];
    }
    /// <summary>
    /// Detach the robot camera from a given robot in preparation for changing robot or other operation that needs to take out a specific group of robot camera
    /// </summary>
    /// <param name="parent"></param> A robot where cameras are going to be detached from
    public void DetachCameras(Robot parent)
    {
        List<GameObject> detachingCamera = GetRobotCamerasFromRobot(parent);

        foreach(GameObject camera in detachingCamera)
        {
            camera.GetComponent<RobotCamera>().DetachCamera();
        }
    }

    /// <summary>
    /// Return a list of robot cameras attached to a given robot
    /// </summary>
    /// <param name="parent"></param> A robot on which cameras are attached to
    /// <returns></returns> A list of camera attach to that robot
    public List<GameObject> GetRobotCamerasFromRobot(Robot parent)
    {

        List<GameObject> camerasOnRobot = new List<GameObject>();
        foreach (GameObject camera in robotCameraList)
        {
            RobotCamera config = camera.GetComponent<RobotCamera>();
            if (config.robot.Equals(parent))
            {
                camerasOnRobot.Add(camera);
            }
        }
        return camerasOnRobot;
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

    private void Update()
    {
        //Enable selecting node state, and users can left click on a node to choose it
        if (SelectingNode)
        {
            if (Input.GetMouseButtonDown(0))
            {
                SetNode();
                Debug.Log("Selecting node");

            }
        }
        UpdateCameraPosition();
    }


    /// <summary>
    /// Initialize robot node selection
    /// </summary>
    public void DefineNode()
    {
        UserMessageManager.Dispatch("Click on a robot node to set it as the attachment node", 5);
        SelectingNode = true;
        //SelectedNode = null;
    }

    /// <summary>
    /// Set the node where the camera will be attached to
    /// </summary>
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
        //If there is a collision object and it is a robot part, set that to be new attachment point
        if (rayResult.CollisionObject != null)
        {
            GameObject selectedObject = ((BRigidBody)rayResult.CollisionObject.UserObject).gameObject;
            if (selectedObject.transform.parent != null && selectedObject.transform.parent.name == "Robot")
            {
                string name = selectedObject.name;

                SelectedNode = selectedObject;

                UserMessageManager.Dispatch(name + " has been selected as the node for camera attachment", 5);
            }
            else
            {
                UserMessageManager.Dispatch("Please select a robot node", 3);
            }
        }
    }

    /// <summary>
    /// Update the attachment point to be the selected node and toggle the state back
    /// </summary>
    public void ChangeNodeAttachment()
    {
        CurrentCamera.transform.parent = SelectedNode.transform;
        SelectingNode = false;
        SelectedNode = null;
    }

    /// <summary>
    /// Use WASD change the position, rotation of camera
    /// </summary>
    private void UpdateCameraPosition()
    {
        if (ChangingCameraPosition)
        {
            if (IsChangingFOV)
            {
                CurrentCamera.GetComponent<Camera>().fieldOfView += Input.GetAxis("CameraVertical");
            }
            else if (IsShowingAngle) //Control rotation (only when the angle panel is active)
            {
                CurrentCamera.transform.Rotate(new Vector3(-Input.GetAxis("CameraVertical") * rotationSpeed, Input.GetAxis("CameraHorizontal") * rotationSpeed, 0) * Time.deltaTime);
            }
            else if (!IsChangingHeight) //Control horizontal plane transform
            {
                CurrentCamera.transform.Translate(new Vector3(Input.GetAxis("CameraHorizontal") * positionSpeed, 0, Input.GetAxis("CameraVertical") * positionSpeed) * Time.deltaTime);
            }
            else //Control height transform
            {
                CurrentCamera.transform.Translate(new Vector3(0, Input.GetAxis("CameraVertical") * positionSpeed, 0) * Time.deltaTime);
            }
            CurrentCamera.GetComponent<RobotCamera>().UpdateConfiguration();
        }
    }
}