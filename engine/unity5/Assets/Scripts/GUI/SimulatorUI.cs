using Assets.Scripts.FSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SimulatorUI : MonoBehaviour
{
    private DynamicCamera dynamicCamera;

    private int x = 0;
    public GameObject stateMachine;
    public MainState mainState;

    private GameObject canvas;

    // Use this for initialization
    void Start()
    {

        canvas = GameObject.Find("Canvas");
    }

    // Update is called once per frame
    void Update()
    {
        if (dynamicCamera == null)
        {
            dynamicCamera = GameObject.Find("Main Camera").AddComponent<DynamicCamera>();
        }
        //dynamicCamera.cameraState
        //if (Input.GetKey(Controls.ControlKey[Controls.Control.CameraToggle])) {
        //  if (dynamicCamera.cameraState ) { 



        //  }
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


    //In game UI loads robot using UI icons
    public void robotClick()
    {

    }

    //In game UI loads field using UI icons
    public void fieldClick()
    {

    }

    //In game UI resets robot using UI icons
    public void resetRobotClick()
    {
        mainState = stateMachine.GetComponent<StateMachine>().MainState;
        mainState.BeginReset();
        mainState.EndReset();
    }

    //In game UI switches view using UI icons
    public void SwitchViewClickMoreBetterer(int joe)
    {
        switch (joe)
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
        }
    }

    //In game UI button opens tutorial link in browser
   
    public void toLink()
    {
        Application.OpenURL("http://bxd.autodesk.com/tutorials.html");
    }

}
   