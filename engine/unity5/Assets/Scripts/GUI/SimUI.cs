using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BulletUnity;
using Assets.Scripts.FSM;
using System.IO;

/// <summary>
/// SimUI serves as an interface between the Unity button UI and the various functions within the simulator.
/// It acomplishes this by having a public function for each button that interacts with the Main State to complete various tasks.
/// </summary>
public class SimUI : MonoBehaviour
{
    MainState main;
    DynamicCamera camera;
    Toolkit toolkit;
    DriverPracticeMode dpm;

    GameObject canvas;

    GameObject freeroamCameraWindow;
    GameObject spawnpointWindow;
    GameObject robotCameraViewWindow;
    RenderTexture robotCameraView;
    RobotCamera robotCamera;

    GameObject robotCameraList;
    GameObject robotCameraIndicator;
    GameObject showCameraButton;
    GameObject configureRobotCameraButton;
    GameObject cameraConfigurationModeButton;
    GameObject changeCameraNodeButton;
    GameObject configureCameraPanel;
    GameObject cancelNodeSelectionButton;

    GameObject changeRobotPanel;
    GameObject changeFieldPanel;
    GameObject addRobotPanel;

    GameObject driverStationPanel;

    GameObject exitPanel;

    GameObject orientWindow;
    bool isOrienting = false;
    GameObject resetDropdown;

    Text cameraNodeText;



    private bool freeroamWindowClosed = false;
    private bool usingRobotView = false;

    private bool oppositeSide = false;
    private bool indicatorActive = false;

    /// <summary>
    /// Retreives the Main State instance which controls everything in the simulator.
    /// </summary>
    void Start()
    {
    }

    private void Update()
    {
        if (main == null)
        {
            main = transform.GetComponent<StateMachine>().CurrentState as MainState;
            camera = GameObject.Find("Main Camera").GetComponent<DynamicCamera>();
            //Get the render texture from Resources/Images
            robotCameraView = Resources.Load("Images/RobotCameraView") as RenderTexture;
        }
        else if (dpm == null)
        {
            camera = GameObject.Find("Main Camera").GetComponent<DynamicCamera>();

            toolkit = GetComponent<Toolkit>();
            dpm = GetComponent<DriverPracticeMode>();
            FindElements();
        }
        else
        {
            UpdateWindows();

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (StateMachine.Instance.CurrentState.GetType().Equals(typeof(MainState)))
                {
                    if (!exitPanel.activeSelf) MainMenuExit("open");
                    else MainMenuExit("cancel");
                }
            }

        }

    }

    private void OnGUI()
    {
        UserMessageManager.Render();
    }

    /// <summary>
    /// Finds all the necessary UI elements that need to be updated/modified
    /// </summary>
    private void FindElements()
    {
        canvas = GameObject.Find("Canvas");

        freeroamCameraWindow = AuxFunctions.FindObject(canvas, "FreeroamPanel");
        spawnpointWindow = AuxFunctions.FindObject(canvas, "SpawnpointPanel");

        robotCameraViewWindow = AuxFunctions.FindObject(canvas, "RobotCameraPanel");

        

        robotCameraList = GameObject.Find("RobotCameraList");
        robotCameraViewWindow = AuxFunctions.FindObject(canvas, "RobotCameraPanelBorder");

        changeRobotPanel = AuxFunctions.FindObject(canvas, "ChangeRobotPanel");
        changeFieldPanel = AuxFunctions.FindObject(canvas, "ChangeFieldPanel");
        robotCameraIndicator = AuxFunctions.FindObject(robotCameraList, "CameraIndicator");
        showCameraButton = AuxFunctions.FindObject(canvas, "ShowCameraButton");
        configureRobotCameraButton = AuxFunctions.FindObject(canvas, "CameraConfigurationButton");
        changeCameraNodeButton = AuxFunctions.FindObject(canvas, "ChangeNodeButton");
        configureCameraPanel = AuxFunctions.FindObject(canvas, "CameraConfigurationPanel");
        driverStationPanel = AuxFunctions.FindObject(canvas, "DriverStationPanel");


        orientWindow = AuxFunctions.FindObject(canvas, "OrientWindow");
        resetDropdown = GameObject.Find("Reset Robot Dropdown");

        cameraConfigurationModeButton = AuxFunctions.FindObject(canvas, "ConfigurationMode");
        cameraNodeText = AuxFunctions.FindObject(canvas, "NodeText").GetComponent<Text>();
        cancelNodeSelectionButton = AuxFunctions.FindObject(canvas, "CancelNodeSelectionButton");

        exitPanel = AuxFunctions.FindObject(canvas, "ExitPanel");

        addRobotPanel = AuxFunctions.FindObject(canvas, "AddRobotPanel");

    }



    private void UpdateWindows()
    {
        UpdateFreeroamWindow();
        UpdateSpawnpointWindow();
        UpdateCameraWindow();
        UpdateDriverStationPanel();
    }


    #region main button functions
    /// <summary>
    /// Resets the robot
    /// </summary>
    //public void PressReset()
    //{
    //    main.ResetRobot();
    //}
    public void ChangeRobot()
    {
        GameObject panel = GameObject.Find("RobotListPanel");
        string directory = PlayerPrefs.GetString("RobotDirectory") + "\\" + panel.GetComponent<ChangeRobotScrollable>().selectedEntry;
        if (Directory.Exists(directory))
        {
            PlayerPrefs.SetString("simSelectedReplay", string.Empty);
            PlayerPrefs.SetString("simSelectedRobot", directory);
            PlayerPrefs.SetString("simSelectedRobotName", panel.GetComponent<ChangeRobotScrollable>().selectedEntry);
            main.ChangeRobot(directory);
            ToggleChangeRobotPanel();
        }
        else
        {
            UserMessageManager.Dispatch("Robot directory not found!", 5);
        }
    }

    public void ToggleChangeRobotPanel()
    {
        if (changeRobotPanel.activeSelf)
        {
            changeRobotPanel.SetActive(false);
        }
        else
        {
            EndOtherProcesses();
            changeRobotPanel.SetActive(true);
        }
    }

    public void ChangeField()
    {
        GameObject panel = GameObject.Find("FieldListPanel");
        string directory = PlayerPrefs.GetString("FieldDirectory") + "\\" + panel.GetComponent<ChangeFieldScrollable>().selectedEntry;
        if (Directory.Exists(directory))
        {
            PlayerPrefs.SetString("simSelectedReplay", string.Empty);
            PlayerPrefs.SetString("simSelectedField", directory);
            PlayerPrefs.SetString("simSelectedFieldName", panel.GetComponent<ChangeFieldScrollable>().selectedEntry);
            PlayerPrefs.Save();
            Application.LoadLevel("Scene");
        }
        else
        {
            UserMessageManager.Dispatch("Field directory not found!", 5);
        }
    }

    public void ToggleChangeFieldPanel()
    {
        if (changeFieldPanel.activeSelf)
        {
            changeFieldPanel.SetActive(false);
        }
        else
        {
            EndOtherProcesses();
            changeFieldPanel.SetActive(true);
        }

    }

    public void ChooseResetMode(int i)
    {
        switch (i)
        {
            case 1:
                main.BeginRobotReset();
                main.EndRobotReset();
                resetDropdown.GetComponent<Dropdown>().value = 0;
                break;
            case 2:
                EndOtherProcesses();
                main.IsResetting = true;
                main.BeginRobotReset();
                resetDropdown.GetComponent<Dropdown>().value = 0;
                break;
        }
    }

    /// <summary>
    /// Call this function whenever the user enters a new state (ex. selecting a new robot, using ruler function, orenting robot)
    /// </summary>
    public void EndOtherProcesses()
    {
        changeFieldPanel.SetActive(false);
        changeRobotPanel.SetActive(false);
        addRobotPanel.SetActive(false);
        exitPanel.SetActive(false);
        CloseOrientWindow();
        main.IsResetting = false;   

        dpm.EndProcesses();
        toolkit.EndProcesses();
    }
    #endregion
    #region camera button functions
    //Camera Functions
    public void SwitchCameraFreeroam()
    {
        camera.SwitchCameraState(0);
    }

    public void SwitchCameraOrbit()
    {
        camera.SwitchCameraState(1);
    }

    public void SwitchCameraDriverStation()
    {
        camera.SwitchCameraState(2);
    }
    #endregion
    #region orient button functions

    public void ToggleOrientWindow()
    {
        if (isOrienting)
        {
            isOrienting = false;
            main.EndRobotReset();
        }
        else
        {
            EndOtherProcesses();
            isOrienting = true;
            main.BeginRobotReset();
        }
        orientWindow.SetActive(isOrienting);
    }

    public void OrientLeft()
    {
        main.RotateRobot(new Vector3(Mathf.PI * 0.25f, 0f, 0f));
    }
    public void OrientRight()
    {
        main.RotateRobot(new Vector3(-Mathf.PI * 0.25f, 0f, 0f));
    }
    public void OrientForward()
    {
        main.RotateRobot(new Vector3(0f, 0f, Mathf.PI * 0.25f));
    }
    public void OrientBackward()
    {
        main.RotateRobot(new Vector3(0f, 0f, -Mathf.PI * 0.25f));
    }

    public void DefaultOrientation()
    {
        main.ResetRobotOrientation();
        orientWindow.SetActive(isOrienting = false);
    }

    public void SaveOrientation()
    {
        main.SaveRobotOrientation();
        orientWindow.SetActive(isOrienting = false);
    }

    public void CloseOrientWindow()
    {
        isOrienting = false;
        orientWindow.SetActive(isOrienting);
        main.EndRobotReset();
    }

    #endregion
 

    #region robot camera functions
    /// <summary>
    /// Updates the robot camera view window
    /// </summary>
    private void UpdateCameraWindow()
    {
        //Make sure robot camera exists first
        if (robotCamera == null && robotCameraList.GetComponent<RobotCamera>() != null)
        {
            robotCamera = robotCameraList.GetComponent<RobotCamera>();
        }

        if (robotCamera != null)
        {
            //Can use robot view when dynamicCamera is active
            if (usingRobotView && main.dynamicCameraObject.activeSelf)
            {
                robotCamera = robotCameraList.GetComponent<RobotCamera>();
                //Debug.Log(robotCamera.CurrentCamera);

                //Make sure there is camera on robot
                if (robotCamera.CurrentCamera != null)
                {
                    robotCamera.CurrentCamera.SetActive(true);
                    robotCamera.CurrentCamera.GetComponent<Camera>().targetTexture = robotCameraView;
                    //Toggle the robot camera using Q (should be changed later)
                    if (Input.GetKeyDown(KeyCode.Q))
                    {
                        robotCamera.CurrentCamera.GetComponent<Camera>().targetTexture = null;
                        robotCamera.ToggleCamera();
                        robotCamera.CurrentCamera.GetComponent<Camera>().targetTexture = robotCameraView;

                    }
                    //Debug.Log("Robot camera view is " + robotCameraView.name);
                    //Debug.Log(robotCamera.CurrentCamera);
                }
            }
            //Don't allow using robot view window when users are currently using one of the robot view
            else if (usingRobotView && !main.dynamicCameraObject.activeSelf)
            {
                UserMessageManager.Dispatch("You can only use robot view window when you are not in robot view mode!", 2f);
                usingRobotView = false;
                robotCameraViewWindow.SetActive(false);
            }
            //Free the target texture of the current camera when the window is closed (for normal toggle camera function)
            else
            {
                if (robotCamera.CurrentCamera != null)
                    robotCamera.CurrentCamera.GetComponent<Camera>().targetTexture = null;
            }
        }
    }

    /// <summary>
    /// Toggles the state of usingRobotView when the button "Toggle Robot Camera" is clicked
    /// </summary>
    public void ToggleCameraWindow()
    {
        usingRobotView = !usingRobotView;
        robotCameraViewWindow.SetActive(usingRobotView);
        if (usingRobotView)
        {
            robotCamera.CurrentCamera.GetComponent<Camera>().targetTexture = robotCameraView;
        }
        else
        {
            //Free the target texture and disable the camera since robot camera has more depth than main camera
            robotCamera.CurrentCamera.GetComponent<Camera>().targetTexture = null;
            robotCamera.CurrentCamera.SetActive(false);
        }
    }

    /// <summary>
    /// Toggles the state of showing or hiding the robot indicator
    /// </summary>
    public void ToggleCameraIndicator()
    {
        indicatorActive = !indicatorActive;
        if (indicatorActive)
        {
            //Only allow the camera configuration when the indicator is active
            showCameraButton.GetComponentInChildren<Text>().text = "Hide Camera";
            configureRobotCameraButton.SetActive(true);
        }
        else
        {
            showCameraButton.GetComponentInChildren<Text>().text = "Show Camera";
            configureRobotCameraButton.SetActive(false);
            //Close the panel when indicator is not active and stop all configuration
            configureCameraPanel.SetActive(false);
            robotCamera.IsChangingHeight = robotCamera.SelectingNode = robotCamera.ChangingCameraPosition = false;
            configureRobotCameraButton.GetComponentInChildren<Text>().text = "Configure Robot Camera";
            robotCamera.SelectingNode = false;
            robotCamera.SelectedNode = null;
        }
        robotCameraIndicator.SetActive(indicatorActive);
    }

    /// <summary>
    /// Activate the configure camera panel and start position configuration
    /// </summary>
    public void ConfigureCameraPosition()
    {
        robotCamera.ChangingCameraPosition = !robotCamera.ChangingCameraPosition;
        configureCameraPanel.SetActive(robotCamera.ChangingCameraPosition);
        if (robotCamera.ChangingCameraPosition)
        {
            //Update the node where current camera is attached to
            cameraNodeText.text = "Current Node: " + robotCamera.CurrentCamera.transform.parent.gameObject.name;
            configureRobotCameraButton.GetComponentInChildren<Text>().text = "End Configuration";
        }
        else
        {
            configureRobotCameraButton.GetComponentInChildren<Text>().text = "Configure Robot Camera";
        }
    }

    /// <summary>
    /// Toggle between changing position along horizontal plane or changing height
    /// </summary>
    public void ToggleConfigurationMode()
    {
        robotCamera.IsChangingHeight = !robotCamera.IsChangingHeight;
        if (robotCamera.IsChangingHeight)
        {
            cameraConfigurationModeButton.GetComponentInChildren<Text>().text = "Configure Horizontal Plane";
        }
        else
        {
            cameraConfigurationModeButton.GetComponentInChildren<Text>().text = "Configure Height";
        }
    }

    /// <summary>
    /// Going into the state of selecting a new node and confirming it
    /// </summary>
    public void ToggleChangeNode()
    {
        if (!robotCamera.SelectingNode && robotCamera.SelectedNode == null)
        {
            robotCamera.DefineNode(); //Start selecting a new node
            changeCameraNodeButton.GetComponentInChildren<Text>().text = "Confirm";
            cancelNodeSelectionButton.SetActive(true);
        }
        else if (robotCamera.SelectingNode && robotCamera.SelectedNode != null)
        {
            //Change the node where camera is attached to, clear selected node, and update name of current node
            robotCamera.ChangeNodeAttachment();
            cameraNodeText.text = "Current Node: " + robotCamera.CurrentCamera.transform.parent.gameObject.name;
            changeCameraNodeButton.GetComponentInChildren<Text>().text = "Change Attachment Node";
            cancelNodeSelectionButton.SetActive(false);

        }
    }

    public void CancelNodeSelection()
    {
        robotCamera.SelectedNode = null;
        robotCamera.SelectingNode = false;
        cameraNodeText.text = "Current Node: " + robotCamera.CurrentCamera.transform.parent.gameObject.name;
        changeCameraNodeButton.GetComponentInChildren<Text>().text = "Change Attachment Node";
        cancelNodeSelectionButton.SetActive(false);
    }
    #endregion

    /// <summary>
    /// Pop reset instructions when main is in reset spawnpoint mode
    /// </summary>
    private void UpdateSpawnpointWindow()
    {
        if (main.IsResetting)
        {
            spawnpointWindow.SetActive(true);
        }
        else
        {
            spawnpointWindow.SetActive(false);
        }
    }

    /// <summary>
    /// Pop freeroam instructions when using freeroam camera, won't show up again if the user closes it
    /// </summary>
    private void UpdateFreeroamWindow()
    {
        if (camera.cameraState.GetType().Equals(typeof(DynamicCamera.FreeroamState)) && !freeroamWindowClosed)
        {
            if (!freeroamWindowClosed)
            {
                freeroamCameraWindow.SetActive(true);
            }

        }
        else if (!camera.cameraState.GetType().Equals(typeof(DynamicCamera.FreeroamState)))
        {
            freeroamCameraWindow.SetActive(false);
        }
    }


    public void CloseFreeroamWindow()
    {
        freeroamCameraWindow.SetActive(false);
        freeroamWindowClosed = true;
    }

    /// <summary>
    /// Activate driver station panel if the main camera is in driver station state
    /// </summary>
    private void UpdateDriverStationPanel()
    {
        driverStationPanel.SetActive(camera.cameraState.GetType().Equals(typeof(DynamicCamera.DriverStationState)));
    }

    /// <summary>
    /// Change to driver station view to the opposite side
    /// </summary>
    public void ToggleDriverStation()
    {
        oppositeSide = !oppositeSide;
        camera.SwitchCameraState(new DynamicCamera.DriverStationState(camera, oppositeSide));
    }


    public void ShowControlPanel(bool show)
    {
        if (show)
        {
            EndOtherProcesses();
            AuxFunctions.FindObject(canvas, "FullscreenPanel").SetActive(true);
        }
        else
        {
            AuxFunctions.FindObject(canvas, "FullscreenPanel").SetActive(false);
        }
    }

    public void MainMenuExit(string option)
    {
        EndOtherProcesses();
        switch (option)
        {
            case "open":
                exitPanel.SetActive(true);
                break;
            case "exit":
                Application.LoadLevel("MainMenu");
                break;

            case "cancel":
                exitPanel.SetActive(false);
                break;
        }

    }
}