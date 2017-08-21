using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MaMPreset {

    string name = "Default";
    int wheelIndex = 0;
    int driveBaseIndex = 0;
    int manipulatorIndex = 0;

    public MaMPreset (int wheel, int driveBase, int manipulator, string name)
    {
        wheelIndex = wheel;
        driveBaseIndex = driveBase;
        manipulatorIndex = manipulator;

        this.name = name;
    }

    public MaMPreset()
    {

    }

    public string GetName()
    {
        return name;
    }

    public int GetWheel()
    {
        return wheelIndex;
    }

    public int GetDriveBase()
    {
        return driveBaseIndex;
    }

    public int GetManipulator()
    {
        return manipulatorIndex;
    }
	// Update is called once per frame
	void Update () {
		
	}
}
