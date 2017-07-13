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
    public void switchViewClick()
    {
        switch (x)
        {
            case 0:
                dynamicCamera.SwitchCameraState(new DynamicCamera.DriverStationState(dynamicCamera));
                x++;
                break;
            case 1:
                dynamicCamera.SwitchCameraState(new DynamicCamera.OrbitState(dynamicCamera));
                dynamicCamera.EnableMoving();
                x++;
                break;
            case 2:
                dynamicCamera.SwitchCameraState(new DynamicCamera.FreeroamState(dynamicCamera));
                x = 0;
                break;
        }
    }
    public void switchViewClickMoreBetterer(int joe)
    {
        switch (joe)
        {
            case 1:
                dynamicCamera.SwitchCameraState(new DynamicCamera.DriverStationState(dynamicCamera));
                break;
            case 2:
                dynamicCamera.SwitchCameraState(new DynamicCamera.OrbitState(dynamicCamera));
                dynamicCamera.EnableMoving();
                break;
            case 3:
                dynamicCamera.SwitchCameraState(new DynamicCamera.FreeroamState(dynamicCamera));
                break;
        }
    }
}
   