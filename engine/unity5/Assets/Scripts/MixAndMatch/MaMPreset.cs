using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MaMPreset {

    public string Name = "Default";
    public int WheelIndex = 0;
    public int DriveBaseIndex = 0;
    public int ManipulatorIndex = 0;

    public MaMPreset (int wheel, int driveBase, int manipulator, string name)
    {
        WheelIndex = wheel;
        DriveBaseIndex = driveBase;
        ManipulatorIndex = manipulator;

        this.Name = name;
    }

    public MaMPreset()
    {

    }

    public string GetName()
    {
        return Name;
    }

    public int GetWheel()
    {
        return WheelIndex;
    }

    public int GetDriveBase()
    {
        return DriveBaseIndex;
    }

    public int GetManipulator()
    {
        return ManipulatorIndex;
    }
	// Update is called once per frame
	void Update () {
		
	}
}
