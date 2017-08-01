using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class QuickSwapMode : MonoBehaviour
{
    private GameObject quickSwapMode;
    
    //Wheel options
    private GameObject tractionWheel;
    private GameObject colsonWheel;
    private GameObject omniWheel;
    List<GameObject> wheels;
    int selectedWheel;

    //Drive Base options
    private GameObject defaultDrive;
    private GameObject mecanumDrive;
    List<GameObject> bases;
    int selectedDriveBase;
    public static bool isMecanum = false;

    //Manipulator Options
    private GameObject noManipulator;
    private GameObject syntheClaw;
    List<GameObject> manipulators;
    int selectedManipulator;
    public static bool hasManipulator = true;

    // Use this for initialization
    void Start()
    {
        FindAllGameObjects();
        StartQuickSwap();
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FindAllGameObjects()
    {
        quickSwapMode = GameObject.Find("QuickSwapMode");

        //Find wheel objects
        tractionWheel = GameObject.Find("TractionWheel");
        colsonWheel = GameObject.Find("ColsonWheel");
        omniWheel = GameObject.Find("OmniWheel");
        //Put all the wheels in the wheels list
        wheels = new List<GameObject> { tractionWheel, colsonWheel, omniWheel };
        

        //Find drive base objects
        defaultDrive = GameObject.Find("DefaultBase");
        mecanumDrive = GameObject.Find("MecanumBase");
        //Put all the drive bases in the bases list
        bases = new List<GameObject> { defaultDrive, mecanumDrive };

        //Find manipulator objects
        noManipulator = GameObject.Find("NoManipulator");
        syntheClaw = GameObject.Find("SyntheClaw");
        //Put all the manipulators in the manipulators list
        manipulators = new List<GameObject> { noManipulator, syntheClaw };
    }

    /// <summary>
    /// Called when the QuickSwap Configuration tab is opened from the main menu. 
    /// </summary>
    public void StartQuickSwap()
    {
        //Selects the traction wheel (default)  
        SelectWheel(0);

        //Selects the default base
        SelectDriveBase(0);

        //Selects the default manipulator
        SelectManipulator(0);
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
        hasManipulator = (manipulator == 0) ? false : true;
        selectedManipulator = manipulator;
    }

    ///<summary>
    ///Returns the string destination path of a drive base.
    /// </summary>
    /// <param name="baseID"></param>
    public string GetDriveBase(int baseID)
    {
        switch (baseID)
        {
            case 0: //Default Drive Base
                return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\DriveBases\\DriveBase2557");
            case 1: //Mech Drive Base
                return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\DriveBases\\MechDrive");
        }

        return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\DriveBases\\DriveBase2557");
    }

    /// <summary>
    /// Returns the string destination path of a manipulator.
    /// </summary>
    public string GetManipulator(int manipulatorID)
    {
        switch (manipulatorID)
        {
            case 0: //No manipulator
                hasManipulator = false;
                break;
            case 1: //SyntheClaw
                hasManipulator = true;
                return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\Manipulators\\Claw");
        }
        return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\Manipulators\\Claw");
    }

    /// <summary>
    /// Sets the destination paths of the selected field, robot base and manipulator to be used by MainState. Starts the simulation in Quick Swap Mode. 
    /// </summary>
    public void StartSwapSim()
    {
        PlayerPrefs.SetString("simSelectedField", "C:\\Program Files (x86)\\Autodesk\\Synthesis\\Synthesis\\Fields\\2014 Aerial Assist");
        PlayerPrefs.SetString("simSelectedFieldName", "2014 Aerial Assist");
        PlayerPrefs.SetString("simSelectedRobot", GetDriveBase(selectedDriveBase));
        PlayerPrefs.SetString("simSelectedRobotName", "DriveBase2557");
        PlayerPrefs.SetString("simSelectedManipulator", GetManipulator(selectedManipulator));
        PlayerPrefs.Save();
        SceneManager.LoadScene("QuickSwap");
    }
}