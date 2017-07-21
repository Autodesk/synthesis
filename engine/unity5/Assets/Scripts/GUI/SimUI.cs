using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using BulletUnity;
using Assets.Scripts.FSM;

/// <summary>
/// SimUI serves as an interface between the Unity button UI and the various functions within the simulator.
/// It acomplishes this by having a public function for each button that interacts with the Main State to complete various tasks.
/// </summary>
public class SimUI : MonoBehaviour
{

    MainState main;
    DynamicCamera camera;
    DriverPractice dpm;

    GameObject canvas;

    GameObject dpmWindow;
    GameObject configWindow;
    GameObject defineIntakeWindow;
    GameObject defineReleaseWindow;
    GameObject setSpawnWindow;
    GameObject defineGamepieceWindow;

    GameObject freeroamCameraWindow;
    GameObject spawnpointWindow;
    GameObject robotCameraViewWindow;
    RenderTexture robotCameraView;
    RobotCamera robotCamera;

    GameObject releaseVelocityPanel;

    GameObject xOffsetEntry;
    GameObject yOffsetEntry;
    GameObject zOffsetEntry;
    GameObject releaseSpeedEntry;
    GameObject releaseVerticalEntry;
    GameObject releaseHorizontalEntry;

    GameObject lockPanel;



    Text enableDPMText;

    Text primaryGamepieceText;
    Text secondaryGamepieceText;

    Text intakeControlText;
    Text releaseControlText;
    Text spawnControlText;

    Text configHeaderText;
    Text configuringText;

    Text intakeMechanismText;
    Text releaseMechanismText;

    Text primaryCountText;
    Text secondaryCountText;

    public bool dpmWindowOn = false; //if the driver practice mode window is active

    public bool configuring = false; //if the configuration window is active
    public int configuringIndex = 0; //0 if user is configuring primary, 1 if user is configuring secondary

    private int holdCount = 0; //counts how long a button has been pressed (for add/subtract buttons to increase increment)
    const int holdThreshold = 150;
    const float offsetIncrement = 0.01f;
    const float speedIncrement = 0.1f;
    const float angleIncrement = 1f;

    private float deltaOffsetX;
    private float deltaOffsetY;
    private float deltaOffsetZ;

    private float deltaReleaseSpeed;
    private float deltaReleaseHorizontal;
    private float deltaReleaseVertical;

    private int settingControl = 0; //0 if false, 1 if intake, 2 if release, 3 if spawn

    bool isEditing = false;

    private bool freeroamWindowClosed = false;
    private bool usingRobotView = false;

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
            main = transform.GetComponent<StateMachine>().MainState;
            camera = GameObject.Find("Main Camera").GetComponent<DynamicCamera>();
            //Get the render texture from Resources/Images
            robotCameraView = Resources.Load("Images/RobotCameraView") as RenderTexture;
        }
        else if (dpm == null)
        {
            dpm = main.GetDriverPractice();
            FindElements();


        }
        else
        {
            UpdateDPMValues();
            UpdateVectorConfiguration();
            UpdateWindows();
            if (settingControl != 0) ListenControl();

        }

    }

    private void OnGUI()
    {
    }

    /// <summary>
    /// Finds all the necessary UI elements that need to be updated/modified
    /// </summary>
    private void FindElements()
    {
        canvas = GameObject.Find("Canvas");

        dpmWindow = AuxFunctions.FindObject(canvas, "DPMPanel");
        configWindow = AuxFunctions.FindObject(canvas, "ConfigurationPanel");

        enableDPMText = AuxFunctions.FindObject(canvas, "EnableDPMText").GetComponent<Text>();

        primaryGamepieceText = AuxFunctions.FindObject(canvas, "PrimaryGamepieceText").GetComponent<Text>();
        secondaryGamepieceText = AuxFunctions.FindObject(canvas, "SecondaryGamepieceText").GetComponent<Text>();

        configuringText = AuxFunctions.FindObject(canvas, "ConfiguringText").GetComponent<Text>();
        configHeaderText = AuxFunctions.FindObject(canvas, "ConfigHeaderText").GetComponent<Text>();

        releaseVelocityPanel = AuxFunctions.FindObject(canvas, "ReleaseVelocityPanel");

        xOffsetEntry = AuxFunctions.FindObject(canvas, "XOffsetEntry");
        yOffsetEntry = AuxFunctions.FindObject(canvas, "YOffsetEntry");
        zOffsetEntry = AuxFunctions.FindObject(canvas, "ZOffsetEntry");
        releaseSpeedEntry = AuxFunctions.FindObject(canvas, "ReleaseSpeedEntry");
        releaseVerticalEntry = AuxFunctions.FindObject(canvas, "ReleaseVerticalEntry");
        releaseHorizontalEntry = AuxFunctions.FindObject(canvas, "ReleaseHorizontalEntry");

        releaseMechanismText = AuxFunctions.FindObject(canvas, "ReleaseMechanismText").GetComponent<Text>();
        intakeMechanismText = AuxFunctions.FindObject(canvas, "IntakeMechanismText").GetComponent<Text>();

        defineIntakeWindow = AuxFunctions.FindObject(canvas, "DefineIntakePanel");
        defineReleaseWindow = AuxFunctions.FindObject(canvas, "DefineReleasePanel");
        defineGamepieceWindow = AuxFunctions.FindObject(canvas, "DefineGamepiecePanel");
        setSpawnWindow = AuxFunctions.FindObject(canvas, "SetGamepieceSpawnPanel");

        intakeControlText = AuxFunctions.FindObject(canvas, "IntakeInputButton").GetComponentInChildren<Text>();
        releaseControlText = AuxFunctions.FindObject(canvas, "ReleaseInputButton").GetComponentInChildren<Text>();
        spawnControlText = AuxFunctions.FindObject(canvas, "SpawnInputButton").GetComponentInChildren<Text>();

        lockPanel = AuxFunctions.FindObject(canvas, "DPMLockPanel");

        freeroamCameraWindow = AuxFunctions.FindObject(canvas, "FreeroamPanel");
        spawnpointWindow = AuxFunctions.FindObject(canvas, "SpawnpointPanel");

        robotCameraViewWindow = AuxFunctions.FindObject(canvas, "RobotCameraPanel");

        primaryCountText = AuxFunctions.FindObject(canvas, "PrimaryCountText").GetComponent<Text>();
        secondaryCountText = AuxFunctions.FindObject(canvas, "SecondaryCountText").GetComponent<Text>();

        robotCameraViewWindow = AuxFunctions.FindObject(canvas, "RobotCameraPanelBorder");
    }

    /// <summary>
    /// Updates the UI elements in the driver practice mode toolbars to reflect changes in configurable values
    /// </summary>
    private void UpdateDPMValues()
    {
        if (dpm.gamepieceNames[0] == null) primaryGamepieceText.text = "Primary Gamepiece:  NOT CONFIGURED";
        else primaryGamepieceText.text = "Primary Gamepiece:  " + dpm.gamepieceNames[0];

        if (dpm.gamepieceNames[1] == null) secondaryGamepieceText.text = "Secondary Gamepiece:  NOT CONFIGURED";
        else secondaryGamepieceText.text = "Secondary Gamepiece:  " + dpm.gamepieceNames[1];

        primaryCountText.text = "Spawned: " + dpm.spawnedPrimary.Count + "\nHeld: " + dpm.objectsHeld[0].Count;
        secondaryCountText.text = "Spawned: " + dpm.spawnedSecondary.Count + "\nHeld: " + dpm.objectsHeld[1].Count;

        if (configuring)
        {
            if (dpm.gamepieceNames[configuringIndex] == null) configuringText.text = "Gamepiece not defined yet!";
            else configuringText.text = "Configuring:  " + dpm.gamepieceNames[configuringIndex];

            releaseMechanismText.text = "Current Part:  " + dpm.releaseNode[configuringIndex].name;
            intakeMechanismText.text = "Current Part:  " + dpm.intakeNode[configuringIndex].name;

            if (configuringIndex == 0)
            {
                intakeControlText.text = Controls.ControlKey[(int)Controls.Control.PickupPrimary].ToString();
                releaseControlText.text = Controls.ControlKey[(int)Controls.Control.ReleasePrimary].ToString();
                spawnControlText.text = Controls.ControlKey[(int)Controls.Control.SpawnPrimary].ToString();
            }
            else
            {
                intakeControlText.text = Controls.ControlKey[(int)Controls.Control.PickupSecondary].ToString();
                releaseControlText.text = Controls.ControlKey[(int)Controls.Control.ReleaseSecondary].ToString();
                spawnControlText.text = Controls.ControlKey[(int)Controls.Control.SpawnSecondary].ToString();
            }
        }
    }

    /// <summary>
    /// Updates DPM values regarding release position vector and release velocity values
    /// </summary>
    private void UpdateVectorConfiguration()
    {
        if (holdCount > 0 && !isEditing) //This indicates that any of the configuration increment buttons are being pressed
        {
            if (deltaOffsetX != 0)
            {
                if (holdCount < holdThreshold) dpm.ChangeOffsetX(deltaOffsetX, configuringIndex);
                else dpm.ChangeOffsetX(deltaOffsetX * 5, configuringIndex);
            }
            else if (deltaOffsetY != 0)
            {
                if (holdCount < holdThreshold) dpm.ChangeOffsetY(deltaOffsetY, configuringIndex);
                else dpm.ChangeOffsetY(deltaOffsetY * 5, configuringIndex);
            }
            else if (deltaOffsetZ != 0)
            {
                if (holdCount < holdThreshold) dpm.ChangeOffsetZ(deltaOffsetZ, configuringIndex);
                else dpm.ChangeOffsetZ(deltaOffsetZ * 5, configuringIndex);
            }
            else if (deltaReleaseSpeed != 0)
            {
                if (holdCount < holdThreshold) dpm.ChangeReleaseSpeed(deltaReleaseSpeed, configuringIndex);
                else dpm.ChangeReleaseSpeed(deltaReleaseSpeed * 5, configuringIndex);
            }
            else if (deltaReleaseHorizontal != 0)
            {
                if (holdCount < holdThreshold) dpm.ChangeReleaseHorizontalAngle(deltaReleaseHorizontal, configuringIndex);
                else dpm.ChangeReleaseHorizontalAngle(deltaReleaseHorizontal * 5, configuringIndex);
            }
            else if (deltaReleaseVertical != 0)
            {
                if (holdCount < holdThreshold) dpm.ChangeReleaseVerticalAngle(deltaReleaseVertical, configuringIndex);
                else dpm.ChangeReleaseVerticalAngle(deltaReleaseVertical * 5, configuringIndex);
            }
            holdCount++;
        }

        if (!isEditing)
        {

            xOffsetEntry.GetComponent<InputField>().text = dpm.positionOffset[configuringIndex][0].ToString();
            yOffsetEntry.GetComponent<InputField>().text = dpm.positionOffset[configuringIndex][1].ToString();
            zOffsetEntry.GetComponent<InputField>().text = dpm.positionOffset[configuringIndex][2].ToString();

            releaseSpeedEntry.GetComponent<InputField>().text = dpm.releaseVelocity[configuringIndex][0].ToString();
            releaseHorizontalEntry.GetComponent<InputField>().text = dpm.releaseVelocity[configuringIndex][1].ToString();
            releaseVerticalEntry.GetComponent<InputField>().text = dpm.releaseVelocity[configuringIndex][2].ToString();
        }
    }

    private void UpdateWindows()
    {
        if (configuring)
        {
            if (dpm.addingGamepiece)
            {
                configWindow.SetActive(false);
                dpmWindow.SetActive(false);
                defineGamepieceWindow.SetActive(true);
            }
            else if (dpm.settingSpawn != 0)
            {
                configWindow.SetActive(false);
                dpmWindow.SetActive(false);
                setSpawnWindow.SetActive(true);
            }
            else if (dpm.definingIntake)
            {
                configWindow.SetActive(false);
                dpmWindow.SetActive(false);
                defineIntakeWindow.SetActive(true);
            }
            else if (dpm.definingRelease)
            {
                configWindow.SetActive(false);
                dpmWindow.SetActive(false);
                defineReleaseWindow.SetActive(true);
            }
            else
            {
                defineGamepieceWindow.SetActive(false);
                setSpawnWindow.SetActive(false);
                defineIntakeWindow.SetActive(false);
                defineReleaseWindow.SetActive(false);
                dpmWindow.SetActive(true);
                configWindow.SetActive(true);
            }
        }

        UpdateFreeroamWindow();
        UpdateSpawnpointWindow();
        UpdateCameraWindow();

    }

    private void UpdateCameraView()
    {

    }
    #region main button functions
    /// <summary>
    /// Resets the robot
    /// </summary>
    //public void PressReset()
    //{
    //    main.ResetRobot();
    //}
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

    ////Orient Robot Functions
    //public void OrientStart()
    //{
    //    main.StartOrient();
    //}

    //public void OrientLeft()
    //{
    //    main.RotateRobot(new Vector3(Mathf.PI * 0.25f, 0f, 0f));
    //}

    //public void OrientRight()
    //{
    //    main.RotateRobot(new Vector3(-Mathf.PI * 0.25f, 0f, 0f));
    //}

    //public void OrientForward()
    //{
    //    main.RotateRobot(new Vector3(0f, 0f, Mathf.PI * 0.25f));
    //}

    //public void OrientBackward()
    //{
    //    main.RotateRobot(new Vector3(0f, 0f, -Mathf.PI * 0.25f));
    //}

    //public void OrientSave()
    //{
    //    main.SaveOrientation();
    //}

    //public void OrientEnd()
    //{
    //    //To be filled in later when UI work has been done
    //}

    //public void OrientDefault()
    //{
    //    main.ResetOrientation();
    //}

    #endregion
    #region driver practice mode button functions
    /// <summary>
    /// Toggles the Driver Practice Mode window
    /// </summary>
    public void DPMToggleWindow()
    {
        dpmWindowOn = !dpmWindowOn;
        dpmWindow.SetActive(dpmWindowOn);
    }

    /// <summary>
    /// Sets the driver practice mode to either be enabled or disabled, depending on what state it was at before.
    /// </summary>
    public void DPMToggle()
    {
        if (!dpm.modeEnabled)
        {
            dpm.modeEnabled = true;
            enableDPMText.text = "Disable Driver Practice Mode";
            lockPanel.SetActive(false);
            
        }
        else
        {
            if (configuring) UserMessageManager.Dispatch("You must close the configuration window first!", 5);
            else
            {
                enableDPMText.text = "Enable Driver Practice Mode";
                dpm.modeEnabled = false;
                lockPanel.SetActive(true);
                dpm.displayTrajectories[0] = false;
                dpm.displayTrajectories[1] = false;
            }

        }
    }

    /// <summary>
    /// Clears all the gamepieces sharing the same name as the ones that have been configured from the field.
    /// </summary>
    public void ClearGamepieces()
    {
        dpm.ClearGamepieces();
    }

    /// <summary>
    /// Toggles the display of primary gamepiece release trajectory.
    /// </summary>
    public void DisplayTrajectoryPrimary()
    {
        dpm.displayTrajectories[0] = !dpm.displayTrajectories[0];
    }

    /// <summary>
    /// Toggles the display of primary gamepiece release trajectory.
    /// </summary>
    public void DisplayTrajectorySecondary()        
    {
        dpm.displayTrajectories[1] = !dpm.displayTrajectories[1];
    }

    /// <summary>
    /// Opens the configuration window and sets it up for the primary gamepiece
    /// </summary>
    public void DPMConfigurePrimary()
    {
        if (dpm.modeEnabled)
        {
            configuring = true;
            configuringIndex = 0;
            configHeaderText.text = "Configuration Menu - Primary Gamepiece";
            configWindow.SetActive(true);
            dpm.displayTrajectories[0] = true;
            dpm.displayTrajectories[1] = false;
        }
        else UserMessageManager.Dispatch("You must enable Driver Practice Mode first!", 5);
    }

    /// <summary>
    /// Opens the configuration window and sets it up for the secondary gamepiece
    /// </summary>
    public void DPMConfigureSecondary()
    {
        if (dpm.modeEnabled)
        {
            configuring = true;
            configuringIndex = 1;
            configHeaderText.text = "Configuration Menu - Secondary Gamepiece";
            configWindow.SetActive(true);
            dpm.displayTrajectories[0] = false;
            dpm.displayTrajectories[1] = true;
        }
        else UserMessageManager.Dispatch("You must enable Driver Practice Mode first!", 5);
    }

    /// <summary>
    /// Spawns the primary gamepiece at its defined spawn location, or at the field's origin if one hasn't been defined
    /// </summary>
    public void SpawnGamepiecePrimary()
    {
        dpm.SpawnGamepiece(0);
    }

    /// <summary>
    /// Spawns the secondary gamepiece at its defined spawn location, or at the field's origon if one hasn't been defined.
    /// </summary>
    public void SpawnGamepieceSecondary()
    {
        dpm.SpawnGamepiece(1);
    }

    #endregion
    #region dpm configuration button functions
    public void CloseConfigurationWindow()
    {
        configWindow.SetActive(false);
        configuring = false;
        dpm.displayTrajectories[configuringIndex] = false;
        dpm.Save();
    }

    public void DefineGamepiece()
    {
        dpm.DefineGamepiece(configuringIndex);
    }

    public void CancelDefineGamepiece()
    {
        dpm.addingGamepiece = false;
    }

    public void DefineIntake()
    {
        dpm.DefineIntake(configuringIndex);
    }

    public void CancelDefineIntake()
    {
        dpm.definingIntake = false;
    }

    public void CloseFreeroamWindow()
    {
        freeroamCameraWindow.SetActive(false);
        freeroamWindowClosed = true;
    }

    public void HighlightIntake()
    {
        dpm.HighlightNode(dpm.intakeNode[configuringIndex].name);
    }

    public void DefineRelease()
    {
        dpm.DefineRelease(configuringIndex);
    }

    public void CancelDefineRelease()
    {
        dpm.definingRelease = false;
    }

    public void HighlightRelease()
    {
        dpm.HighlightNode(dpm.releaseNode[configuringIndex].name);
    }

    public void SetGamepieceSpawn()
    {
        dpm.StartGamepieceSpawn(configuringIndex);
    }

    public void CancelGamepieceSpawn()
    {
        dpm.FinishGamepieceSpawn();
    }




    public void AddOffsetX()
    {
        deltaOffsetX = offsetIncrement;
        holdCount++;
    }
    public void SubstractOffsetX()
    {
        deltaOffsetX = -offsetIncrement;
        holdCount++;
    }
    public void AddOffsetY()
    {
        deltaOffsetY = offsetIncrement;
        holdCount++;
    }
    public void SubstractOffsetY()
    {
        deltaOffsetY = -offsetIncrement;
        holdCount++;
    }
    public void AddOffsetZ()
    {
        deltaOffsetZ = offsetIncrement;
        holdCount++;
    }
    public void SubtractOffsetZ()
    {
        deltaOffsetZ = -offsetIncrement;
        holdCount++;
    }

    public void AddReleaseSpeed()
    {
        deltaReleaseSpeed = speedIncrement;
        holdCount++;
    }
    public void SubtractReleaseSpeed()
    {
        deltaReleaseSpeed = -speedIncrement;
        holdCount++;
    }
    public void AddReleaseHorizontalAngle()
    {
        deltaReleaseHorizontal = angleIncrement;
        holdCount++;
    }
    public void SubtractReleaseHorizontalAngle()
    {
        deltaReleaseHorizontal = -angleIncrement;
        holdCount++;
    }
    public void AddReleaseVerticalAngle()
    {
        deltaReleaseVertical = angleIncrement;
        holdCount++;
    }
    public void SubtractReleaseVerticalAngle()
    {
        deltaReleaseVertical = -angleIncrement;
        holdCount++;
    }
    public void ReleaseConfigurationButton()
    {
        deltaOffsetX = 0;
        deltaOffsetY = 0;
        deltaOffsetZ = 0;

        deltaReleaseSpeed = 0;
        deltaReleaseHorizontal = 0;
        deltaReleaseVertical = 0;
        holdCount = 0;
    }

    public void StartEdit()
    {
        isEditing = true;
    }

    public void EndEdit()
    {
        isEditing = false;
    }

    public void SyncInputFieldEntry()
    {
        if (isEditing)
        {
            float temp = 0;
            if (float.TryParse(xOffsetEntry.GetComponent<InputField>().text, out temp))
                dpm.positionOffset[configuringIndex][0] = temp;
            temp = 0;
            if (float.TryParse(yOffsetEntry.GetComponent<InputField>().text, out temp))
                dpm.positionOffset[configuringIndex][1] = temp;
            temp = 0;
            if (float.TryParse(zOffsetEntry.GetComponent<InputField>().text, out temp))
                dpm.positionOffset[configuringIndex][2] = temp;
            temp = 0;
            if (float.TryParse(releaseSpeedEntry.GetComponent<InputField>().text, out temp))
                dpm.releaseVelocity[configuringIndex][0] = temp;
            temp = 0;
            if (float.TryParse(releaseHorizontalEntry.GetComponent<InputField>().text, out temp))
                dpm.releaseVelocity[configuringIndex][1] = temp;
            temp = 0;
            if (float.TryParse(releaseVerticalEntry.GetComponent<InputField>().text, out temp))
                dpm.releaseVelocity[configuringIndex][2] = temp;
        }
    }
    #endregion
    #region control customization functions

    public void ChangeIntakeControl()
    {
        settingControl = 1;
    }

    public void ChangeReleaseControl()
    {
        settingControl = 2;
    }

    public void ChangeSpawnControl()
    {
        settingControl = 3;
    }

    private void ListenControl()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            settingControl = 0;
            return;
        }
        foreach (KeyCode vKey in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(vKey))
            {
                if (configuringIndex == 0)
                {
                    if (settingControl == 1)
                    {
                        Controls.SetControl((int)Controls.Control.PickupPrimary, vKey);
                    }
                    else if (settingControl == 2) Controls.SetControl((int)Controls.Control.ReleasePrimary, vKey);
                    else Controls.SetControl((int)Controls.Control.SpawnPrimary, vKey);
                }
                else
                {
                    if (settingControl == 1) Controls.SetControl((int)Controls.Control.PickupSecondary, vKey);
                    else if (settingControl == 2) Controls.SetControl((int)Controls.Control.ReleaseSecondary, vKey);
                    else Controls.SetControl((int)Controls.Control.SpawnPrimary, vKey);
                }
                Controls.SaveControls();
                settingControl = 0;
            }
        }
    }
    #endregion

    /// <summary>
    /// Updates the robot camera view window
    /// </summary>
    private void UpdateCameraWindow()
    {
        //Make sure robot camera exists first
        if(robotCamera == null && AuxFunctions.FindObject("RobotCameraList").GetComponent<RobotCamera>() != null)
        {
            robotCamera = AuxFunctions.FindObject("RobotCameraList").GetComponent<RobotCamera>();
        }

        if (robotCamera != null)
        {
            //Can use robot view when dynamicCamera is active
            if (usingRobotView && main.dynamicCameraObject.activeSelf)
            {
                robotCamera = AuxFunctions.FindObject("RobotCameraList").GetComponent<RobotCamera>();
                Debug.Log(robotCamera.CurrentCamera);

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
}

