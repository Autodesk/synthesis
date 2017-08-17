using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MixAndMatchMode : MonoBehaviour
{
#region variables
    private GameObject mixAndMatchMode;
    private GameObject mixAndMatchModeScript;
    public static bool isMixAndMatchMode = false;
    private GameObject infoText;
    private GameObject mecWheelPanel;

    //Presets
    private GameObject presetsPanel;
    [HideInInspector] public List<GameObject> presetClones = new List<GameObject>();
    [HideInInspector] public List<MaMPreset> presetsList = new List<MaMPreset>();
    private GameObject setPresetPanel;
    private GameObject inputField;
    private GameObject deletePresetButton;


    //Wheel options
    private GameObject tractionWheel;
    private GameObject colsonWheel;
    private GameObject omniWheel;
    private GameObject pneumaticWheel;
    [HideInInspector] public List<GameObject> wheels;
    public static int selectedWheel; //This is public static so that it can be accessed by RNMesh


    //Drive Base options
    private GameObject defaultDrive;
    private GameObject mecanumDrive;
    private GameObject swerveDrive;
    private GameObject narrowDrive;
    [HideInInspector] public List<GameObject> bases;
    int selectedDriveBase;
    public static bool isMecanum = false;

    //Manipulator Options
    private GameObject noManipulator;
    private GameObject syntheClaw;
    private GameObject syntheShot;
    private GameObject lift;
    [HideInInspector] public List<GameObject> manipulators;
    int selectedManipulator;
    public static bool hasManipulator = true;

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
    void Start()
    {
        FindAllGameObjects();
        StartMixAndMatch();
        PlayerPrefs.SetInt("mixAndMatch", 1); //0 is false, 1 is true
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FindAllGameObjects()
    {
        mixAndMatchMode = GameObject.Find("MixAndMatchMode");
        mixAndMatchModeScript = GameObject.Find("MixAndMatchModeScript");
        infoText = GameObject.Find("PartDescription");
        Text txt = infoText.GetComponent<Text>();
        presetsPanel = GameObject.Find("PresetPanel");
        setPresetPanel = GameObject.Find("SetPresetPanel");
        inputField = GameObject.Find("InputField");
        deletePresetButton = GameObject.Find("DeleteButton");
        mecWheelPanel = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("MecWheelLabel")).First();

        //Find wheel objects
        tractionWheel = GameObject.Find("TractionWheel");
        colsonWheel = GameObject.Find("ColsonWheel");
        omniWheel = GameObject.Find("OmniWheel");
        pneumaticWheel = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("PneumaticWheel")).First();
        //Put all the wheels in the wheels list
        wheels = new List<GameObject> { tractionWheel, colsonWheel, omniWheel, pneumaticWheel };


        //Find drive base objects
        defaultDrive = GameObject.Find("DefaultBase");
        mecanumDrive = GameObject.Find("MecanumBase");
        swerveDrive = GameObject.Find("SwerveBase");
        narrowDrive = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("NarrowBase")).First();
        //Put all the drive bases in the bases list
        bases = new List<GameObject> { defaultDrive, mecanumDrive, swerveDrive, narrowDrive };

        //Find manipulator objects
        noManipulator = GameObject.Find("NoManipulator");
        syntheClaw = GameObject.Find("SyntheClaw");
        syntheShot = GameObject.Find("SyntheShot");
        lift = Resources.FindObjectsOfTypeAll<GameObject>().Where(x => x.name.Equals("Lift")).First();
        //Put all the manipulators in the manipulators list
        manipulators = new List<GameObject> { noManipulator, syntheClaw, syntheShot, lift };

        //Find all the scroll buttons
        wheelRightScroll = GameObject.Find("WheelRightScroll");
        wheelLeftScroll = GameObject.Find("WheelLeftScroll");
        driveBaseRightScroll = GameObject.Find("BaseRightScroll");
        driveBaseLeftScroll = GameObject.Find("BaseLeftScroll");
        manipulatorRightScroll = GameObject.Find("ManipulatorRightScroll");
        manipulatorLeftScroll = GameObject.Find("ManipulatorLeftScroll");
        presetRightScroll = GameObject.Find("PresetRightScroll");
        presetLeftScroll = GameObject.Find("PresetLeftScroll");
        
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
        }
    }

    /// <summary>
    /// Sets the destination paths of the selected field, robot base and manipulator to be used by MainState. Starts the simulation in Quick Swap Mode. 
    /// </summary>
    public void StartMaMSim()
    {
        PlayerPrefs.SetString("simSelectedRobot", mixAndMatchModeScript.GetComponent<MaMGetters>().GetDriveBase(selectedDriveBase));
        PlayerPrefs.SetString("simSelectedRobotName", "DriveBase2557");
        PlayerPrefs.SetString("simSelectedManipulator", mixAndMatchModeScript.GetComponent<MaMGetters>().GetManipulator(selectedManipulator));
        PlayerPrefs.SetString("simSelectedWheel", mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheel(selectedWheel));
        PlayerPrefs.SetFloat("wheelFriction", mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelFriction(selectedWheel));
        PlayerPrefs.SetFloat("wheelMass", mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelMass(selectedWheel));
        PlayerPrefs.Save();
        isMixAndMatchMode = true;
        SceneManager.LoadScene("mixAndMatch");
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
        int robotHasManipulator = PlayerPrefs.GetInt("hasManipulator"); //0 is false, 1 is true

        string baseDirectory = mixAndMatchModeScript.GetComponent<MaMGetters>().GetDriveBase(selectedDriveBase);
        string manipulatorDirectory = mixAndMatchModeScript.GetComponent<MaMGetters>().GetManipulator(selectedManipulator);

        PlayerPrefs.SetString("simSelectedReplay", string.Empty);
        PlayerPrefs.SetString("simSelectedRobot", baseDirectory);
        PlayerPrefs.SetString("simSelectedManipulator", manipulatorDirectory);
        PlayerPrefs.SetString("simSelectedWheel", mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheel(selectedWheel));
        PlayerPrefs.SetFloat("wheelFriction", mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelFriction(selectedWheel));
        PlayerPrefs.SetFloat("wheelMass", mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelMass(selectedWheel));

        GameObject stateMachine = GameObject.Find("StateMachine");

        stateMachine.GetComponent<SimUI>().MaMChangeRobot(baseDirectory, manipulatorDirectory, robotHasManipulator);
    }

    /// <summary>
    /// When the user adds a MaMRobot in  multiplayer mode, sets the player prefs to file paths of robot parts
    /// </summary>
    void AddMaMRobot()
    {
        string baseDirectory = mixAndMatchModeScript.GetComponent<MaMGetters>().GetDriveBase(selectedDriveBase);
        string manipulatorDirectory = mixAndMatchModeScript.GetComponent<MaMGetters>().GetManipulator(selectedManipulator);

        PlayerPrefs.SetString("simSelectedReplay", string.Empty);
        PlayerPrefs.SetString("simSelectedRobot", baseDirectory);
        PlayerPrefs.SetString("simSelectedManipulator", manipulatorDirectory);
        PlayerPrefs.SetString("simSelectedWheel", mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheel(selectedWheel));
        PlayerPrefs.SetFloat("wheelFriction", mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelFriction(selectedWheel));
        PlayerPrefs.SetFloat("wheelMass", mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelMass(selectedWheel));
        int robotHasManipulator = PlayerPrefs.GetInt("hasManipulator"); //0 is false, 1 is true
        GameObject stateMachine = GameObject.Find("StateMachine");

        stateMachine.GetComponent<LocalMultiplayer>().AddMaMRobot(baseDirectory, manipulatorDirectory, robotHasManipulator);
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
        if (inputField.GetComponent<InputField>().text.Length > 0) name = inputField.GetComponent<InputField>().text;
        XMLManager.ins.itemDB.xmlList.Add(new MaMPreset(selectedWheel, selectedDriveBase, selectedManipulator, name));

        XMLManager.ins.SaveItems();

        int clonePosition = (presetClones.Count < 3) ? presetClones.Count : 0;
        GameObject clone = presetsPanel.GetComponent<MaMPresetMono>().CreateClone(XMLManager.ins.itemDB.xmlList[presetClones.Count], clonePosition);
        clone.GetComponent<Text>().text = XMLManager.ins.itemDB.xmlList[presetClones.Count].GetName();
        presetClones.Add(clone);
        if (presetClones.Count > 3)
        {
            clone.SetActive(false);
            presetRightScroll.SetActive(true);
        }

        //Creates a listener for OnClick
        int value = presetClones.Count - 1;
        Button buttonCtrl = clone.GetComponent<Button>();
        buttonCtrl.onClick.AddListener(() => SelectPresets(value));

        Text txt = infoText.GetComponent<Text>();
        txt.text = XMLManager.ins.itemDB.xmlList[presetClones.Count - 1].GetName();

    }

    int lastSelectedPreset;
    /// <summary>
    /// Called when a preset option is clicked. Selects the preset's wheel, drive base and manipulator.
    /// </summary>
    /// <param name="value"></param>
    public void SelectPresets(int value)
    {
        SelectWheel(XMLManager.ins.itemDB.xmlList[value].GetWheel());
        SelectDriveBase(XMLManager.ins.itemDB.xmlList[value].GetDriveBase());
        SelectManipulator(XMLManager.ins.itemDB.xmlList[value].GetManipulator(), "preset");

        Text txt = infoText.GetComponent<Text>();
        txt.text = XMLManager.ins.itemDB.xmlList[value].GetName();

        lastSelectedPreset = value;

        deletePresetButton.SetActive(true);
    }

    public void DeletePreset()
    {
        XMLManager.ins.itemDB.xmlList.RemoveAt(lastSelectedPreset);
        XMLManager.ins.SaveItems();

        int count = presetClones.Count;
        for (int i = 0; i < count; i++)
        {
            Destroy(presetClones[0]);
            presetClones.RemoveAt(0);
        }

        MaMScroller.firstPreset = 0;

        XMLManager.ins.LoadItems();
        LoadPresets();

        deletePresetButton.SetActive(false);
        presetLeftScroll.SetActive(false);
        presetRightScroll.SetActive(false);
        if (presetClones.Count > 3) presetRightScroll.SetActive(true);
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
            presetClones.Add(clone);

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
            presetClones.Add(clone);
            clone.SetActive(false);

            //Creates a listner for OnClick
            int value = i;
            Button buttonCtrl = clone.GetComponent<Button>();
            buttonCtrl.onClick.AddListener(() => SelectPresets(value));
        }

    }

    public void ToggleSetPresetPanel()
    {
        setPresetPanel.SetActive(!setPresetPanel.activeSelf);
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
        for (int i = 0; i < wheels.Count; i++)
        {
            SetColor(wheels[i], Color.white);
        }

        //selects the wheel that is clicked
        SetColor(wheels[wheel], purple);
        this.gameObject.GetComponent<MaMInfoText>().SetWheelInfoText(wheel);
        selectedWheel = wheel;
    }

    /// <summary>
    /// Selects a drive base, as referenced by its index in the bases list
    /// </summary>
    /// <param name="driveBase"></param>
    public void SelectDriveBase(int driveBase)
    {
        Color purple = new Color(0.757f, 0.200f, 0.757f);

        //unselects all wheels
        for (int j = 0; j < bases.Count; j++)
        {
            SetColor(bases[j], Color.white);
        }

        //selects the wheel that is clicked
        SetColor(bases[driveBase], purple);
        this.gameObject.GetComponent<MaMInfoText>().SetBaseInfoText(driveBase);
        selectedDriveBase = driveBase;
        mecWheelPanel.SetActive(false);
        if (selectedDriveBase == 1)
        {
            isMecanum = true;
            mecWheelPanel.SetActive(true);
        }
    }

    public static bool GetMecanum()
    {
        return isMecanum;
    }

    /// <summary>
    /// Selects a manipulator, as referenced by its index in the manipualtors list.
    /// </summary>
    /// <param name="manipulator"></param>
    public void SelectManipulator(int manipulator)
    {
        Color purple = new Color(0.757f, 0.200f, 0.757f);

        //unselects all manipulators
        for (int k = 0; k < manipulators.Count; k++)
        {
            SetColor(manipulators[k], Color.white);
        }

        //selects the manipulator that is clicked
        SetColor(manipulators[manipulator], purple);
        this.gameObject.GetComponent<MaMInfoText>().SetManipulatorInfoText(manipulator);
        //hasManipulator = (manipulator == 0) ? false : true;
        selectedManipulator = manipulator;
    }

    public void SelectManipulator(int manipulator, string presetName)
    {
        Color purple = new Color(0.757f, 0.200f, 0.757f);

        //unselects all manipulators
        for (int k = 0; k < manipulators.Count; k++)
        {
            SetColor(manipulators[k], Color.white);
        }

        //selects the manipulator that is clicked
        SetColor(manipulators[manipulator], purple);
        hasManipulator = (manipulator == 0) ? false : true;
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