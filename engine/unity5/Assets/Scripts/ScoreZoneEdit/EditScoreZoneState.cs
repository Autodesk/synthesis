using System;
using System.Collections;
using System.Collections.Generic;
using Assets.Scripts.FSM;
using UnityEngine;

public class EditScoreZoneState : SimState
{
    public GameObject mainCam { get; private set; }

    private bool LoadScene = true; // For debugging
    
    /// <summary>
    /// Called when the SimState is started.
    /// </summary>
    public override void Start()
    {
        Debug.Log("Initializing EditScoreZone state machine");
        if (LoadScene)
            Debug.Log(RobotFieldLoader.LoadField() ? "Load field success!" : "Load field failed.");
        
        mainCam =  GameObject.Find("Main Camera");
        
        mainCam.GetComponent<DynamicCamera>().SwitchCameraState(0);
        DynamicCamera.MovingEnabled = true;
    }

    /// <summary>
    /// Called when the SimState is ended.
    /// </summary>
    public override void End()
    {
    }

    /// <summary>
    /// Called when the SimState is paused.
    /// </summary>
    public override void Pause()
    {
        
    }

    /// <summary>
    /// Called when the SimState is resumed.
    /// </summary>
    public override void Resume()
    {
    }

    /// <summary>
    /// Called when the SimState is update.
    /// </summary>
    public override void Update()
    {
        
    }

    /// <summary>
    /// Called when the SimState is updated late.
    /// </summary>
    public override void LateUpdate()
    {
        
    }

    /// <summary>
    /// Called when the SimState is updated on a fixed basis.
    /// </summary>
    public override void FixedUpdate()
    {
        
    }

    /// <summary>
    /// Called when the GUI is updated in the SimState.
    /// </summary>
    public override void OnGUI()
    {
        
    }

    /// <summary>
    /// Called when the SimState is awaken.
    /// </summary>
    public override void Awake()
    {
        
    }
}