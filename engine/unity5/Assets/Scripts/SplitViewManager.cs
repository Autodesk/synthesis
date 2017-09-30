using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.FSM;
using System.Linq;
using UnityEngine.Analytics;

/// <summary>
/// Class for controlling the various aspects of local multiplayer
/// </summary>
public class SplitViewManager : MonoBehaviour {

    private GameObject canvas;
    private GameObject mainCamera;
    private MainState mainState;
    private SimUI simUI;
    /// <summary>
    /// A list of driver station cameras
    /// </summary>
    public List<GameObject> playerCameras = new List<GameObject>();
    /// <summary>
    /// A list of active driver station cameras depending on which ones are assigned to a 
    /// specific robot
    /// </summary>
    private List<PlayerCamera> activeCameras = new List<PlayerCamera>();

    /// <summary>
    /// The number of active robot cameras
    /// </summary>
    private int activeCount;
    private GameObject splitViewManager;
    /// <summary>
    /// A boolean indicating if split view is active, mainly used by simUI to determine the correct 
    /// action for ESC key
    /// </summary>
    public bool SplitViewActive;

    private void Awake()
    {
        //Link this script to MainState
        StateMachine.Instance.Link<MainState>(this);
    }

    private void Start()
    {
        //Find the individual cameras and store them in playerCameras list
        splitViewManager = GameObject.Find("SplitViewManager");
        mainCamera = GameObject.Find("Main Camera");
        playerCameras.Add(AuxFunctions.FindObject(splitViewManager, "Red1Cam"));
        playerCameras.Add(AuxFunctions.FindObject(splitViewManager, "Red2Cam"));
        playerCameras.Add(AuxFunctions.FindObject(splitViewManager, "Red3Cam"));
        playerCameras.Add(AuxFunctions.FindObject(splitViewManager, "Blue1Cam"));
        playerCameras.Add(AuxFunctions.FindObject(splitViewManager, "Blue2Cam"));
        playerCameras.Add(AuxFunctions.FindObject(splitViewManager, "Blue3Cam"));
        
    }

    private void Update()
    {
        if (mainState == null) mainState = StateMachine.Instance.FindState<MainState>();
        //Toggle the state of all driver station cameras between driver station view and orbit view
        //Using space
        if (Input.GetKeyDown(KeyCode.Space)) SwitchSplitView();
    }

    /// <summary>
    /// Assign a driver station camera to a robot
    /// </summary>
    /// <param name="index"></param> the driver station index of the robot/the index of the camera
    /// <param name="target"></param> the robot that will be the target of the camera
    public void AssignRobotCamera(int index, GameObject target)
    {
        //Set the given camera to active
        PlayerCamera playerCam = playerCameras[index].GetComponent<PlayerCamera>();
        playerCam.SetActive(true);
        //Add the camera to the active list, increment the number of active robot
        activeCameras.Add(playerCam);
        activeCount++;
        //Set the target of the player camera to the given robot
        playerCam.cameraState.SetTargetRobot(target);
    }

    public void InitiateSplitView()
    {
        List<int> indexes = new List<int>();
        //Collect a list of driver station indexes from the spawned robots
        foreach (Robot robot in mainState.SpawnedRobots)
        {
            indexes.Add(robot.DriverStationIndex);
        }
        //Make sure the indexes are all unique. i.e. no two driver station targeting the same robot
        if (indexes.Distinct().Count() == indexes.Count)
        {
            //Assign cameras to spawned robots
            foreach (Robot robot in mainState.SpawnedRobots)
            {
                AssignRobotCamera(robot.DriverStationIndex, robot.gameObject);
            }

            //Start the split view if there are more than one robots on the field
            if (activeCount > 1) {
                SplitViewActive = true;
                GenerateSplitView();
            }
            //Send a warning message and reset targets of the cameras
            else
            {
                UserMessageManager.Dispatch("Please have two or more robots to use split view!", 5f);
                ClearCameraTarget();
            }
        }
        //Set a warning message if the indexes are not unique
        else
        {
            UserMessageManager.Dispatch("You can't control two or more robots from the same driver station!", 5f);
        }
    }

    /// <summary>
    /// Generates the split view depending on how many cameras are used
    /// </summary>
    public void GenerateSplitView()
    {
        //Deactivate the main camera
        mainCamera.SetActive(false);
        //Arrange the camera views depending on the number of active cameras
        switch (activeCount)
        {
            case 2:
                SetTwoCamView();
                break;
            case 3:
                SetThreeCamView();
                break;
            case 4:
                SetFourCamView();
                break;
            case 5:
                SetFiveCamView();
                break;
            case 6:
                SetSixCamView();
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Exit split view
    /// </summary>
    public void EndSplitView()
    {
        SplitViewActive = false;
        mainCamera.SetActive(true);
        DynamicCamera.MovingEnabled = true;
        ClearCameraTarget();
    }

    //Toggle the camera views of all camera
    public void SwitchSplitView()
    {
        foreach(GameObject cam in playerCameras)
        {
            PlayerCamera playerCam = cam.GetComponent<PlayerCamera>();
            playerCam.ToggleCameraState(playerCam.cameraState);
        }
    }

    /// <summary>
    /// Clear the targets of the cameras and reset active count and the activeCameras list
    /// </summary>
    public void ClearCameraTarget()
    {
        foreach (GameObject cam in playerCameras)
        {
            if (cam.GetComponent<PlayerCamera>().cameraState != null)
            {
                cam.GetComponent<PlayerCamera>().cameraState.SetTargetRobot(null);
            }
            cam.SetActive(false);
        }
        activeCount = 0;
        activeCameras.Clear();
    }

    /// <summary>
    /// Generate a split view with 2 cameras with robot 1 on the left and robot 2 on the right
    /// </summary>
    public void SetTwoCamView()
    {
        activeCameras[0].gameObject.GetComponent<Camera>().rect = new Rect(0, 0, 0.5f, 1);
        activeCameras[1].gameObject.GetComponent<Camera>().rect = new Rect(0.5f, 0, 0.5f, 1);
    }

    /// <summary>
    /// Generate a split view with 3 cameras with robot 1 at the top, 2 at lower left, and 3 at lower right
    /// </summary>
    public void SetThreeCamView()
    {
        activeCameras[0].gameObject.GetComponent<Camera>().rect = new Rect(0f, 0.5f, 1f, 0.5f);
        activeCameras[1].gameObject.GetComponent<Camera>().rect = new Rect(0f, 0.0f, 0.5f, 0.5f);
        activeCameras[2].gameObject.GetComponent<Camera>().rect = new Rect(0.5f, 0.0f, 0.5f, 0.5f);
    }

    /// <summary>
    /// Generate a split view with 4 cameras with robot 1 at upper left, 2 at upper right, 
    /// 3 at lower left, and 4 at lower right
    /// </summary>
    public void SetFourCamView()
    {
        activeCameras[0].gameObject.GetComponent<Camera>().rect = new Rect(0f, 0.5f, 0.5f, 0.5f);
        activeCameras[1].gameObject.GetComponent<Camera>().rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
        activeCameras[2].gameObject.GetComponent<Camera>().rect = new Rect(0f, 0.0f, 0.5f, 0.5f);
        activeCameras[3].gameObject.GetComponent<Camera>().rect = new Rect(0.5f, 0.0f, 0.5f, 0.5f);
    }

    /// <summary>
    /// Generate a split view with 5 cameras with robot 1 at upper left, 2 at upper right, 
    /// 3 at lower left, 4 at lower middle, and 5 at lower right
    /// </summary>
    public void SetFiveCamView()
    {
        activeCameras[0].gameObject.GetComponent<Camera>().rect = new Rect(0f, 0.5f, 0.5f, 0.5f);
        activeCameras[1].gameObject.GetComponent<Camera>().rect = new Rect(0.5f, 0.5f, 0.5f, 0.5f);
        activeCameras[2].gameObject.GetComponent<Camera>().rect = new Rect(0.0f, 0.0f, 1 / 3f, 0.5f);
        activeCameras[3].gameObject.GetComponent<Camera>().rect = new Rect(1 / 3f, 0.0f, 1 / 3f, 0.5f);
        activeCameras[4].gameObject.GetComponent<Camera>().rect = new Rect(2 / 3f, 0.0f, 1 / 3f, 0.5f);
    }

    /// <summary>
    /// Generate a split view with 5 cameras with robot 1 at upper left, 2 at upper middle, 
    /// 3 at upper right, 4 at lower left, 5 at lower middle, and 6 at lower right
    /// </summary>
    public void SetSixCamView()
    {
        activeCameras[0].gameObject.GetComponent<Camera>().rect = new Rect(0f, 0.5f, 1 / 3f, 0.5f);
        activeCameras[1].gameObject.GetComponent<Camera>().rect = new Rect(1 / 3f, 0.5f, 1 / 3f, 0.5f);
        activeCameras[2].gameObject.GetComponent<Camera>().rect = new Rect(2 / 3f, 0.5f, 1 / 3f, 0.5f);

        activeCameras[3].gameObject.GetComponent<Camera>().rect = new Rect(0f, 0.0f, 1 / 3f, 0.5f);
        activeCameras[4].gameObject.GetComponent<Camera>().rect = new Rect(1 / 3f, 0.0f, 1 / 3f, 0.5f);
        activeCameras[5].gameObject.GetComponent<Camera>().rect = new Rect(2 / 3f, 0.0f, 1 / 3f, 0.5f);
    }

    ///// <summary>
    ///// Attempted to scale the camera canvas base on their arrangement but failed
    ///// </summary>
    //public void ScaleCanvas()
    //{
    //    foreach (PlayerCamera cam in activeCameras)
    //    {
    //        GameObject canvas = cam.gameObject.transform.GetChild(0).gameObject;
    //        Rect cameraRect = cam.gameObject.GetComponent<Camera>().rect;
    //        canvas.transform.position = new Vector3(cameraRect.x * Screen.width, cameraRect.y * Screen.height, 0f);
    //        canvas.GetComponent<RectTransform>().sizeDelta =
    //            new Vector2(cameraRect.width * Screen.width, cameraRect.height * Screen.height);
    //    }
    //}
}
