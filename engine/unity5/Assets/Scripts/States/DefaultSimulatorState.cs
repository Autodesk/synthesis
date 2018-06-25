using Assets.Scripts.FSM;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DefaultSimulatorState : State
{
    private GameObject simFieldSelectText;
    private GameObject simRobotSelectText;
    private GameObject splashScreen;

    public override void Start()
    {
        simFieldSelectText = AuxFunctions.FindGameObject("SimFieldSelectText");
        simRobotSelectText = AuxFunctions.FindGameObject("SimRobotSelectText");
        splashScreen = AuxFunctions.FindGameObject("LoadSplash");
    }

    public override void Resume()
    {
        simFieldSelectText.GetComponent<Text>().text = PlayerPrefs.GetString("simSelectedFieldName");
        simRobotSelectText.GetComponent<Text>().text = PlayerPrefs.GetString("simSelectedRobotName");
    }

    public void OnBackButtonPressed()
    {
        StateMachine.Instance.PopState();
    }

    public void OnChangeFieldButtonPressed()
    {
        StateMachine.Instance.PushState(new LoadFieldState());
    }

    public void OnChangeRobotButtonPressed()
    {
        StateMachine.Instance.PushState(new LoadRobotState());
    }

    public void OnStartButtonPressed()
    {
        string selectedField = PlayerPrefs.GetString("simSelectedField");
        string selectedRobot = PlayerPrefs.GetString("simSelectedRobot");

        if (Directory.Exists(selectedField) && Directory.Exists(selectedRobot))
        {
            splashScreen.SetActive(true);
            SceneManager.LoadScene("Scene");
            RobotTypeManager.SetProperties(false);
        }
        else
        {
            UserMessageManager.Dispatch("No Robot/Field Selected!", 2);
        }
    }
}
