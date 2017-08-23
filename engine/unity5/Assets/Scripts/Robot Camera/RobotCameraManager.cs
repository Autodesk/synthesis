using UnityEngine;
using System.Collections.Generic;
using BulletUnity;
using BulletSharp;
using UnityEngine.UI;
using Assets.Scripts.FSM;

/// <summary>
/// This class is attached to RobotCameraList object and handles a list of cameras
/// Includes AddCamera to a specific robot, detach camera from a robot, toggle between cameras, and all robot camera configuration functions
/// </summary>
public class RobotCameraManager : MonoBehaviour
{
    public List<GameObject> robotCameraList = new List<GameObject>();
    public List<GameObject> tempCameraList = new List<GameObject>();

    private GameObject cameraIndicator;
    public GameObject CurrentCamera { get; set; }
    
    private GameObject robotCameraListObject;
    public GameObject SelectedNode;
    public bool SelectingNode { get; set; }
    
    private static float positionSpeed = 0.5f;
    private static float rotationSpeed = 25;

    private List<Color> hoveredColors = new List<Color>();
    private List<Color> selectedColors = new List<Color>();
    private Color selectedColor = new Color(1, 0, 0);
    private Color hoverColor = new Color(1, 1, 0, 0.1f);
    private GameObject lastNode;

    public bool ChangingCameraPosition { get; set; }
    public bool IsChangingHeight { get; set; }
    public bool IsShowingAngle { get; set; }
    public bool IsChangingFOV { get; set; }

    private void Start()
    {
        robotCameraListObject = GameObject.Find("RobotCameraList");
        cameraIndicator = AuxFunctions.FindObject(robotCameraListObject, "CameraIndicator");
    }

    private void Update()
    {
        //SelectingNode is enabled, users can pick the node for camera attachment
        if (SelectingNode)
        {
            SetNode();
        }
        //Update configurations
        UpdateCameraPosition();
    }

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
        //Set the camera parent robot, which is necessary when changing robot and restoring the configuration
        configuration.SetParentRobot(robot);

        newCamera.SetActive(false);
        if (robotCameraList.Count == 0)
            CurrentCamera = newCamera;

        robotCameraList.Add(newCamera);

        return newCamera;
    }

    /// <summary>
    /// Add a new camera to the robot using the default position (0, 0.5, 0) and rotation
    /// </summary>
    /// <returns></returns>
    public GameObject AddCamera(Robot robot, Transform anchor)
    {
        GameObject newCamera = new GameObject("RobotCamera_" + robotCameraList.Count);
        newCamera.AddComponent<Camera>();

        newCamera.transform.parent = anchor;
        newCamera.transform.localPosition = new Vector3(0f, 0f, 0f);
        newCamera.transform.localRotation = Quaternion.identity;

        RobotCamera configuration = newCamera.AddComponent<RobotCamera>();
        configuration.UpdateConfiguration();
        //Set the camera parent robot, which is necessary when changing robot and restoring the configuration
        configuration.SetParentRobot(robot);

        newCamera.SetActive(false);
        //Make sure current camera is the first one on the list
        if (robotCameraList.Count == 0)
            CurrentCamera = newCamera;

        robotCameraList.Add(newCamera);

        return newCamera;
    }

    /// <summary>
    /// Remove all existing robot camera and destroy the objects
    /// Use DetachCameras instead if the removal is just temporary
    /// </summary>
    public void RemoveAllCameras()
    {
        CurrentCamera = null;
        //Move out camera indicator in case it get destroyed with a removed robot in multiplayer
        cameraIndicator.transform.parent = robotCameraListObject.transform;
        foreach(GameObject robotCamera in robotCameraList)
        {
            robotCamera.transform.parent = null;
            Destroy(robotCamera);
        }
        robotCameraList.Clear();
    }

    /// <summary>
    /// Remove all cameras from a given robot, used when a robot is removed. Use DetachCamerasFromRobot when changing a robot!
    /// </summary>
    /// <param name="parent"></param> The robot whose cameras you want to remove
    public void RemoveCamerasFromRobot(Robot parent)
    {
        List<GameObject> removingCameras = GetRobotCamerasFromRobot(parent);
        //Take out the camera indicator in case it gets destroyed with one of the robots
        cameraIndicator.transform.parent = robotCameraListObject.transform;
        foreach (GameObject camera in removingCameras)
        {
            //Remove those useless cameras from the list and destroy them
            if (robotCameraList.Contains(camera))
            {
                robotCameraList.Remove(camera);
                
                Destroy(camera);
            }
        }
        //Reset the current camera to the first one on the list in case the current one gets destroyed already
        CurrentCamera = robotCameraList[0];
    }

    /// <summary>
    /// Detach the robot camera from a given robot in preparation for changing robot or other operation that needs to take out a specific group of robot camera
    /// </summary>
    /// <param name="parent"></param> A robot where cameras are going to be detached from
    public void DetachCamerasFromRobot(Robot parent)
    {
        List<GameObject> detachingCameras = GetRobotCamerasFromRobot(parent);

        foreach(GameObject camera in detachingCameras)
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

    /// <summary>
    /// Returns the robot camera list
    /// </summary>
    /// <returns></returns>
    public List<GameObject> GetRobotCameraList()
    {
        return robotCameraList;
    }
    
    /// <summary>
    /// Initialize robot node selection
    /// </summary>
    public void DefineNode()
    {
        UserMessageManager.Dispatch("Click on a robot node to set it as the attachment node", 5);
        SelectingNode = true;
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

        //Debug.Log("Selected:" + rayResult.CollisionObject);
        //If there is a collision object and it is a robot part, set that to be new attachment point
        if (rayResult.CollisionObject != null)
        {
            GameObject selectedObject = ((BRigidBody)rayResult.CollisionObject.UserObject).gameObject;
            if (selectedObject.transform.parent != null && selectedObject.transform.parent.name == "Robot")
            {
                //Change highlight target when the mouse point to a different object
                if(lastNode != null && !selectedObject.Equals(lastNode))
                {
                    RevertNodeColors(lastNode, hoveredColors);
                    lastNode = null;
                }
                //Highlight the node which mouse is pointing to to yellow
                else
                {
                    ChangeNodeColors(selectedObject, hoverColor, hoveredColors);
                    lastNode = selectedObject;
                }
                //Change the color to selected color when user click and choose the node
                if (Input.GetMouseButtonDown(0))
                {
                    string name = selectedObject.name;

                    //Revert the current selection back to its original so selectedColors can store the new selection properly
                    RevertNodeColors(lastNode, hoveredColors);

                    RevertNodeColors(SelectedNode, selectedColors);

                    SelectedNode = selectedObject;
                    ChangeNodeColors(SelectedNode, selectedColor, selectedColors);
                    UserMessageManager.Dispatch(name + " has been selected as the node for camera attachment", 5);
                }
                
            }
            else
            {
                //When mouse is not pointing to any robot node, set the last hovered node back to its original color
                if(lastNode != null)
                {
                    RevertNodeColors(lastNode, hoveredColors);
                    lastNode = null;
                }
                //When users try to select a non-robotNode object
                if (Input.GetMouseButtonDown(0))
                {
                    UserMessageManager.Dispatch("Please select a robot node!", 3);
                }
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
        ResetNodeColors();
        SelectedNode = null;
    }

    /// <summary>
    /// Reset the selected node color to its original, called after confirming node selection or in EndProcesses
    /// </summary>
    public void ResetNodeColors()
    {
        RevertNodeColors(SelectedNode, selectedColors);
    }

    /// <summary>
    /// Use WASD tp change the position, rotation, fov of camera
    /// </summary>
    private void UpdateCameraPosition()
    {
        if (ChangingCameraPosition)
        {
            if (IsChangingFOV) //Control fov
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
            //Update configuration info of the current camera
            CurrentCamera.GetComponent<RobotCamera>().UpdateConfiguration();
        }
    }

    /// <summary>
    /// Reset the configuration states to false so nothing can be changed in UpdateCameraPosition
    /// </summary>
    public void ResetConfigurationState()
    {
        ChangingCameraPosition = IsChangingFOV = IsShowingAngle = IsChangingHeight = false;
    }

    #region Highlighting Functions
    private void ChangeNodeColors(GameObject node, Color color, List<Color> storedColors)
    {
        foreach (Renderer renderers in node.GetComponentsInChildren<Renderer>())
        {
            foreach (Material m in renderers.materials)
            {
                storedColors.Add(m.color);
                m.color = color;
            }
        }
    }

    private void RevertNodeColors(GameObject node, List<Color> storedColors)
    {
        if (node != null && storedColors.Count != 0)
        {
            
            int counter = 0;
            foreach (Renderer renderers in node.GetComponentsInChildren<Renderer>())
            {
                foreach (Material m in renderers.materials)
                {
                    if (counter <= storedColors.Count - 1)
                    {
                        m.color = storedColors[counter];
                        counter++;
                    }
                }
            }
            storedColors.Clear();
        }
    }
    #endregion

}