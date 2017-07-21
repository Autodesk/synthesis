using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickSwapMode : MonoBehaviour
{
    private GameObject quickSwapMode;
    
    //Wheel options
    private GameObject tractionWheel;
    private GameObject colsonWheel;
    private GameObject mecanumWheel;
    private GameObject omniWheel;
    List<GameObject> wheels;

    //Drive Base options
    private GameObject defaultDrive;
    List<GameObject> bases;

    //Manipulator Options
    private GameObject defaultManipulator;
    List<GameObject> manipulators;

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
        mecanumWheel = GameObject.Find("MecanumWheel");
        omniWheel = GameObject.Find("OmniWheel");
        //Put all the wheels in the wheels list
        wheels = new List<GameObject> { tractionWheel, colsonWheel, mecanumWheel, omniWheel };

        //Find drive base objects
        defaultDrive = GameObject.Find("DefaultBase");
        //Put all the drive bases in the bases list
        bases = new List<GameObject> { defaultDrive };

        //Find manipulator objects
        defaultManipulator = GameObject.Find("DefaultManipulator");
        //Put all the manipulators in the manipulators list
        manipulators = new List<GameObject> { defaultManipulator };
    }

    public void StartQuickSwap()
    {
        //Selects the traction wheel (default)  
        SelectWheel(0);

        //Selects the default base
        SelectDriveBase(0);

        //Selects the default manipulator
        SelectManipulator(0);
    }

    public void SetColor(GameObject part, Color color)
    {
        part.GetComponent<Image>().color = color;
    }

    /// <summary>
    /// Selects a wheel, as referenced by its index in the wheels list
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
    }

    /// <summary>
    /// Selects a drive base, as referenced by its index in the bases list
    /// </summary>
    /// <param name="driveBase"></param>
    public void SelectDriveBase(int driveBase) //Drive Bases are indexed in the list bases
    {
        Color purple = new Color(0.757f, 0.200f, 0.757f);
        
        //unselects all wheels
        for (int j = 0; j < bases.Count; j++)
        {
            SetColor(bases[j], Color.white);          
        }
        
        //selects the wheel that is clicked
        SetColor(bases[driveBase], purple);
    }

    /// <summary>
    /// Selects a manipulator, as referenced by its index in the manipualtors list
    /// </summary>
    /// <param name="manipulator"></param>
    public void SelectManipulator(int manipulator) //Drive Bases are indexed in the list bases
    {
        Color purple = new Color(0.757f, 0.200f, 0.757f);

        //unselects all wheels
        for (int k = 0; k < manipulators.Count; k++)
        {
            SetColor(manipulators[k], Color.white);
        }

        //selects the wheel that is clicked
        SetColor(manipulators[manipulator], purple);
    }

}