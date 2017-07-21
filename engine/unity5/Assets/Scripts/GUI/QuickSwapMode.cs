using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QuickSwapMode : MonoBehaviour {
    private GameObject quickSwapMode;
    //Wheel options
    private GameObject tractionWheel;
    private GameObject colsonWheel;
    private GameObject mecanumWheel;
    private GameObject omniWheel;

    //Drive Base options
    private GameObject defaultDrive;

    //Manipulator Options
    private GameObject defaultManipulator;

	// Use this for initialization
	void Start () {
        FindAllGameObjects();
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void FindAllGameObjects()
    {
        quickSwapMode = GameObject.Find("QuickSwapMode");//AuxFunctions.FindObject(gameObject, "QuickSwapMode");
        //We need to make refernces to various buttons/text game objects, but using GameObject.Find is inefficient if we do it every update.
        //Therefore, we assign variables to them and only use GameObject.Find once for each object in startup.
        tractionWheel = GameObject.Find("TractionWheel");//AuxFunctions.FindObject(quickSwapMode, "TractionWheel");
        colsonWheel = AuxFunctions.FindObject(quickSwapMode, "ColsonWheel");
        mecanumWheel = AuxFunctions.FindObject(quickSwapMode, "MecanumWheel");
        omniWheel = AuxFunctions.FindObject(quickSwapMode, "OmniWheel");

        defaultDrive = AuxFunctions.FindObject(quickSwapMode, "DefaultBase");

        defaultManipulator = AuxFunctions.FindObject(quickSwapMode, "DefaultManipulator");
    }

    public void SetColor(GameObject part, Color color)
    {
        part.GetComponent<Image>().color = color;
    }

    public void SelectWheel(string Wheel)
    {
        List<GameObject> wheels = new List<GameObject> {tractionWheel, colsonWheel, mecanumWheel, omniWheel };
        Color purple = new Color(0.757f, 0.200f, 0.757f);
        Color white = new Color(1f, 1f, 1f);

        for(int i = 0; i< wheels.Count; i++)
        {
            SetColor(wheels[i], white);
        }

        switch (Wheel)
        {
            case "traction":
                SetColor(tractionWheel, purple);
                break;
            case "colson":
                SetColor(colsonWheel, purple);
                break;
            case "mecanum":
                SetColor(mecanumWheel, purple);
                break;
            case "omni":
                SetColor(omniWheel, purple);
                break;
            default:
                break;
        }
    }
}