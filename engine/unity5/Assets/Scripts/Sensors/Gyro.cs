using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using BulletUnity;
using BulletSharp;
using Assets.Scripts.FSM;

/// <summary>
/// Gyroscope sensor class, must be attached to a prefab gameobject
/// </summary>
public class Gyro : SensorBase
{
    
    //private float threshold = 5;
    //private float angleTurn;
    //private float sensitivity;
    //private float timePeriod = 2f;
    //private float outputTimePeriod = 2;
    //private GameObject parent;
    //private float currentTime;
    //private float lastOutputTime;
    //private float lastSampleTime;
    //private float currentSample;
    //private float lastSample;
    //private bool startUpdate = false;
    //private void FixedUpdate()
    //{
    //    if(parent == null)
    //    {
    //        parent = gameObject.transform.parent.gameObject;
    //    }else if (!startUpdate)
    //    {
    //        InvokeRepeating("TakeSamples", 0f, 0.1f);
    //    }
        
    //}

    /// <summary>
    /// Return the angle rotated on Y axis
    /// </summary>
    /// <returns></returns>
    public override float ReturnOutput()
    {
        return 0;
    }
    
    //public void TakeSamples()
    //{
    //    currentSample = gameObject.transform.parent.eulerAngles.y;
    //}
}