using Synthesis.FSM;
using UnityEngine;

namespace Synthesis.States
{
    public class HomeTabState : State
    {
        /// <summary>
        /// Launches the Sim Tab when the start button is pressed.
        /// </summary>
        public void OnStartButtonPressed()
        {
            StateMachine.ChangeState(new SimTabState());
        }

        /// <summary>
        /// Switches to the options tab and its respective UI elements.
        /// </summary>
        public void OnSettingsButtonPressed()
        {
            StateMachine.SceneGlobal.ChangeState(new OptionsTabState());
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

        /// <summary>
        /// Exits the program.
        /// </summary>
        public void OnExitButtonPressed()
        {
            if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}