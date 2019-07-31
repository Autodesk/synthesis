using Synthesis.FSM;
using Synthesis.Utils;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace Synthesis.States
{
    public class ErrorScreenState : State
    {

        /// <summary>
        /// Initailizes references to requried <see cref="GameObject"/>s and sets the error message text.
        /// </summary>
        public override void Start()
        {
            string error = AppModel.ErrorMessage;
            if (error.Split('|')[0].Equals("ROBOT_SELECT")) {
                AppModel.ClearError();
                StateMachine.ChangeState(new LoadRobotState());
                Auxiliary.FindGameObject("ErrorNote").GetComponent<Text>().text = error.Split('|')[1];
            }
            else {
                Auxiliary.FindGameObject("ErrorScreen").SetActive(true);
                Auxiliary.FindGameObject("ErrorText").GetComponent<Text>().text = AppModel.ErrorMessage;
                AppModel.ClearError();
            }
        }

        /// <summary>
        /// Exits this <see cref="State"/> when the OK button is pressed.
        /// </summary>
        public void OnExitButtonClicked()
        {
            if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        public void OnBackToSimButtonClicked()
        {
            Auxiliary.FindGameObject("LoadSplash").SetActive(true);
            SceneManager.LoadScene("Scene");
        }

        public override void End() {
            Auxiliary.FindGameObject("ErrorScreen").SetActive(false);
        }
    }
}
