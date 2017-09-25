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
    public List<GameObject> playerCameras = new List<GameObject>();
    private List<PlayerCamera> activeCameras = new List<PlayerCamera>();
    private GameObject multiplayerWindow;

    private int activeCount;
    private GameObject splitViewManager;

    private void Start()
    {
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
        if (Input.GetKeyDown(KeyCode.Escape)) EndSplitView();
    }

    public void AssignRobotCamera(int index, GameObject target)
    {
        PlayerCamera playerCam = playerCameras[index].GetComponent<PlayerCamera>();
        playerCam.SetActive(true);
        activeCameras.Add(playerCam);
        activeCount++;
        playerCam.cameraState.SetTargetRobot(target);
    }

    public void InitiateSplitView()
    {
        List<int> indexes = new List<int>();
        Debug.Log(mainState.SpawnedRobots[0]);
        foreach (Robot robot in mainState.SpawnedRobots)
        {
            indexes.Add(robot.DriverStationIndex);
        }
        if (indexes.Distinct().Count() == indexes.Count)
        {
            foreach (Robot robot in mainState.SpawnedRobots)
            {
                AssignRobotCamera(robot.DriverStationIndex, robot.gameObject);

                Debug.Log(robot.DriverStationIndex);
            }

            if (activeCount > 1) {
                GenerateSplitView();
            }
            else
            {
                UserMessageManager.Dispatch("Please have two/more robots to use split view!", 5f);
                //activeCount = 0;
                ClearCameraTarget();
            }
        }
        else
        {
            UserMessageManager.Dispatch("You can't control two/more robots from the same driver station!", 5f);
        }
    }

    public void GenerateSplitView()
    {
        mainCamera.SetActive(false);
        switch (activeCount)
        {
            case 2:
                SetTwoCamView();
                break;
            case 3:
                SetThreeCamView();
                break;
            case 4:
                //SetFourCamView();
                break;
            case 5:
                //SetFiveCamView();
                break;
            case 6:
                //SetSixCamView();
                break;
            default:
                break;
        }
    }

    public void EndSplitView()
    {
        mainCamera.SetActive(true);
        ClearCameraTarget();
    }
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

    public void SetTwoCamView()
    {
        activeCameras[0].gameObject.GetComponent<Camera>().rect = new Rect(0, 0, 0.5f, 1);
        activeCameras[1].gameObject.GetComponent<Camera>().rect = new Rect(0.5f, 0, 0.5f, 1);
    }

    public void SetThreeCamView()
    {
        activeCameras[0].gameObject.GetComponent<Camera>().rect = new Rect(0.3f, 0, 0.5f, 0.5f);
        activeCameras[1].gameObject.GetComponent<Camera>().rect = new Rect(0f, 0.5f, 0.5f, 1);
        activeCameras[2].gameObject.GetComponent<Camera>().rect = new Rect(0.5f, 0.5f, 0.5f, 1);
    }
}
