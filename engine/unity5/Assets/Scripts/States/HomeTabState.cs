using Synthesis.FSM;
using UnityEngine;

namespace Synthesis.States
{
    public class HomeTabState : State
    {
        /// <summary>
        /// 
        /// es the Sim Tab when the start button is pressed.
        /// </summary>
        public void OnStartButtonClicked()
        {
            StateMachine.ChangeState(new SimTabState());
        }

        /// <summary>
        /// Switches to the options tab and its respective UI elements.
        /// </summary>
        public void OnSettingsButtonClicked()
        {
            StateMachine.SceneGlobal.ChangeState(new OptionsTabState());
        }

        /// <summary>
        /// Opens the tutorials webpage in the browser when the tutorials button is presssed.
        /// </summary>
        public void OnTutorialsButtonClicked()
        {
            Application.OpenURL("http://synthesis.autodesk.com/tutorials.html");
        }

        /// <summary>
        /// Opens the website in the browser when the website button is pressed.
        /// </summary>
        public void OnWebsiteButtonClicked()
        {
            Application.OpenURL("http://synthesis.autodesk.com");
        }

        /// <summary>
        /// Exits the program.
        /// </summary>
        public void OnExitButtonClicked()
        {
            if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
        }
    }
}