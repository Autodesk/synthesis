using Assets.Scripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Analytics;

public class MixAndMatchMode : MonoBehaviour
{
#region Variables
    private GameObject mixAndMatchMode;
    private GameObject mixAndMatchModeScript;
    private GameObject infoText;
    private GameObject mecWheelPanel;

    //Presets
    private GameObject presetsPanel;
    [HideInInspector] public List<GameObject> PresetClones = new List<GameObject>();
    [HideInInspector] public List<MaMPreset> PresetsList = new List<MaMPreset>();
    private GameObject setPresetPanel;
    private GameObject inputField;
    private GameObject deletePresetButton;


    //Wheel options
    private GameObject tractionWheel;
    private GameObject colsonWheel;
    private GameObject omniWheel;
    private GameObject pneumaticWheel;
    [HideInInspector] public List<GameObject> Wheels;
    public static int SelectedWheel; //This is public static so that it can be accessed by RNMesh


    //Drive Base options
    private GameObject defaultDrive;
    private GameObject mecanumDrive;
    private GameObject swerveDrive;
    private GameObject narrowDrive;
    [HideInInspector] public List<GameObject> Bases;
    int selectedDriveBase;
    public static bool IsMecanum = false;

    //Manipulator Options
    private GameObject noManipulator;
    private GameObject syntheClaw;
    private GameObject syntheShot;
    private GameObject lift;
    [HideInInspector] public List<GameObject> Manipulators;
    int selectedManipulator;
    public static bool HasManipulator = true;

    //Scroll buttons
    private GameObject wheelRightScroll;
    private GameObject wheelLeftScroll;
    private GameObject driveBaseRightScroll;
    private GameObject driveBaseLeftScroll;
    private GameObject manipulatorRightScroll;
    private GameObject manipulatorLeftScroll;
    private GameObject presetRightScroll;
    private GameObject presetLeftScroll;
    #endregion
    // Use this for initialization
    private void Awake()
    {
        
    }
    void Start()
    {
        FindAllGameObjects();
        StartMixAndMatch();

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void FindAllGameObjects()
    {
        mixAndMatchMode = GameObject.Find("MixAndMatchMode");
        mixAndMatchModeScript = GameObject.Find("MixAndMatchModeScript");
        infoText = GameObject.Find("PartDescription");
        Text txt = infoText.GetComponent<Text>();
        presetsPanel = GameObject.Find("PresetPanel");
        setPresetPanel = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("SetPresetPanel")).First();
        inputField = GameObject.Find("InputField");
        deletePresetButton = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("DeleteButton")).First();
        mecWheelPanel = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("MecWheelLabel")).First();

        //Find wheel objects
        tractionWheel = GameObject.Find("TractionWheel");
        colsonWheel = GameObject.Find("ColsonWheel");
        omniWheel = GameObject.Find("OmniWheel");
        pneumaticWheel = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("PneumaticWheel")).First();
        //Put all the wheels in the wheels list
        Wheels = new List<GameObject> { tractionWheel, colsonWheel, omniWheel, pneumaticWheel };


        //Find drive base objects
        defaultDrive = GameObject.Find("DefaultBase");
        mecanumDrive = GameObject.Find("MecanumBase");
        swerveDrive = GameObject.Find("SwerveBase");
        narrowDrive = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("NarrowBase")).First();
        //Put all the drive bases in the bases list
        Bases = new List<GameObject> { defaultDrive, mecanumDrive, swerveDrive, narrowDrive };

        //Find manipulator objects
        noManipulator = GameObject.Find("NoManipulator");
        syntheClaw = GameObject.Find("SyntheClaw");
        syntheShot = GameObject.Find("SyntheShot");
        lift = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("Lift")).First();
        //Put all the manipulators in the manipulators list
        Manipulators = new List<GameObject> { noManipulator, syntheClaw, syntheShot, lift };

        //Find all the scroll buttons
        wheelRightScroll = GameObject.Find("WheelRightScroll");
        wheelLeftScroll = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("WheelLeftScroll")).First(); 
        driveBaseRightScroll = GameObject.Find("BaseRightScroll");
        driveBaseLeftScroll = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("BaseLeftScroll")).First(); ;
        manipulatorRightScroll = GameObject.Find("ManipulatorRightScroll");
        manipulatorLeftScroll = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("ManipulatorLeftScroll")).First();
        presetRightScroll = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("PresetRightScroll")).First();
        presetLeftScroll = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("PresetLeftScroll")).First();

        if (this.gameObject.name == "MixAndMatchModeScript")
        {
            this.gameObject.GetComponent<MaMScroller>().FindAllGameObjects();
            this.gameObject.GetComponent<MaMInfoText>().FindAllGameObjects();
        }
    }

    /// <summary>
    /// Called when the Mix and Match Configuration tab is opened from the main menu. 
    /// </summary>
    public void StartMixAndMatch()
    {
        if (this.gameObject.name == "MixAndMatchModeScript")
        {
            wheelLeftScroll.SetActive(false);
            driveBaseLeftScroll.SetActive(false);
            manipulatorLeftScroll.SetActive(false);
            presetLeftScroll.SetActive(false);
            presetRightScroll.SetActive(false);

            mecWheelPanel.SetActive(false);

            setPresetPanel.SetActive(false);

            deletePresetButton.SetActive(false);
            //XMLManager.ins.itemDB.xmlList.Clear();

            String presetFile = Application.persistentDataPath + "/item_data.xml";
            if (File.Exists(presetFile))
            {
                XMLManager.ins.LoadItems();
                LoadPresets();
            }

            if (XMLManager.ins.itemDB.xmlList.Count() > 3) presetRightScroll.SetActive(true);

            SelectWheel(0);
            SelectDriveBase(0);
            SelectManipulator(0);

            this.gameObject.GetComponent<MaMScroller>().ResetFirsts();
            this.gameObject.GetComponent<MaMScroller>().FindAllGameObjects();
            this.gameObject.GetComponent<MaMInfoText>().FindAllGameObjects();
            // Sets info panel to blank
            Text txt = infoText.GetComponent<Text>();
            txt.text = "";



            if (SimUI.changeAnalytics) //for analytics tracking
            {
                Analytics.CustomEvent("Opened Mix and Match", new Dictionary<string, object>
                {
                });
            }
        }
    }

    /// <summary>
    /// Sets the destination paths of the selected field, robot base and manipulator to be used by MainState. Starts the simulation in Quick Swap Mode. 
    /// </summary>
    public void StartMaMSim()
    {
        RobotTypeManager.IsMixAndMatch = true;
        RobotTypeManager.RobotPath = mixAndMatchModeScript.GetComponent<MaMGetters>().GetDriveBase(selectedDriveBase);
        RobotTypeManager.ManipulatorPath = mixAndMatchModeScript.GetComponent<MaMGetters>().GetManipulator(selectedManipulator);
        RobotTypeManager.WheelPath = mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheel(SelectedWheel);

        RobotTypeManager.SetWheelProperties(mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelMass(SelectedWheel), 
            mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelRadius(SelectedWheel),
            mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelFriction(SelectedWheel),
            mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelLateralFriction(SelectedWheel));
        PlayerPrefs.SetString("simSelectedReplay", string.Empty);
        SceneManager.LoadScene("Scene");

        if (SimUI.changeAnalytics) //for analytics tracking
        {
            Analytics.CustomEvent("Started Mix and Match", new Dictionary<string, object>
            {
            });
        }
    }

    #region Change or Add MaM Robot
    /// <summary>
    /// Called when the "next" button on the MaM panel is clicked within the simulator. 
    /// Determines if the user wants to change the active robot or add a robot for local multiplayer and calls the correct function.
    /// </summary>
    bool changeMaMRobot = true;
    public void ChangeOrAddMaMRobot()
    {
        if (changeMaMRobot)
        {
            ChangeMaMRobot();
        } else
        {
            AddMaMRobot();
        }
    }

    public void ChangeMaMClicked()
    {
        changeMaMRobot = true;
    }

    public void AddMaMClicked()
    {
        changeMaMRobot = false;
    }

    /// <summary>
    /// When the user changes wheels/drive bases/manipulators within the simulator, changes the robot.
    /// </summary>
    void ChangeMaMRobot()
    { 
        string baseDirectory = mixAndMatchModeScript.GetComponent<MaMGetters>().GetDriveBase(selectedDriveBase);
        string manipulatorDirectory = mixAndMatchModeScript.GetComponent<MaMGetters>().GetManipulator(selectedManipulator);

        PlayerPrefs.SetString("simSelectedReplay", string.Empty);

        RobotTypeManager.IsMixAndMatch = true;
        RobotTypeManager.RobotPath = baseDirectory;
        if(selectedManipulator == 0)
        {
            RobotTypeManager.HasManipulator = false;
        } else
        {
            RobotTypeManager.HasManipulator = true;
            RobotTypeManager.ManipulatorPath = manipulatorDirectory;
        }

        RobotTypeManager.WheelPath = mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheel(SelectedWheel);
        RobotTypeManager.SetWheelProperties(mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelMass(SelectedWheel),
            mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelRadius(SelectedWheel),
            mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelFriction(SelectedWheel),
            mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelLateralFriction(SelectedWheel));

        GameObject stateMachine = GameObject.Find("StateMachine");

        stateMachine.GetComponent<SimUI>().MaMChangeRobot(baseDirectory, manipulatorDirectory);

        if (SimUI.changeAnalytics) //For analytics tracking
        {
            Analytics.CustomEvent("Changed Mix and Match Robot", new Dictionary<string, object> 
            {
            });
        }
    }

    /// <summary>
    /// When the user adds a MaMRobot in  multiplayer mode, sets the player prefs to file paths of robot parts
    /// </summary>
    void AddMaMRobot()
    {
        string baseDirectory = mixAndMatchModeScript.GetComponent<MaMGetters>().GetDriveBase(selectedDriveBase);
        string manipulatorDirectory = mixAndMatchModeScript.GetComponent<MaMGetters>().GetManipulator(selectedManipulator);

        RobotTypeManager.IsMixAndMatch = true;
        RobotTypeManager.RobotPath = mixAndMatchModeScript.GetComponent<MaMGetters>().GetDriveBase(selectedDriveBase);
        if (selectedManipulator == 0)
        {
            RobotTypeManager.HasManipulator = false;
        }
        else
        {
            RobotTypeManager.HasManipulator = true;
            RobotTypeManager.ManipulatorPath = manipulatorDirectory;
        }
        RobotTypeManager.WheelPath = mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheel(SelectedWheel);

        RobotTypeManager.SetWheelProperties(mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelMass(SelectedWheel),
            mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelRadius(SelectedWheel),
            mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelFriction(SelectedWheel),
            mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelLateralFriction(SelectedWheel));


        PlayerPrefs.SetString("simSelectedReplay", string.Empty);
        GameObject stateMachine = GameObject.Find("StateMachine");

        stateMachine.GetComponent<LocalMultiplayer>().AddMaMRobot(baseDirectory, manipulatorDirectory, RobotTypeManager.HasManipulator);
    }
#endregion
   
    #region Presets

    /// <summary>
    /// When the user enters the name for a preset, creates a MaMPreset object with the name and selected parts and adds it to the list.
    /// Also creates a GameObject clone of the preset prefab and adds it to the presetClones list.
    /// </summary>
    public void SetPresetName()
    {
        String name = "";
        if (inputField.GetComponent<InputField>().text.Length > 0)
        {
            name = inputField.GetComponent<InputField>().text;
        } else
        {
            UserMessageManager.Dispatch("Please enter a name", 5);
            ToggleSetPresetPanel();
            return;
        }

        foreach (MaMPreset preset in XMLManager.ins.itemDB.xmlList)
        {
            if (name == preset.GetName())
            {
                UserMessageManager.Dispatch("Please choose a new preset name", 5);
                ToggleSetPresetPanel();
                return;
            }
        }
        XMLManager.ins.itemDB.xmlList.Add(new MaMPreset(SelectedWheel, selectedDriveBase, selectedManipulator, name));

        XMLManager.ins.SaveItems();

        int clonePosition = (PresetClones.Count < 3) ? PresetClones.Count : 0;
        GameObject clone = presetsPanel.GetComponent<MaMPresetMono>().CreateClone(XMLManager.ins.itemDB.xmlList[PresetClones.Count], clonePosition);
        clone.GetComponent<Text>().text = XMLManager.ins.itemDB.xmlList[PresetClones.Count].GetName();
        SetPresetFontSize(clone);

        PresetClones.Add(clone);
        if (PresetClones.Count > 3)
        {
            clone.SetActive(false);
            presetRightScroll.SetActive(true);
        }

        //Creates a listener for OnClick
        int value = PresetClones.Count - 1;
        Button buttonCtrl = clone.GetComponent<Button>();
        buttonCtrl.onClick.AddListener(() => SelectPresets(value));

        Text txt = infoText.GetComponent<Text>();
        txt.text = XMLManager.ins.itemDB.xmlList[PresetClones.Count - 1].GetName();

        if (SimUI.changeAnalytics) //for analytics tracking
        {
            Analytics.CustomEvent("Created Mix and Match Preset", new Dictionary<string, object>
            {
            });
        }

        inputField.GetComponent<InputField>().text = "";
    }

    void SetPresetFontSize(GameObject clone)
    {
        Debug.Log(clone.GetComponent<Text>().text+ clone.GetComponent<Text>().text.Length);
        if (clone.GetComponent<Text>().text.Length < 8)
        {
            clone.GetComponent<Text>().fontSize = 36; 
        } else if (clone.GetComponent<Text>().text.Length < 20)
        {
            clone.GetComponent<Text>().fontSize = 36 - 3 * (clone.GetComponent<Text>().text.Length - 8);
        } else
        {
            clone.GetComponent<Text>().fontSize = 16;
        }
    }

    int lastSelectedPreset;
    /// <summary>
    /// Called when a preset option is clicked. Selects the preset's wheel, drive base and manipulator.
    /// </summary>
    /// <param name="value"></param>
    public void SelectPresets(int value)
    {
        int wheel = XMLManager.ins.itemDB.xmlList[value].GetWheel();
        int driveBase = XMLManager.ins.itemDB.xmlList[value].GetDriveBase();
        int manipulator = XMLManager.ins.itemDB.xmlList[value].GetManipulator();
        SelectWheel(wheel);
        SelectDriveBase(driveBase);
        SelectManipulator(manipulator, "preset");

        Text txt = infoText.GetComponent<Text>();

        txt.text = XMLManager.ins.itemDB.xmlList[value].GetName() + "\n\n";

        SplitCamelCase(Wheels[wheel].name);
        SplitCamelCase(Bases[driveBase].name);
        SplitCamelCase(Manipulators[manipulator].name);

        lastSelectedPreset = value;

        deletePresetButton.SetActive(true);
    }

    /// <summary>
    /// Splits the variable names of the parts for a more readable info text when a preset is selected
    /// </summary>
    /// <param name="source"></param>
    /// <returns></returns>
    public void SplitCamelCase(string source)
    {
        String[] partName = Regex.Split(source, @"(?<!^)(?=[A-Z])");
        Text txt = infoText.GetComponent<Text>();
        foreach (var s in partName)
        {
            txt.text += s + " ";
        }
        txt.text += "\n";
    }

    public void DeletePreset()
    {
        XMLManager.ins.itemDB.xmlList.RemoveAt(lastSelectedPreset);
        XMLManager.ins.SaveItems();

        int count = PresetClones.Count;
        for (int i = 0; i < count; i++)
        {
            Destroy(PresetClones[0]);
            PresetClones.RemoveAt(0);
        }

        MaMScroller.firstPreset = 0;

        XMLManager.ins.LoadItems();
        LoadPresets();

        deletePresetButton.SetActive(false);
        presetLeftScroll.SetActive(false);
        presetRightScroll.SetActive(false);
        if (PresetClones.Count > 3) presetRightScroll.SetActive(true);
    }

    public void HidePresetButton()
    {
        deletePresetButton.SetActive(false);
    }

    /// <summary>
    /// Creates the GameObjects that are clones of the PresetPrefab and addes them to the presetClones list.
    /// Sets the text to the name of the corresponding MaMPreset Object.
    /// Shows the first 3 in the preset panel. 
    /// </summary>
    public void LoadPresets()
    {

        //Loads the first three presets
        for (int i = 0; i < 3 && i < XMLManager.ins.itemDB.xmlList.Count; i++)
        {
            GameObject clone = presetsPanel.GetComponent<MaMPresetMono>().CreateClone(XMLManager.ins.itemDB.xmlList[i], i);
            clone.GetComponent<Text>().text = XMLManager.ins.itemDB.xmlList[i].GetName();
            SetPresetFontSize(clone);
            PresetClones.Add(clone);

            //Creates a listner for OnClick
            int value = i;
            Button buttonCtrl = clone.GetComponent<Button>();
            buttonCtrl.onClick.AddListener(() => SelectPresets(value));
        }

        //Loads the rest of the presets and inactivates them
        for (int i = 3; i < XMLManager.ins.itemDB.xmlList.Count; i++)
        {
            GameObject clone = presetsPanel.GetComponent<MaMPresetMono>().CreateClone(XMLManager.ins.itemDB.xmlList[i], 0);
            clone.GetComponent<Text>().text = XMLManager.ins.itemDB.xmlList[i].GetName();
            SetPresetFontSize(clone);
            PresetClones.Add(clone);
            clone.SetActive(false);

            //Creates a listner for OnClick
            int value = i;
            Button buttonCtrl = clone.GetComponent<Button>();
            buttonCtrl.onClick.AddListener(() => SelectPresets(value));
        }

    }

    public static bool setPresetPanelOpen = false;
    public void ToggleSetPresetPanel()
    {
        setPresetPanel.SetActive(!setPresetPanel.activeSelf);
        setPresetPanelOpen = setPresetPanel.activeSelf;
    }

    #endregion

    #region Selecters
    /// <summary>
    /// Selects a wheel, as referenced by its index in the wheels list.
    /// </summary>
    /// <param name="wheel"></param>
    public void SelectWheel(int wheel)
    {
        Color purple = new Color(0.757f, 0.200f, 0.757f);

        //unselects all wheels
        for (int i = 0; i < Wheels.Count; i++)
        {
            SetColor(Wheels[i], Color.white);
        }

        //selects the wheel that is clicked
        SetColor(Wheels[wheel], purple);
        this.gameObject.GetComponent<MaMInfoText>().SetWheelInfoText(wheel);
        SelectedWheel = wheel;
    }

    /// <summary>
    /// Selects a drive base, as referenced by its index in the bases list
    /// </summary>
    /// <param name="driveBase"></param>
    public void SelectDriveBase(int driveBase)
    {
        Color purple = new Color(0.757f, 0.200f, 0.757f);

        //unselects all wheels
        for (int j = 0; j < Bases.Count; j++)
        {
            SetColor(Bases[j], Color.white);
        }

        //selects the wheel that is clicked
        SetColor(Bases[driveBase], purple);
        this.gameObject.GetComponent<MaMInfoText>().SetBaseInfoText(driveBase);
        selectedDriveBase = driveBase;
        mecWheelPanel = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("MecWheelLabel")).First();
        mecWheelPanel.SetActive(false);
        if (selectedDriveBase == 1)
        {
            IsMecanum = true;
            mecWheelPanel.SetActive(true);
        } 
    }

    public static bool GetMecanum()
    {
        return IsMecanum;
    }

    /// <summary>
    /// Selects a manipulator, as referenced by its index in the manipualtors list.
    /// </summary>
    /// <param name="manipulator"></param>
    public void SelectManipulator(int manipulator)
    {
        Color purple = new Color(0.757f, 0.200f, 0.757f);

        //unselects all manipulators
        for (int k = 0; k < Manipulators.Count; k++)
        {
            SetColor(Manipulators[k], Color.white);
        }

        //selects the manipulator that is clicked
        SetColor(Manipulators[manipulator], purple);
        this.gameObject.GetComponent<MaMInfoText>().SetManipulatorInfoText(manipulator);
        selectedManipulator = manipulator;
    }

    public void SelectManipulator(int manipulator, string presetName)
    {
        Color purple = new Color(0.757f, 0.200f, 0.757f);

        //unselects all manipulators
        for (int k = 0; k < Manipulators.Count; k++)
        {
            SetColor(Manipulators[k], Color.white);
        }

        //selects the manipulator that is clicked
        SetColor(Manipulators[manipulator], purple);
        HasManipulator = (manipulator == 0) ? false : true;
        selectedManipulator = manipulator;
        Text txt = infoText.GetComponent<Text>();
        txt.text = "";
    }

    /// <summary>
    /// Sets the color for selecting/unselecting parts.
    /// </summary>
    /// <param name="part"></param>
    /// <param name="color"></param>
    public void SetColor(GameObject part, Color color)
    {
        part.GetComponent<Image>().color = color;
    }
    #endregion
}