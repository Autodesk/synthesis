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
    private GameObject selectedNode;
    private bool selectingNode = false;
    public bool DefiningCameraNode = false;
    private Vector3 currentPosition = new Vector3(0, 0, 0);
    private Vector3 currentRotation = new Vector3(0, 0, 0);
    private static float rotateOffsetAmount = 0.5f;
    private static float positionOffsetAmount = 0.5f;
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

        if (DefiningCameraNode)
        {
            if (Input.GetMouseButtonDown(0))
            {
                if (selectingNode) SetNode();
            }
        }
    }

    public void DefinePosition()
    {
        currentPosition = CurrentCamera.transform.localPosition;
        currentRotation = CurrentCamera.transform.localRotation.ToEuler();
    }

    public void DefineNode()
    {
        UserMessageManager.Dispatch("Click on a robot node to set it as the attachment node", 5);
        selectingNode = true;
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

        //If there is a collision object and it is dynamic and not a robot part, change the gamepiece to that
        if (rayResult.CollisionObject != null)
        {
            GameObject selectedObject = ((BRigidBody)rayResult.CollisionObject.UserObject).gameObject;
            if (selectedObject.transform.parent != null && selectedObject.transform.parent.name == "Robot")
            {
                string name = selectedObject.name;

                selectedNode = selectedObject;

                UserMessageManager.Dispatch(name + " has been selected as the node for attachment", 2);
                selectingNode = false;
            }
            else
            {
                UserMessageManager.Dispatch("Please select a robot node", 2);
            }
        }
    }

    public void RotateLeft()
    {
        Vector3 temp = CurrentCamera.transform.rotation.ToEuler();
        temp.y += rotateOffsetAmount;
        CurrentCamera.transform.localRotation = Quaternion.Euler(temp);
    }
    public void RotateRight()
    {
        Vector3 temp = CurrentCamera.transform.rotation.ToEuler();
        temp.y -= rotateOffsetAmount;
        CurrentCamera.transform.localRotation = Quaternion.Euler(temp);
    }
    public void RotateUp()
    {
        Vector3 temp = CurrentCamera.transform.rotation.ToEuler();
        temp.x += rotateOffsetAmount;
        CurrentCamera.transform.localRotation = Quaternion.Euler(temp);
    }
    public void RotateDown()
    {
        Vector3 temp = CurrentCamera.transform.rotation.ToEuler();
        temp.x -= rotateOffsetAmount;
        CurrentCamera.transform.localRotation = Quaternion.Euler(temp);
    }

    public void MoveUp()
    {
        Vector3 temp = CurrentCamera.transform.position;
        temp.y -= positionOffsetAmount;
        CurrentCamera.transform.localPosition = temp;
    }
    public void MoveDown()
    {
        Vector3 temp = CurrentCamera.transform.position;
        temp.y += positionOffsetAmount;
        CurrentCamera.transform.localPosition = temp;
    }
    public void MoveLeft()
    {
        Vector3 temp = CurrentCamera.transform.position;
        temp.x -= positionOffsetAmount;
        CurrentCamera.transform.localPosition = temp;
    }
    public void MoveRight()
    {
        Vector3 temp = CurrentCamera.transform.position;
        temp.y += positionOffsetAmount;
        CurrentCamera.transform.localPosition = temp;
    }

    public void MoveBack()
    {
        Vector3 temp = CurrentCamera.transform.position;
        temp.z += positionOffsetAmount;
        CurrentCamera.transform.localPosition = temp;
    }

    public void MoveForward()
    {
        Vector3 temp = CurrentCamera.transform.position;
        temp.z -= positionOffsetAmount;
        CurrentCamera.transform.localPosition = temp;
    }
}