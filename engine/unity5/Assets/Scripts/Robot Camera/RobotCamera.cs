using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class is used to store robot camera configuration and restore info when changing robot
/// </summary>
public class RobotCamera : MonoBehaviour {

    Vector3 localPosition;
    Quaternion localRotation;
    Transform parent;
    int parentNodeIndex;
    float FOV;
    public Robot robot;

    /// <summary>
    /// Update the configuration info of robot camera, called in RobotCameraManager after each configuration update
    /// </summary>
    public void UpdateConfiguration()
    {
        localPosition = transform.localPosition;
        localRotation = transform.localRotation;
        FOV = GetComponent<Camera>().fieldOfView;
        parent = transform.parent;
        parentNodeIndex = transform.parent.GetSiblingIndex();
    }

    /// <summary>
    /// Set the parent robot of the robot cameras
    /// </summary>
    /// <param name="robot"></param>
    public void SetParentRobot(Robot robot)
    {
        this.robot = robot;
    }

    /// <summary>
    /// Set the configuration of camera and update the fields
    /// </summary>
    /// <param name="parent"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    /// <param name="fov"></param>
    public void SetConfiguration(Transform parent, Vector3 pos, Quaternion rot, float fov = 60)
    {
        transform.parent = parent;
        transform.localPosition = pos;
        transform.localRotation = rot;
        GetComponent<Camera>().fieldOfView = fov;
        UpdateConfiguration();
    }

    /// <summary>
    /// Recover the camera configuration on the new robot, if there's not as many nodes the camera will be put to node 0
    /// </summary>
    public void RecoverConfiguration()
    {
        //If the new robot does not have a matching parent node
        if(parentNodeIndex > robot.gameObject.transform.childCount - 1)
        {
            UserMessageManager.Dispatch(gameObject.name + " attachment reset to node 0 for an absence of corresponding node.", 5f);
            transform.parent = robot.gameObject.transform.GetChild(0);
        }
        else
        {
            //Set transform using the parent node index because the original parent transform will destroy the camera with the destroyed node
            transform.parent = robot.gameObject.transform.GetChild(parentNodeIndex);
        }
        //Update parent transform to the current parent
        parent = transform.parent;
        transform.localPosition = localPosition;
        transform.localRotation = localRotation;
        GetComponent<Camera>().fieldOfView = FOV;
    }

    /// <summary>
    /// Detach the robot camera from the current parent node and put it with robot camera list object temporarily
    /// </summary>
    public void DetachCamera()
    {
        transform.parent = GameObject.Find("RobotCameraList").transform;
    }

    /// <summary>
    /// Attach the robot camera to a specific parent (robot node)
    /// </summary>
    /// <param name="parent"></param>
    public void AttachCamera(Transform parent)
    {
        transform.parent = parent;
        this.parent = parent;
    }
}
