using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.FSM;

public class DriverPracticeMode : MonoBehaviour {

    private DriverPracticeRobot dpmRobot;
    private SimUI simUI;
    private MainState mainState;

    GameObject canvas;

    GameObject dpmWindow;
    GameObject configWindow;
    GameObject defineIntakeWindow;
    GameObject defineReleaseWindow;
    GameObject setSpawnWindow;
    GameObject defineGamepieceWindow;

    GameObject releaseVelocityPanel;

    GameObject xOffsetEntry;
    GameObject yOffsetEntry;
    GameObject zOffsetEntry;
    GameObject releaseSpeedEntry;
    GameObject releaseVerticalEntry;
    GameObject releaseHorizontalEntry;

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

    GameObject lockPanel;

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

    private void Awake()
    {
        InitializeTrajectories();
        StateMachine.Instance.Link<MainState>(this);
    }

    private void Update()
    {
        if (mainState == null)
        {
            mainState = StateMachine.Instance.FindState<MainState>();
        }
        else if (dpmRobot == null)
        {
            dpmRobot = mainState.ActiveRobot.GetDriverPractice();
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

    void FindElements()
    {
        canvas = GameObject.Find("Canvas");
        simUI = GetComponent<SimUI>();

        dpmWindow = AuxFunctions.FindObject(canvas, "DPMPanel");
        configWindow = AuxFunctions.FindObject(canvas, "ConfigurationPanel");

        //enableDPMText = AuxFunctions.FindObject(canvas, "EnableDPMText").GetComponent<Text>();

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

        primaryCountText = AuxFunctions.FindObject(canvas, "PrimaryCountText").GetComponent<Text>();
        secondaryCountText = AuxFunctions.FindObject(canvas, "SecondaryCountText").GetComponent<Text>();
    }

    /// <summary>
    /// Updates the UI elements in the driver practice mode toolbars to reflect changes in configurable values
    /// </summary>
    private void UpdateDPMValues()
    {
        if (dpmRobot.gamepieceNames[0] == null) primaryGamepieceText.text = "Primary Gamepiece:  NOT CONFIGURED";
        else primaryGamepieceText.text = "Primary Gamepiece:  " + dpmRobot.gamepieceNames[0];

        if (dpmRobot.gamepieceNames[1] == null) secondaryGamepieceText.text = "Secondary Gamepiece:  NOT CONFIGURED";
        else secondaryGamepieceText.text = "Secondary Gamepiece:  " + dpmRobot.gamepieceNames[1];

        primaryCountText.text = "Spawned: " + dpmRobot.spawnedGamepieces[0].Count + "\nHeld: " + dpmRobot.objectsHeld[0].Count;
        secondaryCountText.text = "Spawned: " + dpmRobot.spawnedGamepieces[1].Count + "\nHeld: " + dpmRobot.objectsHeld[1].Count;

        if (configuring)
        {
            if (dpmRobot.gamepieceNames[configuringIndex] == null) configuringText.text = "Gamepiece not defined yet!";
            else configuringText.text = "Configuring:  " + dpmRobot.gamepieceNames[configuringIndex];

            releaseMechanismText.text = "Current Part:  " + dpmRobot.releaseNode[configuringIndex].name;
            intakeMechanismText.text = "Current Part:  " + dpmRobot.intakeNode[configuringIndex].name;

            if (configuringIndex == 0)
            {
                intakeControlText.text = Controls.buttons[0].pickupPrimary.primaryInput.ToString();
                releaseControlText.text = Controls.buttons[0].releasePrimary.primaryInput.ToString();
                spawnControlText.text = Controls.buttons[0].spawnPrimary.primaryInput.ToString();
            }
            else
            {
                intakeControlText.text = Controls.buttons[0].pickupSecondary.primaryInput.ToString();
                releaseControlText.text = Controls.buttons[0].releaseSecondary.primaryInput.ToString();
                spawnControlText.text = Controls.buttons[0].spawnSecondary.primaryInput.ToString();
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
                if (holdCount < holdThreshold) dpmRobot.ChangeOffsetX(deltaOffsetX, configuringIndex);
                else dpmRobot.ChangeOffsetX(deltaOffsetX * 5, configuringIndex);
            }
            else if (deltaOffsetY != 0)
            {
                if (holdCount < holdThreshold) dpmRobot.ChangeOffsetY(deltaOffsetY, configuringIndex);
                else dpmRobot.ChangeOffsetY(deltaOffsetY * 5, configuringIndex);
            }
            else if (deltaOffsetZ != 0)
            {
                if (holdCount < holdThreshold) dpmRobot.ChangeOffsetZ(deltaOffsetZ, configuringIndex);
                else dpmRobot.ChangeOffsetZ(deltaOffsetZ * 5, configuringIndex);
            }
            else if (deltaReleaseSpeed != 0)
            {
                if (holdCount < holdThreshold) dpmRobot.ChangeReleaseSpeed(deltaReleaseSpeed, configuringIndex);
                else dpmRobot.ChangeReleaseSpeed(deltaReleaseSpeed * 5, configuringIndex);
            }
            else if (deltaReleaseHorizontal != 0)
            {
                if (holdCount < holdThreshold) dpmRobot.ChangeReleaseHorizontalAngle(deltaReleaseHorizontal, configuringIndex);
                else dpmRobot.ChangeReleaseHorizontalAngle(deltaReleaseHorizontal * 5, configuringIndex);
            }
            else if (deltaReleaseVertical != 0)
            {
                if (holdCount < holdThreshold) dpmRobot.ChangeReleaseVerticalAngle(deltaReleaseVertical, configuringIndex);
                else dpmRobot.ChangeReleaseVerticalAngle(deltaReleaseVertical * 5, configuringIndex);
            }
            holdCount++;
        }

        if (!isEditing)
        {

            xOffsetEntry.GetComponent<InputField>().text = dpmRobot.positionOffset[configuringIndex][0].ToString();
            yOffsetEntry.GetComponent<InputField>().text = dpmRobot.positionOffset[configuringIndex][1].ToString();
            zOffsetEntry.GetComponent<InputField>().text = dpmRobot.positionOffset[configuringIndex][2].ToString();

            releaseSpeedEntry.GetComponent<InputField>().text = dpmRobot.releaseVelocity[configuringIndex][0].ToString();
            releaseHorizontalEntry.GetComponent<InputField>().text = dpmRobot.releaseVelocity[configuringIndex][1].ToString();
            releaseVerticalEntry.GetComponent<InputField>().text = dpmRobot.releaseVelocity[configuringIndex][2].ToString();
        }


    }

    private void UpdateWindows()
    {
        if (configuring)
        {
            if (dpmRobot.addingGamepiece)
            {
                configWindow.SetActive(false);
                dpmWindow.SetActive(false);
                defineGamepieceWindow.SetActive(true);
            }
            else if (dpmRobot.settingSpawn != 0)
            {
                configWindow.SetActive(false);
                dpmWindow.SetActive(false);
                setSpawnWindow.SetActive(true);
            }
            else if (dpmRobot.definingIntake)
            {
                configWindow.SetActive(false);
                dpmWindow.SetActive(false);
                defineIntakeWindow.SetActive(true);
            }
            else if (dpmRobot.definingRelease)
            {
                configWindow.SetActive(false);
                dpmWindow.SetActive(false);
                defineReleaseWindow.SetActive(true);
            }
            else
            {
                dpmWindowOn = true;
                defineGamepieceWindow.SetActive(false);
                setSpawnWindow.SetActive(false);
                defineIntakeWindow.SetActive(false);
                defineReleaseWindow.SetActive(false);
                dpmWindow.SetActive(true);
                configWindow.SetActive(true);
            }
        }
    }

    /// <summary>
    /// Toggles the Driver Practice Mode window
    /// </summary>
    public void DPMToggleWindow()
    {
        if (dpmWindowOn)
        {
            dpmWindowOn = false;

        }
        else
        {
            simUI.EndOtherProcesses();
            dpmWindowOn = true;
        }
        dpmWindow.SetActive(dpmWindowOn);
    }

    /// <summary>
    /// Clears all the gamepieces sharing the same name as the ones that have been configured from the field.
    /// </summary>
    public void ClearGamepieces()
    {
        dpmRobot.ClearGamepieces();
    }

    /// <summary>
    /// Toggles the display of primary gamepiece release trajectory.
    /// </summary>
    public void DisplayTrajectoryPrimary()
    {
        dpmRobot.displayTrajectories[0] = !dpmRobot.displayTrajectories[0];
    }

    /// <summary>
    /// Toggles the display of primary gamepiece release trajectory.
    /// </summary>
    public void DisplayTrajectorySecondary()
    {
        dpmRobot.displayTrajectories[1] = !dpmRobot.displayTrajectories[1];
    }

    /// <summary>
    /// Opens the configuration window and sets it up for the primary gamepiece
    /// </summary>
    public void DPMConfigurePrimary()
    {
        if (dpmRobot.modeEnabled)
        {
            StartNewProcess(); ;
            configuring = true;
            configuringIndex = 0;
            configHeaderText.text = "Configuration Menu - Primary Gamepiece";
            configWindow.SetActive(true);
            dpmRobot.displayTrajectories[0] = true;
            dpmRobot.displayTrajectories[1] = false;
        }
        else UserMessageManager.Dispatch("You must enable Driver Practice Mode first!", 5);
    }

    /// <summary>
    /// Opens the configuration window and sets it up for the secondary gamepiece
    /// </summary>
    public void DPMConfigureSecondary()
    {
        if (dpmRobot.modeEnabled)
        {
            StartNewProcess();
            configuring = true;
            configuringIndex = 1;
            configHeaderText.text = "Configuration Menu - Secondary Gamepiece";
            configWindow.SetActive(true);
            dpmRobot.displayTrajectories[0] = false;
            dpmRobot.displayTrajectories[1] = true;
        }
        else UserMessageManager.Dispatch("You must enable Driver Practice Mode first!", 5);
    }

    /// <summary>
    /// Spawns the primary gamepiece at its defined spawn location, or at the field's origin if one hasn't been defined
    /// </summary>
    public void SpawnGamepiecePrimary()
    {
        dpmRobot.SpawnGamepiece(0);
    }

    /// <summary>
    /// Spawns the secondary gamepiece at its defined spawn location, or at the field's origon if one hasn't been defined.
    /// </summary>
    public void SpawnGamepieceSecondary()
    {
        dpmRobot.SpawnGamepiece(1);
    }

    #region dpm configuration button functions
    public void CloseConfigurationWindow()
    {
        configWindow.SetActive(false);
        configuring = false;
        dpmRobot.displayTrajectories[configuringIndex] = false;
        dpmRobot.Save();
    }

    public void DefineGamepiece()
    {
        StartNewProcess();
        dpmRobot.DefineGamepiece(configuringIndex);
    }

    public void CancelDefineGamepiece()
    {
        dpmRobot.addingGamepiece = false;
    }

    public void DefineIntake()
    {
        StartNewProcess();
        dpmRobot.DefineIntake(configuringIndex);
    }

    public void CancelDefineIntake()
    {
        dpmRobot.definingIntake = false;
    }

    public void HighlightIntake()
    {
        dpmRobot.HighlightNode(dpmRobot.intakeNode[configuringIndex]);
    }

    public void DefineRelease()
    {
        StartNewProcess();
        dpmRobot.DefineRelease(configuringIndex);
    }

    public void CancelDefineRelease()
    {
        dpmRobot.definingRelease = false;
    }

    public void HighlightRelease()
    {
        dpmRobot.HighlightNode(dpmRobot.releaseNode[configuringIndex]);
    }

    public void SetGamepieceSpawn()
    {
        StartNewProcess();
        dpmRobot.StartGamepieceSpawn(configuringIndex);
    }

    public void CancelGamepieceSpawn()
    {
        dpmRobot.FinishGamepieceSpawn();
    }



    public void ChangeOffsetX(int sign)
    {
        deltaOffsetX = offsetIncrement * sign;
        holdCount++;
    }

    public void ChangeOffsetY(int sign)
    {
        deltaOffsetY = offsetIncrement * sign;
        holdCount++;
    }

    public void ChangeOffsetZ(int sign)
    {
        deltaOffsetZ = offsetIncrement * sign;
        holdCount++;
    }

    public void ChangeReleaseSpeed(int sign)
    {
        deltaReleaseSpeed = speedIncrement * sign;
        holdCount++;
    }

    public void ChangeHorizontalAngle(int sign)
    {
        deltaReleaseHorizontal = angleIncrement * sign;
        holdCount++;
    }

    public void ChangeVerticalAngle(int sign)
    {
        deltaReleaseVertical = angleIncrement * sign;
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
                dpmRobot.positionOffset[configuringIndex][0] = temp;
            temp = 0;
            if (float.TryParse(yOffsetEntry.GetComponent<InputField>().text, out temp))
                dpmRobot.positionOffset[configuringIndex][1] = temp;
            temp = 0;
            if (float.TryParse(zOffsetEntry.GetComponent<InputField>().text, out temp))
                dpmRobot.positionOffset[configuringIndex][2] = temp;
            temp = 0;
            if (float.TryParse(releaseSpeedEntry.GetComponent<InputField>().text, out temp))
                dpmRobot.releaseVelocity[configuringIndex][0] = temp;
            temp = 0;
            if (float.TryParse(releaseHorizontalEntry.GetComponent<InputField>().text, out temp))
                dpmRobot.releaseVelocity[configuringIndex][1] = temp;
            temp = 0;
            if (float.TryParse(releaseVerticalEntry.GetComponent<InputField>().text, out temp))
                dpmRobot.releaseVelocity[configuringIndex][2] = temp;
        }
    }
    #endregion
    #region control customization functions

    public void SetNewControl(int index)
    {
        settingControl = index;
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
                int index = mainState.ActiveRobot.ControlIndex;
                if (configuringIndex == 0)
                {
                    if (settingControl == 1)
                    {
                        Controls.buttons[index].pickupPrimary.primaryInput = Controls.customInputFromString(vKey.ToString());
                    }
                    else if (settingControl == 2) Controls.buttons[index].releasePrimary.primaryInput = Controls.customInputFromString(vKey.ToString());
                    else Controls.buttons[index].spawnPrimary.primaryInput = Controls.customInputFromString(vKey.ToString());
                }
                else
                {
                    if (settingControl == 1) Controls.buttons[index].pickupSecondary.primaryInput = Controls.customInputFromString(vKey.ToString());
                    else if (settingControl == 2) Controls.buttons[index].releaseSecondary.primaryInput = Controls.customInputFromString(vKey.ToString());
                    else Controls.buttons[index].spawnSecondary.primaryInput = Controls.customInputFromString(vKey.ToString());
                }
                settingControl = 0;
                Controls.Save();
            }
        }
    }
    #endregion

    public void EndProcesses()
    {
        dpmWindowOn = false;
        dpmWindow.SetActive(false);
        if (configuring)
        {
            CloseConfigurationWindow();
            CancelDefineGamepiece();
            CancelDefineIntake();
            CancelDefineRelease();
            CancelGamepieceSpawn();
        }
    }

    private void StartNewProcess()
    {
        if (configuring)
        {
            simUI.EndOtherProcesses();
            configuring = true;
        }
    }

    public void ChangeActiveRobot(int index)
    {

        DriverPracticeRobot newRobot = mainState.SpawnedRobots[index].GetComponent<DriverPracticeRobot>();
        dpmRobot.displayTrajectories[0] = false;
        dpmRobot.displayTrajectories[1] = false;

        UpdateDPMValues();
        dpmRobot = newRobot;

    }

    private void InitializeTrajectories()
    {
        LineRenderer[] drawnTrajectory = new LineRenderer[2];

        GameObject trajectory1 = new GameObject("DrawnTrajectory1");
        GameObject trajectory2 = new GameObject("DrawnTrajectory2");

        StateMachine.Instance.Link<MainState>(trajectory1);
        StateMachine.Instance.Link<MainState>(trajectory2);

        drawnTrajectory[0] = trajectory1.AddComponent<LineRenderer>();
        drawnTrajectory[1] = trajectory2.AddComponent<LineRenderer>();
        foreach (LineRenderer line in drawnTrajectory)
        {
            line.startWidth = 0.2f;
            line.material = Resources.Load("Materials/Projection") as Material;
            line.enabled = false;
        }
        drawnTrajectory[0].startColor = Color.blue;
        drawnTrajectory[0].endColor = Color.cyan;
        drawnTrajectory[1].startColor = Color.red;
        drawnTrajectory[1].endColor = Color.magenta;
    }
}