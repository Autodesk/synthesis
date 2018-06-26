using Assets.Scripts.FSM;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DefaultSimulatorState : State
{
    private GameObject simFieldSelectText;
    private GameObject simRobotSelectText;
    private GameObject splashScreen;

    /// <summary>
    /// Initializes references to required <see cref="GameObject"/>s.
    /// </summary>
    public override void Start()
    {
        simFieldSelectText = AuxFunctions.FindGameObject("SimFieldSelectText");
        simRobotSelectText = AuxFunctions.FindGameObject("SimRobotSelectText");
        splashScreen = AuxFunctions.FindGameObject("LoadSplash");
    }

    /// <summary>
    /// Updates the robot and field selection text when this state is activated.
    /// </summary>
    public override void Resume()
    {
        simFieldSelectText.GetComponent<Text>().text = PlayerPrefs.GetString("simSelectedFieldName");
        simRobotSelectText.GetComponent<Text>().text = PlayerPrefs.GetString("simSelectedRobotName");
    }

    /// <summary>
    /// Pops this state when the back button is pressed.
    /// </summary>
    public void OnBackButtonPressed()
    {
        StateMachine.Instance.PopState();
    }

    /// <summary>
    /// Pushes a new <see cref="LoadReplayState"/> when the replays button is pressed.
    /// </summary>
    public void OnReplaysButtonPressed()
    {
        StateMachine.Instance.PushState(new LoadReplayState());
    }

    /// <summary>
    /// Pushes a new <see cref="LoadFieldState"/> when the change field button is pressed.
    /// </summary>
    public void OnChangeFieldButtonPressed()
    {
        StateMachine.Instance.PushState(new LoadFieldState());
    }

    /// <summary>
    /// Pushes a new <see cref="LoadRobotState"/> when teh change robot button is pressed.
    /// </summary>
    public void OnChangeRobotButtonPressed()
    {
        StateMachine.Instance.PushState(new LoadRobotState());
    }

    /// <summary>
    /// Starts the main simulator when the start button is pressed.
    /// </summary>
    public void OnStartButtonPressed()
    {
        string selectedField = PlayerPrefs.GetString("simSelectedField");
        string selectedRobot = PlayerPrefs.GetString("simSelectedRobot");

        if (Directory.Exists(selectedField) && Directory.Exists(selectedRobot))
        {
            splashScreen.SetActive(true);
            PlayerPrefs.SetString("simSelectedReplay", string.Empty);
            SceneManager.LoadScene("Scene");
            RobotTypeManager.SetProperties(false);
        }
        else
        {
            UserMessageManager.Dispatch("No Robot/Field Selected!", 2);
        }
    }
}
