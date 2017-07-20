using Assets.Scripts.FSM;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class SimulatorUI : MonoBehaviour
{
    private DynamicCamera dynamicCamera;

    private int x = 0;
    public GameObject stateMachine;
    private MainState mainState;

    // Use this for initialization
    void Start()
    {
        dynamicCamera = GameObject.Find("Main Camera").AddComponent<DynamicCamera>();
    }

    // Update is called once per frame
    void Update()
    {
        //dynamicCamera.cameraState
        //if (Input.GetKey(Controls.ControlKey[Controls.Control.CameraToggle])) {
      //  if (dynamicCamera.cameraState ) { 



      //  }
    }

    public void showControlPanel(bool show)
    {
        if (show)
        {
            AuxFunctions.FindObject("FullscreenPanel").GetComponent<RectTransform>().localScale = new Vector3(1, 1);
        }
        else
        {
            AuxFunctions.FindObject("FullscreenPanel").GetComponent<RectTransform>().localScale = new Vector3(0, 0);
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
        mainState = stateMachine.GetComponent<StateMachine>().GetMainState();
        mainState.BeginReset(true);
        mainState.EndReset();
    }

    //In game UI switches view using UI icons
  
    public void switchViewClickMoreBetterer(int joe)
    {
        switch (joe)
        {
            case 1:
                dynamicCamera.SwitchCameraState(new DynamicCamera.DriverStationState(dynamicCamera));
                Debug.Log("go ahead and type out what you were going to type out");
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
}
   