using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MaMInfoText : MonoBehaviour {
    private GameObject infoText;
    public void FindAllGameObjects()
    {
        infoText = GameObject.Find("PartDescription");
    }
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
                txt.text = "Mecanum Drive \n\nAllows robot to strafe from horizontally. \nUse inputs for PWM 2 for strafing";
                break;
            case 2: //Swerve Drive           
                txt.text = "Swerve Drive \n\nAllows wheels to swivel. \nUse controls for PWM port 2 to swivel wheels"; //Check if it is PWM port 2
                break;
            case 3: //Narrow Drive
                txt.text = "Narrow Drive \n\n";
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
                txt.text = "Syntheclaw \n\nIdeal for handling Yoga Balls. Use controls for PWM 5.";
                break;
            case 2: //syntheshot
                txt.text = "Syntheshot \n\nIdeal for shooting projectiles. Use controls for PWM 5 and 6.";
                break;
            case 3: //
                txt.text = "Lift \n\nIdeal for picking up and stacking game elements. Use controls for PWM 5 and 6.";
                break;
        }

    }
}
