using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.FSM;

class RobotCameraGUI : MonoBehaviour
{
    //GUI stuff
    MainState main;
    GameObject canvas;
    DynamicCamera dynamicCamera;
    DynamicCamera.CameraState preConfigCamState;
    GameObject robotCameraListObject;
    public GameObject CameraIndicator;

    GameObject cameraAnglePanel;
    GameObject xAngleEntry;
    GameObject yAngleEntry;
    GameObject zAngleEntry;
    GameObject showAngleButton;
    GameObject editAngleButton;

    GameObject cameraFOVPanel;
    GameObject editFOVButton;
    GameObject showFOVButton;
    GameObject FOVEntry;

    GameObject robotCameraViewWindow;
    RenderTexture robotCameraView;
    RobotCamera robotCamera;

    GameObject robotCameraIndicator;
    GameObject showCameraButton;
    GameObject configureRobotCameraButton;
    GameObject cameraConfigurationModeButton;
    GameObject changeCameraNodeButton;
    GameObject configureCameraPanel;
    GameObject cancelNodeSelectionButton;

    GameObject lockPositionButton;
    GameObject lockAngleButton;
    GameObject lockFOVButton;

    Text cameraNodeText;

    private bool usingRobotView = false;
    private bool indicatorActive = false;
    private bool isEditingAngle;
    private bool isEditingFOV;


    private void Start()
    {
        FindGUIElements();
    }

    private void Update()
    {
        if (main == null)
        {
            main = GameObject.Find("StateMachine").GetComponent<StateMachine>().CurrentState as MainState;
        }
        else if (dynamicCamera == null && main.dynamicCameraObject != null)
        {
            dynamicCamera = main.dynamicCameraObject.GetComponent<DynamicCamera>();
        }
        else if (main != null && dynamicCamera != null)
        {
            UpdateCameraWindow();
            UpdateCameraAnglePanel();
            UpdateCameraFOVPanel();
            UpdateNodeAttachment();
        }

        if (CameraIndicator.activeSelf)
        {
            CameraIndicator.transform.position = robotCamera.CurrentCamera.transform.position;
            CameraIndicator.transform.rotation = robotCamera.CurrentCamera.transform.rotation;

            CameraIndicator.transform.parent = robotCamera.CurrentCamera.transform;
        }
    }

    #region robot camera GUI functions

    /// <summary>
    /// Find all robot camera related GUI elements in the canvas
    /// </summary>
    public void FindGUIElements()
    {
        canvas = GameObject.Find("Canvas");

        //For robot camera view window
        robotCameraView = Resources.Load("Images/RobotCameraView") as RenderTexture;
        robotCameraViewWindow = AuxFunctions.FindObject(canvas, "RobotCameraPanel");
        robotCameraViewWindow = AuxFunctions.FindObject(canvas, "RobotCameraPanelBorder");

        //For camera indicator
        robotCameraListObject = GameObject.Find("RobotCameraList");
        robotCamera = robotCameraListObject.GetComponent<RobotCamera>();

        if (CameraIndicator == null)
        {
            CameraIndicator = AuxFunctions.FindObject(robotCameraListObject, "CameraIndicator");
        }
        showCameraButton = AuxFunctions.FindObject(canvas, "ShowCameraButton");

        //For camera position and attachment configuration
        configureRobotCameraButton = AuxFunctions.FindObject(canvas, "CameraConfigurationButton");
        changeCameraNodeButton = AuxFunctions.FindObject(canvas, "ChangeNodeButton");
        configureCameraPanel = AuxFunctions.FindObject(canvas, "CameraConfigurationPanel");
        cameraConfigurationModeButton = AuxFunctions.FindObject(canvas, "ConfigurationMode");
        cameraNodeText = AuxFunctions.FindObject(canvas, "NodeText").GetComponent<Text>();
        cancelNodeSelectionButton = AuxFunctions.FindObject(canvas, "CancelNodeSelectionButton");

        //For camera angle configuration
        cameraAnglePanel = AuxFunctions.FindObject(canvas, "CameraAnglePanel");
        xAngleEntry = AuxFunctions.FindObject(cameraAnglePanel, "xAngleEntry");
        yAngleEntry = AuxFunctions.FindObject(cameraAnglePanel, "yAngleEntry");
        zAngleEntry = AuxFunctions.FindObject(cameraAnglePanel, "zAngleEntry");
        showAngleButton = AuxFunctions.FindObject(configureCameraPanel, "ShowCameraAngleButton");
        editAngleButton = AuxFunctions.FindObject(cameraAnglePanel, "EditButton");

        //For field of view configuration
        cameraFOVPanel = AuxFunctions.FindObject(canvas, "CameraFOVPanel");
        FOVEntry = AuxFunctions.FindObject(cameraFOVPanel, "FOVEntry");
        showFOVButton = AuxFunctions.FindObject(configureCameraPanel, "ShowCameraFOVButton");
        editFOVButton = AuxFunctions.FindObject(cameraFOVPanel, "EditButton");

        lockPositionButton = AuxFunctions.FindObject(configureCameraPanel, "LockPositionButton");
        lockAngleButton = AuxFunctions.FindObject(configureCameraPanel, "LockAngleButton");
        lockFOVButton = AuxFunctions.FindObject(configureCameraPanel, "LockFOVButton");
    }
    /// <summary>
    /// Updates the robot camera view window
    /// </summary>
    private void UpdateCameraWindow()
    {
        //Can use robot view when dynamicCamera is active
        if (usingRobotView && main.dynamicCameraObject.activeSelf)
        {
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
            //Close the panel when indicator is not active and stop all configuration
            configureCameraPanel.SetActive(false);
            if (robotCamera.ChangingCameraPosition) dynamicCamera.SwitchToState(preConfigCamState);
            robotCamera.IsChangingHeight = robotCamera.SelectingNode = robotCamera.ChangingCameraPosition = false;
            configureRobotCameraButton.GetComponentInChildren<Text>().text = "Configure Robot Camera";
            robotCamera.SelectingNode = false;
            robotCamera.SelectedNode = null;
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
            if (robotCamera.ChangingCameraPosition) dynamicCamera.SwitchToState(preConfigCamState);
            robotCamera.IsChangingHeight = robotCamera.SelectingNode = robotCamera.ChangingCameraPosition = false;
            configureRobotCameraButton.GetComponentInChildren<Text>().text = "Configure Robot Camera";
            robotCamera.SelectingNode = false;
            robotCamera.SelectedNode = null;
        }
        CameraIndicator.SetActive(indicatorActive);
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
            preConfigCamState = dynamicCamera.cameraState;
            dynamicCamera.SwitchCameraState(new DynamicCamera.CameraConfigurationState(dynamicCamera));
            //Update the node where current camera is attached to
            cameraNodeText.text = "Current Node: " + robotCamera.CurrentCamera.transform.parent.gameObject.name;
            configureRobotCameraButton.GetComponentInChildren<Text>().text = "End Configuration";
        }
        else
        {
            configureRobotCameraButton.GetComponentInChildren<Text>().text = "Configure Robot Camera";
            dynamicCamera.SwitchToState(preConfigCamState);
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

    /// <summary>
    /// Exit the state of selecting node attachment
    /// </summary>
    public void CancelNodeSelection()
    {
        robotCamera.SelectedNode = null;
        robotCamera.SelectingNode = false;
        cameraNodeText.text = "Current Node: " + robotCamera.CurrentCamera.transform.parent.gameObject.name;
        changeCameraNodeButton.GetComponentInChildren<Text>().text = "Change Attachment Node";
        cancelNodeSelectionButton.SetActive(false);
    }

    /// <summary>
    /// Update the local angle of the current camera to the camera angle panel
    /// </summary>
    public void UpdateCameraAnglePanel()
    {
        if (!isEditingAngle)
        {
            xAngleEntry.GetComponent<InputField>().text = robotCamera.CurrentCamera.transform.localEulerAngles.x.ToString();
            yAngleEntry.GetComponent<InputField>().text = robotCamera.CurrentCamera.transform.localEulerAngles.y.ToString();
            zAngleEntry.GetComponent<InputField>().text = robotCamera.CurrentCamera.transform.localEulerAngles.z.ToString();
        }
    }

    /// <summary>
    /// Take the angle input and set the rotation of the current camera
    /// </summary>
    public void SyncCameraAngle()
    {
        float xTemp = 0;
        float yTemp = 0;
        float zTemp = 0;
        if (float.TryParse(xAngleEntry.GetComponent<InputField>().text, out xTemp) &&
            float.TryParse(yAngleEntry.GetComponent<InputField>().text, out yTemp) &&
            float.TryParse(zAngleEntry.GetComponent<InputField>().text, out zTemp))
        {
            //Debug.Log("Sync angle!");
            robotCamera.CurrentCamera.transform.localRotation = Quaternion.Euler(new Vector3(xTemp, yTemp, zTemp));
        }

    }

    /// <summary>
    /// Control the button that toggles camera angle panel
    /// </summary>
    public void ToggleCameraAnglePanel()
    {
        robotCamera.IsShowingAngle = !robotCamera.IsShowingAngle;
        cameraAnglePanel.SetActive(robotCamera.IsShowingAngle);

        lockPositionButton.SetActive(robotCamera.IsShowingAngle);
        lockFOVButton.SetActive(robotCamera.IsShowingAngle);

        if (robotCamera.IsShowingAngle)
        {
            showAngleButton.GetComponentInChildren<Text>().text = "Hide Camera Angle";
        }
        else
        {
            showAngleButton.GetComponentInChildren<Text>().text = "Show/Edit Camera Angle";
        }
    }

    /// <summary>
    /// Toggle angle edit mode and update angle from the input
    /// </summary>
    public void ToggleEditAngle()
    {
        isEditingAngle = !isEditingAngle;
        if (isEditingAngle)
        {
            editAngleButton.GetComponentInChildren<Text>().text = "Done";
        }
        else
        {
            editAngleButton.GetComponentInChildren<Text>().text = "Edit";
            SyncCameraAngle();
            isEditingAngle = false;
        }
    }

    /// <summary>
    /// Update the local FOV of the current camera to the camera angle panel
    /// </summary>
    public void UpdateCameraFOVPanel()
    {
        if (!isEditingFOV)
        {
            FOVEntry.GetComponent<InputField>().text = robotCamera.CurrentCamera.GetComponent<Camera>().fieldOfView.ToString();
        }
    }

    /// <summary>
    /// Take the FOV input and set the rotation of the current camera
    /// </summary>
    public void SyncCameraFOV()
    {
        float temp = 0;
        if ((float.TryParse(FOVEntry.GetComponent<InputField>().text, out temp) && temp >= 0))
        {
            robotCamera.CurrentCamera.GetComponent<Camera>().fieldOfView = temp;
        }
        else
        {
            robotCamera.CurrentCamera.GetComponent<Camera>().fieldOfView = temp;
        }

    }

    /// <summary>
    /// Control the button that toggles camera FOV panel
    /// </summary>
    public void ToggleCameraFOVPanel()
    {
        robotCamera.IsChangingFOV = !robotCamera.IsChangingFOV;
        cameraFOVPanel.SetActive(robotCamera.IsChangingFOV);

        lockPositionButton.SetActive(robotCamera.IsChangingFOV);
        lockAngleButton.SetActive(robotCamera.IsChangingFOV);

        if (robotCamera.IsChangingFOV)
        {
            showFOVButton.GetComponentInChildren<Text>().text = "Hide Camera FOV";
        }
        else
        {
            showFOVButton.GetComponentInChildren<Text>().text = "Show/Edit Camera FOV";
        }
    }

    /// <summary>
    /// Toggle FOV edit mode and update FOV from the input
    /// </summary>
    public void ToggleEditFOV()
    {
        isEditingFOV = !isEditingFOV;
        if (isEditingFOV)
        {
            editFOVButton.GetComponentInChildren<Text>().text = "Done";
        }
        else
        {
            editFOVButton.GetComponentInChildren<Text>().text = "Edit";
            SyncCameraFOV();
            isEditingFOV = false;
        }
    }

    public void UpdateNodeAttachment()
    {
        cameraNodeText.text = "Current Node: " + robotCamera.CurrentCamera.transform.parent.gameObject.name;
    }

    public void EndProcesses()
    {
        cameraAnglePanel.SetActive(false);
        if (indicatorActive)
        {
            ToggleCameraIndicator();
        }
        if (usingRobotView)
        {
            ToggleCameraWindow();
        }
    }
    #endregion

}

