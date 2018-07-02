using Assets.Scripts.FSM;
using UnityEngine;

public class HomeTabState : State
{
    /// <summary>
    /// Launches the Sim Tab when the start button is pressed.
    /// </summary>
    public void OnStartButtonPressed()
    {
        StateMachine.Instance.ChangeState(new SimTabState());
    }

    /// <summary>
    /// Opens the tutorials webpage in the browser when the tutorials button is presssed.
    /// </summary>
    public void OnTutorialsButtonPressed()
    {
        Application.OpenURL("http://bxd.autodesk.com/tutorials.html");
    }

    /// <summary>
    /// Opens the website in the browser when the website button is pressed.
    /// </summary>
    public void OnWebsiteButtonPressed()
    {
        Application.OpenURL("http://bxd.autodesk.com/");
    }
}
