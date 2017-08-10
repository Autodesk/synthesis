﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MixAndMatchMode : MonoBehaviour
{

    private GameObject mixAndMatchMode;
    private GameObject mixAndMatchModeScript;
    public static bool isMixAndMatchMode = false;
    private GameObject infoText;

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
    [HideInInspector] public List<GameObject> bases;
    int selectedDriveBase;
    public static bool isMecanum = false;

    //Manipulator Options
    private GameObject noManipulator;
    private GameObject syntheClaw;
    [HideInInspector] public List<GameObject> manipulators;
    int selectedManipulator;
    public static bool hasManipulator = true;

    //Scroll buttons
    private GameObject wheelRightScroll;
    private GameObject wheelLeftScroll;
    private GameObject presetRightScroll;
    private GameObject presetLeftScroll;

    // Use this for initialization
    void Start()
    {
        FindAllGameObjects();
        StartMixAndMatch();
        PlayerPrefs.SetInt("MixAndMatch", 0); //0 is true, 1 is false
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
        //Put all the drive bases in the bases list
        bases = new List<GameObject> { defaultDrive, mecanumDrive, swerveDrive };

        //Find manipulator objects
        noManipulator = GameObject.Find("NoManipulator");
        syntheClaw = GameObject.Find("SyntheClaw");
        //Put all the manipulators in the manipulators list
        manipulators = new List<GameObject> { noManipulator, syntheClaw };

        //Find all the scroll buttons
        wheelRightScroll = GameObject.Find("WheelRightScroll");
        wheelLeftScroll = GameObject.Find("WheelLeftScroll");
        presetRightScroll = GameObject.Find("PresetRightScroll");
        presetLeftScroll = GameObject.Find("PresetLeftScroll");

        if (this.gameObject.name == "MixAndMatchModeScript")
        {
            this.gameObject.GetComponent<MaMScroller>().FindAllGameObjects();
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
            presetLeftScroll.SetActive(false);
            presetRightScroll.SetActive(false);

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

            // Sets info panel to blank
            Text txt = infoText.GetComponent<Text>();
            txt.text = "";
        }
    }

    /// <summary>
    /// Sets the destination paths of the selected field, robot base and manipulator to be used by MainState. Starts the simulation in Quick Swap Mode. 
    /// </summary>
    public void StartSwapSim()
    {
        //PlayerPrefs.SetString("simSelectedField", "C:\\Program Files (x86)\\Autodesk\\Synthesis\\Synthesis\\Fields\\2014 Aerial Assist");
        //PlayerPrefs.SetString("simSelectedFieldName", "2014 Aerial Assist");
        
        PlayerPrefs.SetString("simSelectedRobot", mixAndMatchModeScript.GetComponent<MaMGetters>().GetDriveBase(selectedDriveBase));
        PlayerPrefs.SetString("simSelectedRobotName", "DriveBase2557");
        PlayerPrefs.SetString("simSelectedManipulator", mixAndMatchModeScript.GetComponent<MaMGetters>().GetManipulator(selectedManipulator));
        PlayerPrefs.SetFloat("wheelFriction", mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelFriction(selectedWheel));
        PlayerPrefs.SetFloat("wheelMass", mixAndMatchModeScript.GetComponent<MaMGetters>().GetWheelMass(selectedWheel));
        PlayerPrefs.Save();
        isMixAndMatchMode = true;
        SceneManager.LoadScene("Scene");
    }

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
    #region InfoText
    public void SetWheelInfoText(int wheel)
    {
        Text txt = infoText.GetComponent<Text>();
        txt.text = "";
        switch (wheel)
        {
            case 0: //Traction Wheel             
                txt.text = "Traction Wheel and Tread \n\nDimensions: 6\" diameter \nFriction coefficent: 1.1 \nMass: 0.43 kg";
                break;
            case 1: //Colson Wheel           
                txt.text = "Colson Performa Wheel \n\nDimensions: 4\" x 1.5\", 1/2\" Hex bore \nFriction coefficient: 1.0 \nMass: 0.24";
                break;
            case 2: //Omni Wheel            
                txt.text = "Omni Wheel \n\nDimensions: 6\" diameter \nFriction coefficent: 1.1 \nMass: 0.42 kg";
                break;
            case 3: //Pneumatic Wheel
                txt.text = "Pneumatic Wheel \n\nDimensions: 8\" x 1.8\" \nFriction coefficient: 0.93 \nMass: 0.51kg";
                break;
        }

    }

    public void SetBaseInfoText(int driveBase)
    {
        Text txt = infoText.GetComponent<Text>();
        txt.text = "";
        switch (driveBase)
        {
            case 0: //Default Drive          
                txt.text = "Default Drive\n \nNormal drive train  ";
                break;
            case 1: //Mecanum Drive       
                txt.text = "Mecanum Drive \n\nAllows robot to strafe from horizontally. \nUse left/right arrow keys to strafe. Use O and P to rotate.";
                break;
            case 2: //Swerve Drive           
                txt.text = "Swerve Drive \n\nAllows wheels to swivel. \nUse controls for PWM port 2 to swivel wheels"; //Check if it is PWM port 2
                break;
        }

    }

    public void SetManipulatorInfoText(int manipulator)
    {
        Text txt = infoText.GetComponent<Text>();
        txt.text = "";
        switch (manipulator)
        {
            case 0: //no manipulator      
                txt.text = "No Manipulator";
                break;
            case 1: //syntheclaw      
                txt.text = "Syntheclaw \n\nIdeal for handling Yoga Balls";
                break;
        }

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
        SetWheelInfoText(wheel);
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
        SetBaseInfoText(driveBase);
        selectedDriveBase = driveBase;
        if (selectedDriveBase == 1) isMecanum = true;
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
        SetManipulatorInfoText(manipulator);
        hasManipulator = (manipulator == 0) ? false : true;
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