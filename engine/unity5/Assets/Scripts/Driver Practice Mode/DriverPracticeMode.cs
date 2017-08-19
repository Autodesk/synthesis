using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Assets.Scripts.FSM;
using UnityEditor;

public class DriverPracticeMode : MonoBehaviour {

    private DriverPracticeRobot dpmRobot;
    private Scoreboard scoreboard;
    private GameplayTimer timer;
    private SimUI simUI;
    private GoalDisplayManager goalDisplayManager;
    private MainState mainState;

    GameObject canvas;

    GameObject dpmWindow;
    GameObject scoreWindow;
    GameObject scoreLogWindow;
    GameObject timerWindow;
    GameObject configWindow;
    GameObject goalConfigWindow;
    GameObject defineIntakeWindow;
    GameObject defineReleaseWindow;
    GameObject setSpawnWindow;
    GameObject setGoalXZWindow;
    GameObject setGoalYWindow;
    GameObject defineGamepieceWindow;

    GameObject releaseVelocityPanel;

    GameObject xOffsetEntry;
    GameObject yOffsetEntry;
    GameObject zOffsetEntry;
    GameObject releaseSpeedEntry;
    GameObject releaseVerticalEntry;
    GameObject releaseHorizontalEntry;

    Image timerBackground;
    Image scoreBackground;
    /// <summary>
    /// These are the colors applied to the score and timer displays at various points of a game.
    /// scoreInactiveColor - The color of the score display when no game has started or ended, or when a game has been terminated
    /// timerStartColor    - The color of the timer display when a game starts.
    /// scoreStartColor    - The color of the score display when a game starts.
    /// timerEndColor      - The color of the timer display when a game has ended.
    /// scoreEndColor      - The color of the score display when a game has ended.
    /// </summary>
    public Color scoreInactiveColor = new Color(255 / 255f, 255 / 255f, 255 / 255f, 50 / 255f);
    public Color timerStartColor = new Color(0 / 255f, 235 / 255f, 0 / 255f, 127 / 255f);
    public Color scoreStartColor = new Color(0 / 255f, 255 / 255f, 0 / 255f, 127 / 255f);
    public Color timerEndColor = new Color(235 / 255f, 0 / 255f, 0 / 255f, 50 / 255f);
    public Color scoreEndColor = new Color(255 / 255f, 0 / 255f, 0 / 255f, 127 / 255f);

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
    public bool gameStarted = false; //if a game has started
    public bool gameEnded = false; //if the game started has ended
    public bool configuring = false; //if the configuration window is active
    public bool goalConfiguring = false; //if the goal configuration window is active
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

    private void Update()
    {
        if (mainState == null)
        {
            mainState = GetComponent<StateMachine>().CurrentState as MainState;
        }
        else if (dpmRobot == null)
        {
            dpmRobot = mainState.activeRobot.GetDriverPractice();
            FindElements();
        }
        else
        {
            UpdateDPMValues();
            UpdateVectorConfiguration();
            UpdateWindows();

            if (settingControl != 0) ListenControl();

            if (gameStarted && !timer.IsTimerRunning()) // Game is over. Update variables and display colors.
            {
                gameEnded = true;
                SetGameDisplayColors(timerEndColor, scoreEndColor);
            }
        }
    }

    void FindElements()
    {
        canvas = GameObject.Find("Canvas");
        scoreboard = GetComponent<Scoreboard>();
        timer = GetComponent<GameplayTimer>();
        simUI = GetComponent<SimUI>();
        goalDisplayManager = GetComponent<GoalDisplayManager>();

        dpmWindow = AuxFunctions.FindObject(canvas, "DPMPanel");
        scoreWindow = AuxFunctions.FindObject(canvas, "ScorePanel");
        scoreLogWindow = AuxFunctions.FindObject(canvas, "ScoreLogPanel");
        timerWindow = AuxFunctions.FindObject(canvas, "GameplayTimerPanel");
        configWindow = AuxFunctions.FindObject(canvas, "ConfigurationPanel");
        goalConfigWindow = AuxFunctions.FindObject(canvas, "GoalConfigPanel");
        
        timerBackground = AuxFunctions.FindObject(timerWindow, "TimerTextField").GetComponent<Image>();
        scoreBackground = AuxFunctions.FindObject(scoreWindow, "Score").GetComponent<Image>();

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
        setGoalXZWindow = AuxFunctions.FindObject(canvas, "SetGamepieceGoalXZPanel");
        setGoalYWindow = AuxFunctions.FindObject(canvas, "SetGamepieceGoalYPanel");

        intakeControlText = AuxFunctions.FindObject(canvas, "IntakeInputButton").GetComponentInChildren<Text>();
        releaseControlText = AuxFunctions.FindObject(canvas, "ReleaseInputButton").GetComponentInChildren<Text>();
        spawnControlText = AuxFunctions.FindObject(canvas, "SpawnInputButton").GetComponentInChildren<Text>();

        primaryCountText = AuxFunctions.FindObject(canvas, "PrimaryCountText").GetComponent<Text>();
        secondaryCountText = AuxFunctions.FindObject(canvas, "SecondaryCountText").GetComponent<Text>();

        lockPanel = AuxFunctions.FindObject(canvas, "DPMLockPanel");
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
                intakeControlText.text = InputControl.GetButton(Controls.buttons[0].pickupPrimary).ToString();
                releaseControlText.text = InputControl.GetButton(Controls.buttons[0].releasePrimary).ToString();
                spawnControlText.text = InputControl.GetButton(Controls.buttons[0].spawnPrimary).ToString();
            }
            else
            {
                intakeControlText.text = InputControl.GetButton(Controls.buttons[0].pickupSecondary).ToString();
                releaseControlText.text = InputControl.GetButton(Controls.buttons[0].releaseSecondary).ToString();
                spawnControlText.text = InputControl.GetButton(Controls.buttons[0].spawnSecondary).ToString();
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
                goalConfigWindow.SetActive(false);
                dpmWindow.SetActive(false);
                scoreWindow.SetActive(false);
                scoreLogWindow.SetActive(false);
                timerWindow.SetActive(false);
                defineGamepieceWindow.SetActive(true);
            }
            else if (dpmRobot.settingSpawn != 0)
            {
                configWindow.SetActive(false);
                goalConfigWindow.SetActive(false);
                dpmWindow.SetActive(false);
                scoreWindow.SetActive(false);
                scoreLogWindow.SetActive(false);
                timerWindow.SetActive(false);
                setSpawnWindow.SetActive(true);
            }
            else if (dpmRobot.settingGamepieceGoal != 0)
            {
                configWindow.SetActive(false);
                goalConfigWindow.SetActive(false);
                dpmWindow.SetActive(false);
                scoreWindow.SetActive(false);
                scoreLogWindow.SetActive(false);
                timerWindow.SetActive(false);
                if (!dpmRobot.settingGamepieceGoalVertical)
                {
                    setGoalXZWindow.SetActive(true);
                    setGoalYWindow.SetActive(false);
                }
                else
                {
                    setGoalXZWindow.SetActive(false);
                    setGoalYWindow.SetActive(true);
                }
            }
            else if (dpmRobot.definingIntake)
            {
                configWindow.SetActive(false);
                goalConfigWindow.SetActive(false);
                dpmWindow.SetActive(false);
                scoreWindow.SetActive(false);
                scoreLogWindow.SetActive(false);
                timerWindow.SetActive(false);
                defineIntakeWindow.SetActive(true);
            }
            else if (dpmRobot.definingRelease)
            {
                configWindow.SetActive(false);
                goalConfigWindow.SetActive(false);
                dpmWindow.SetActive(false);
                scoreWindow.SetActive(false);
                scoreLogWindow.SetActive(false);
                timerWindow.SetActive(false);
                defineReleaseWindow.SetActive(true);
            }
            else
            {
                defineGamepieceWindow.SetActive(false);
                setSpawnWindow.SetActive(false);
                setGoalXZWindow.SetActive(false);
                setGoalYWindow.SetActive(false);
                defineIntakeWindow.SetActive(false);
                defineReleaseWindow.SetActive(false);
                dpmWindow.SetActive(true);
                scoreWindow.SetActive(true);
                configWindow.SetActive(true);

                if (goalConfiguring)
                    goalConfigWindow.SetActive(true);

                if (gameStarted)
                    timerWindow.SetActive(true);
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
            if (!configuring && !goalConfiguring)
                dpmWindowOn = false;
            else UserMessageManager.Dispatch("You must close all configuration windows first!", 5);
        }
        else
        {
            simUI.EndOtherProcesses();
            dpmWindowOn = true;
        }

        dpmWindow.SetActive(dpmWindowOn);
    }

    /// <summary>
    /// Sets the driver practice mode to either be enabled or disabled, depending on what state it was at before.
    /// </summary>
    public void DPMToggle()
    {
        if (!dpmRobot.modeEnabled)
        {
            dpmRobot.modeEnabled = true;
            enableDPMText.text = "Disable Driver Practice Mode";
            lockPanel.SetActive(false);
            scoreWindow.SetActive(true);
            scoreboard.ResetScore();
        }
        else
        {
            if (goalConfiguring) UserMessageManager.Dispatch("You must close all configuration windows first!", 5);
            else if (configuring) UserMessageManager.Dispatch("You must close the configuration window first!", 5);
            else
            {
                enableDPMText.text = "Enable Driver Practice Mode";
                dpmRobot.displayTrajectories[0] = false;
                dpmRobot.displayTrajectories[1] = false;
                dpmRobot.modeEnabled = false;
                lockPanel.SetActive(true);
                scoreWindow.SetActive(false);
                scoreLogWindow.SetActive(false);
                StopGame();
            }

        }
    }

    /// <summary>
    /// Start a new game. Displays the timer and resets the score.
    /// </summary>
    public void StartGame()
    {
        if (dpmRobot.modeEnabled)
        {
            timerWindow.SetActive(true);
            timer.StartTimer();
            scoreboard.ResetScore();
            gameStarted = true;
            gameEnded = false;
            SetGameDisplayColors(timerStartColor, scoreStartColor);
        }
        else UserMessageManager.Dispatch("You must enable driver practice mode first.", 5);
    }

    /// <summary>
    /// Terminate an on-going game. Hides the timer and resets the score.
    /// </summary>
    public void StopGame()
    {
        if (gameStarted)
        {
            timer.StopTimer();
            timerWindow.SetActive(false);
            scoreboard.ResetScore();
            gameStarted = false;
            gameEnded = true;
            SetGameDisplayColors(timerEndColor, scoreInactiveColor);
        }
        else UserMessageManager.Dispatch("A game has not been started.", 5);
    }

    /// <summary>
    /// Set the color of the timer and score backgrounds to signify certain game states (ongoing, ended, etc).
    /// </summary>
    /// <param name="timerColor">Color of the timer background.</param>
    /// <param name="scoreColor">Color of the score background.</param>
    public void SetGameDisplayColors(Color timerColor, Color scoreColor)
    {
        if (timerBackground != null)
            timerBackground.color = timerColor;
        if (scoreBackground != null)
            scoreBackground.color = scoreColor;
    }

    /// <summary>
    /// Save the log of the current game to a text file.
    /// </summary>
    public void SaveGameStats()
    {
        if (dpmRobot.modeEnabled)
        {
            // This should be changed to defaut to file set in user preferences.
            string filePath = PlayerPrefs.GetString("simSelectedRobot") + "\\";
            string fileName = string.Format("score_log_{0:yyyy-MM-dd_hh-mm-ss-tt}.txt", System.DateTime.Now);

            scoreboard.Save(filePath, fileName);

            UserMessageManager.Dispatch("Saved to \"" + filePath + "\\" + fileName + "\"", 10);
        }
        else UserMessageManager.Dispatch("You must enable driver practice mode first.", 5);
    }

    /// <summary>
    /// Allow the user to select a new file to save game stats to.
    /// </summary>
    public void LoadSaveFile()
    {
        if (dpmRobot.modeEnabled)
        {
            string filePath;
            string fileName;

            string path = EditorUtility.OpenFilePanel("Overwrite with csv", "", "csv");

            // GET NEW FILE PATH AND NAME

            // SAVE FILE PATH AND NAME TO PREFERENCES
        }
        else UserMessageManager.Dispatch("You must enable driver practice mode first.", 5);
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
            simUI.EndOtherProcesses();
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
            simUI.EndOtherProcesses();
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
        CloseGamepieceGoalsConfig();
        configWindow.SetActive(false);
        configuring = false;
        dpmRobot.displayTrajectories[configuringIndex] = false;
        dpmRobot.Save();
    }

    public void DefineGamepiece()
    {
        simUI.EndOtherProcesses();
        dpmRobot.DefineGamepiece(configuringIndex);
    }

    public void CancelDefineGamepiece()
    {
        dpmRobot.addingGamepiece = false;
    }

    public void DefineIntake()
    {
        simUI.EndOtherProcesses();
        dpmRobot.DefineIntake(configuringIndex);
    }

    public void CancelDefineIntake()
    {
        dpmRobot.definingIntake = false;
    }

    public void HighlightIntake()
    {
        dpmRobot.HighlightNode(dpmRobot.intakeNode[configuringIndex].name);
    }

    public void DefineRelease()
    {
        simUI.EndOtherProcesses();
        dpmRobot.DefineRelease(configuringIndex);
    }

    public void CancelDefineRelease()
    {
        dpmRobot.definingRelease = false;
    }

    public void HighlightRelease()
    {
        dpmRobot.HighlightNode(dpmRobot.releaseNode[configuringIndex].name);
    }

    public void SetGamepieceSpawn()
    {
        simUI.EndOtherProcesses();
        dpmRobot.StartGamepieceSpawn(configuringIndex);
    }

    public void CancelGamepieceSpawn()
    {
        dpmRobot.FinishGamepieceSpawn();
    }

    public void OpenGamepieceGoalsConfig()
    {
        dpmRobot.InitGoalManagerDisplay(configuringIndex, goalDisplayManager);
        goalConfigWindow.SetActive(true);
        goalConfiguring = true;
    }

    public void CloseGamepieceGoalsConfig()
    {
        dpmRobot.FinishGamepieceGoal();
        goalConfigWindow.SetActive(false);
        goalConfiguring = false;
    }

    public void NewGamepieceGoal()
    {
        dpmRobot.NewGoal(configuringIndex);
        dpmRobot.InitGoalManagerDisplay(configuringIndex, goalDisplayManager);
    }

    public void DeleteGamepieceGoal(int goalIndex)
    {
        dpmRobot.DeleteGoal(configuringIndex, goalIndex);
        dpmRobot.InitGoalManagerDisplay(configuringIndex, goalDisplayManager);
    }

    public void SetGamepieceGoal(int goalIndex)
    {
        simUI.EndOtherProcesses();
        dpmRobot.StartGamepieceGoal(configuringIndex, goalIndex);
    }

    public void CancelGamepieceGoal()
    {
        dpmRobot.FinishGamepieceGoal();
    }

    public void SetGamepieceGoalDescription(int goalIndex, string description)
    {
        dpmRobot.SetGamepieceGoalDescription(configuringIndex, goalIndex, description);
    }

    public void SetGamepieceGoalPoints(int goalIndex, int points)
    {
        dpmRobot.SetGamepieceGoalPoints(configuringIndex, goalIndex, points);
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

        KeyMapping[] keys = GetComponentsInChildren<KeyMapping>();

        foreach (KeyMapping key in keys)
        {
            if (InputControl.GetButtonDown(key))
            {
                if (configuringIndex == 0)
                {
                    if (settingControl == 1)
                    {
                        InputControl.GetButton(Controls.buttons[0].pickupPrimary);
                    }
                    else if (settingControl == 2) InputControl.GetButton(Controls.buttons[0].pickupPrimary);
                    else InputControl.GetButton(Controls.buttons[0].spawnPrimary);
                }
                else
                {
                    if (settingControl == 1) InputControl.GetButton(Controls.buttons[0].pickupSecondary);
                    else if (settingControl == 2) InputControl.GetButton(Controls.buttons[0].releaseSecondary);
                    else InputControl.GetButton(Controls.buttons[0].spawnPrimary);
                }
                Controls.Save();
                settingControl = 0;
            }
        }
    }
    #endregion

    public void EndProcesses()
    {
        if (configuring)
        {
            CancelDefineGamepiece();
            CancelDefineIntake();
            CancelDefineRelease();
            CancelGamepieceSpawn();
            CancelGamepieceGoal();
            StopGame();
        }
    }

    public void ChangeActiveRobot(int index)
    {

        DriverPracticeRobot newRobot = mainState.SpawnedRobots[index].GetComponent<DriverPracticeRobot>();
        dpmRobot.displayTrajectories[0] = false;
        dpmRobot.displayTrajectories[1] = false;

        if (newRobot.modeEnabled)
        {
            enableDPMText.text = "Disable Driver Practice Mode";
            lockPanel.SetActive(false);
        }
        else
        { 
            enableDPMText.text = "Enable Driver Practice Mode";
            lockPanel.SetActive(true);
        }

        UpdateDPMValues();
        dpmRobot = newRobot;

    }
}