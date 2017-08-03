
﻿using Assets.Scripts.FSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Analytics;

public class SimulatorUI : MonoBehaviour
{
    private DynamicCamera dynamicCamera;

    private int x = 0;
    public GameObject stateMachine;
    public MainState mainState;

    private Color hoverColor;
    private Vector3 toolTransform;

    private GameObject canvas;
    private GameObject cameraToolTip;
    private GameObject orientWindow;
    private bool isOrienting;
    private GameObject resetDropdown;

    private Image imgTemp;

    private Transform tTransform;

    // Use this for initialization
    void Start()
    {

        canvas = GameObject.Find("Canvas");
        stateMachine = GameObject.Find("StateMachine");
        imgTemp = AuxFunctions.FindObject("Toolbar").GetComponent<Image>();
        tTransform = AuxFunctions.FindObject("Toolbar").GetComponent<Transform>();
        toolTransform = tTransform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (dynamicCamera == null)
        {
            dynamicCamera = GameObject.Find("Main Camera").GetComponent<DynamicCamera>();
        }


        if (mainState == null)
        {
            mainState = stateMachine.GetComponent<StateMachine>().CurrentState as MainState;
        }
    }

    public void showControlPanel(bool show)
    {
        if (show)
        {
            AuxFunctions.FindObject(canvas, "FullscreenPanel").SetActive(true);
        }
        else
        {
            AuxFunctions.FindObject(canvas, "FullscreenPanel").SetActive(false);
        }
    }
    /*
    public void hoverOpacityEnter()
    {
        hoverColor = imgTemp.color;
        //AuxFunctions.FindObject("Toolbar").GetComponent<Image>().color = new Color();
       imgTemp.color = new Color(0, 0, 0, 255);
    }

    public void hoverOpacityExit()
    {
        imgTemp.color = hoverColor;
    }
    */

    //In game UI loads robot using UI icons
    public void robotClick()
    {

    }

    //In game UI button activates driver practice mode
    public void driverPracticeClick()
    {
        Analytics.CustomEvent("Driver Practice Mode", new Dictionary<string, object>
                    {
                        { "mode", true },
                    });

    }
    //In game UI loads field using UI icons
    public void fieldClick()
    {

    }


    //In game UI switches view using UI icons
    public void SwitchViewClick(int cam)
    {
        switch (cam)
        {
            case 1:
                dynamicCamera.SwitchCameraState(new DynamicCamera.DriverStationState(dynamicCamera));
                break;
            case 2:
                dynamicCamera.SwitchCameraState(new DynamicCamera.OrbitState(dynamicCamera));
                DynamicCamera.MovingEnabled = true;
                break;
            case 3:
                dynamicCamera.SwitchCameraState(new DynamicCamera.FreeroamState(dynamicCamera));
                break;
            case 4:
                dynamicCamera.SwitchCameraState(new DynamicCamera.OverviewState(dynamicCamera));
                break; 
        }
    }

    public void StartReplay()
    {
        mainState.StartReplay();
    }

    //toolbar animation 

    public void hoverToolOff()
    {
        //tempPos.y = tempVal + amplitude * Mathf.Sin(speed * Time.time);
        //transform.position = tempPos;
       
        tTransform.position = new Vector3(toolTransform.x, toolTransform.y + 60, toolTransform.z);
    }

    public void hoverToolOn()
    {
        //tempPos.y = tempVal + amplitude * Mathf.Sin(speed * Time.time);
        //transform.position = tempPos;
        tTransform.position = toolTransform;
    }


    //In game UI button opens tutorial link in browser

    public void toLink()
    {
        Application.OpenURL("http://bxd.autodesk.com/tutorials.html");
    }

    public void ReplayModeActivated()
    {
    
    }

    public void CameraToolTips()
    {
        if (dynamicCamera.cameraState.GetType().Equals(typeof(DynamicCamera.DriverStationState)))
            cameraToolTip.GetComponent<Text>().text = "Driver Station";
        else if (dynamicCamera.cameraState.GetType().Equals(typeof(DynamicCamera.FreeroamState)))
            cameraToolTip.GetComponent<Text>().text = "Freeroam";
        else if (dynamicCamera.cameraState.GetType().Equals(typeof(DynamicCamera.OrbitState)))
            cameraToolTip.GetComponent<Text>().text = "Orbit Robot";
        else if (dynamicCamera.cameraState.GetType().Equals(typeof(DynamicCamera.OverviewState)))
            cameraToolTip.GetComponent<Text>().text = "Overview";
    }
}