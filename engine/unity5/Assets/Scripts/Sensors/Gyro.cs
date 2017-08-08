using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BulletUnity;
using BulletSharp;
using Assets.Scripts.FSM;
using UnityEngine.UI;

/// <summary>
/// Gyroscope sensor class, must be attached to a prefab gameobject
/// </summary>
public class Gyro : SensorBase
{
    private float lastAngle;
    private float currentAngle;
    private float currentTime;
    private float lastTime;

    private void Update()
    {
        //Update current time and angle
        currentTime = Time.timeSinceLevelLoad;
        currentAngle = gameObject.transform.parent.transform.localEulerAngles.y;
        //Update the output panel
        UpdateOutputDisplay();
        Debug.Log(ReturnOutput());
        //Set last time to current time
        lastTime = currentTime;
    }
    /// <summary>
    /// Return the angle rotated on Y axis
    /// </summary>
    /// <returns></returns>
    public override float ReturnOutput()
    {
        float current = 0;
        float last = 0;
        //Handle the conditions when robot Y rotation jumps from 0 to 360
        if (Math.Abs(currentAngle - lastAngle) >= 300)
        {
            if (currentAngle > lastAngle)
            {
                last = lastAngle + 360;
                current = currentAngle;
            }
            else
            {
                current = currentAngle + 360;
                last = lastAngle;
            }
        }
        else
        {
            current = currentAngle;
            last = lastAngle;
        }
        //Debug.Log("lastAngle is " + lastAngle + " last is " + last);
        //Debug.Log("currentAngle is " + currentAngle + " current is " + current);
        //Set last angle to current angle
        lastAngle = currentAngle;
        //Calculate angular rotation rate in degrees/second
        return (current - last) / (currentTime - lastTime);
    }

    /// <summary>
    /// Do nothing because currently gyro does not have a range specification. Could be sensitivity in the future though
    /// </summary>
    public override void UpdateRangeTransform()
    {
        
    }

    public override void UpdateOutputDisplay()
    {
        base.UpdateOutputDisplay();
        GameObject outputPanel = GameObject.Find(gameObject.name + "_Panel");
        if (outputPanel != null)
        {
            GameObject outputText = AuxFunctions.FindObject(outputPanel, "Text");
            outputText.GetComponent<Text>().text = gameObject.name + " Output (degrees/s)";
        }
    }
}