﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaMGetters : MonoBehaviour {

    ///<summary>
    ///Returns the string destination path of a drive base.
    /// </summary>
    /// <param name="baseID"></param>
    public string GetDriveBase(int baseID)
    {
        switch (baseID)
        {
            case 0: //Default Drive Base
                return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\DriveBases\\Default");
            case 1: //Mech Drive Base
                return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\DriveBases\\SyntheMac");
            case 2: //Swerve Drive
                return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\DriveBases\\SyntheSwerve");
            case 3: //Narrow Drive
                return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\DriveBases\\Non");
        }

        return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\DriveBases\\DriveBase2557");
    }

    /// <summary>
    /// Returns the string destination path of a manipulator.
    /// </summary>
    /// <param name="manipulatorID"></param>
    public string GetManipulator(int manipulatorID)
    {
        switch (manipulatorID)
        {
            case 0: //No manipulator
                MixAndMatchMode.hasManipulator = false;
                PlayerPrefs.SetInt("hasManipulator", 0); //0 is false, 1 is true
                break;
            case 1: //SyntheClaw
                MixAndMatchMode.hasManipulator = true;
                PlayerPrefs.SetInt("hasManipulator", 1); //0 is false, 1 is true
                return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\Manipulators\\Claw");
            case 2: //SyntheShot
                MixAndMatchMode.hasManipulator = true;
                PlayerPrefs.SetInt("hasManipulator", 1); //0 is false, 1 is true
                return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\Manipulators\\SyntheShot");
            case 3: //Lift
                MixAndMatchMode.hasManipulator = true;
                PlayerPrefs.SetInt("hasManipulator", 1);
                return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\Manipulators\\SyntheLift");
        }
        return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\Manipulators\\Claw");
    }

    ///<summary>
    ///Returns the string destination path of a wheel
    /// </summary>
    public string GetWheel(int wheelID)
    {
        switch (wheelID)
        {
            case 0: //traction wheel
                return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\Wheels\\Traction");
            case 1: //colson wheel
                return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\Wheels\\Colson");
            case 2: //omni wheel
                return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\Wheels\\Omni");
            case 3: //pnemuatic wheel
                return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\Wheels\\Pneumatic");

        }

        return (System.Environment.GetFolderPath(System.Environment.SpecialFolder.MyDocuments) + "\\MixAndMatch\\Wheels\\Colson");
    }

    ///<summary> 
    /// Returns the coefficient of friction value associated with a wheel. The coeffecient of friction is taken from VexPro's website. 
    /// These may need to be adjusted.
    /// </summary>
    public float GetWheelFriction(int wheelID)
    {
        switch (wheelID)
        {
            case 0: //traction wheel
                return 1.1f;
            case 1: //colson wheel
                return 1.0f;
            case 2: //omni wheel
                return 1.1f;
            case 3: //pneumatic wheel
                return 0.93f;
        }
        return 1.0f;
    }

    ///<summary>
    ///Returns the wheel mass associated with a wheel. Masses are taken off Vex's website and converted to kilograms.
    /// </summary>
    public float GetWheelMass(int wheelID)
    {
        switch (wheelID)
        {
            case 0: //traction wheel
                return 0.43f;
            case 1: //colson wheel
                return 0.24f;
            case 2: //omni wheel
                return 0.42f;
            case 3: //pneumatic wheel
                return 0.51f;
        }
        return 1.0f;
    }

    ///<summary>
    ///Returns the wheel radius associated with a wheel. Radius are taken off Vex's website and converted to meters.
    /// </summary>
    public float GetWheelRadius(int wheelID)
    {
        switch (wheelID)
        {
            case 0: //traction wheel
                return 0.0762f;
            case 1: //colson wheel
                return 0.0508f;
            case 2: //omni wheel
                return 0.0762f;
            case 3: //pneumatic wheel
                return 0.1016f;
        }
        return 1.0f;
    }
}

