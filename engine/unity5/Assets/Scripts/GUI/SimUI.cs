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

    Text configHeaderText;
    Text configuringText;

    Text intakeMechanismText;
    Text releaseMechanismText;

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

    bool isEditing = false;



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
            main = transform.GetComponent<StateMachine>().GetMainState();
            camera = GameObject.Find("Main Camera").GetComponent<DynamicCamera>();
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

        if (configuring)
        {
            if (dpm.gamepieceNames[configuringIndex] == null) configuringText.text = "Gamepiece not defined yet!";
            else configuringText.text = "Configuring:  " + dpm.gamepieceNames[configuringIndex];

            releaseMechanismText.text = "Current Part:  " + dpm.releaseNode[configuringIndex].name;
            intakeMechanismText.text = "Current Part:  " + dpm.intakeNode[configuringIndex].name;
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

    #region main button functions
    /// <summary>
    /// Resets the robot
    /// </summary>
    public void PressReset()
    {
        main.ResetRobot();
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

    //Orient Robot Functions
    public void OrientStart()
    {
        main.StartOrient();
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

    public void OrientSave()
    {
        main.SaveOrientation();
    }

    public void OrientEnd()
    {
        //To be filled in later when UI work has been done
    }

    public void OrientDefault()
    {
        main.ResetOrientation();
    }

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
            enableDPMText.text = "Disable";
        }
        else
        {
            if (configuring) UserMessageManager.Dispatch("You must close the configuration window first!", 5);
            else
            {
                enableDPMText.text = "Enable";
                dpm.modeEnabled = false;
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
    /// Toggles the display of gamepiece release trajectories.
    /// </summary>
    public void DisplayTrajectories()
    {
        dpm.displayTrajectories = !dpm.displayTrajectories;
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
        }
        else UserMessageManager.Dispatch("You must enable Driver Practice Mode first!",5);
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
    }

    public void DefineGamepiece()
    {
        dpm.DefineGamepiece(configuringIndex);
    }

    public void DefineIntake()
    {
        dpm.DefineIntake(configuringIndex);
    }

    public void HighlightIntake()
    {
        dpm.HighlightNode(dpm.intakeNode[configuringIndex].name);
    }

    public void DefineRelease()
    {
        dpm.DefineRelease(configuringIndex);
    }

    public void HighlightRelease()
    {
        dpm.HighlightNode(dpm.releaseNode[configuringIndex].name);
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

}

