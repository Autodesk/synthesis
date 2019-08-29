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
            if (error == null)
            {
                throw new System.Exception("Missing error message - don't start error scene directly");
            }
            string error_type = error.Contains("|") ? error.Split('|')[0] : "";
            string error_mesage = error.Contains("|") ? error.Split('|')[1] : error;

            if (error_type.Equals("ROBOT_SELECT"))
            {
                AppModel.ClearError();
                StateMachine.ChangeState(new LoadRobotState());
                if (error_mesage.Equals("FIRST")) Auxiliary.FindGameObject("ErrorNote").GetComponent<Text>().text = "";
                else Auxiliary.FindGameObject("ErrorNote").GetComponent<Text>().text = error_mesage;
                PlayerPrefs.SetString("simSelectedRobot", "");
                PlayerPrefs.SetString("simSelectedRobotName", "");
            }
            /*
            else if (error_type.Equals("FIELD_SELECT"))
            {
                AppModel.ClearError();
                StateMachine.ChangeState(new LoadFieldState());
                if (error_mesageEquals("FIRST")) Auxiliary.FindGameObject("ErrorNote").GetComponent<Text>().text = "";
                else Auxiliary.FindGameObject("ErrorNote").GetComponent<Text>().text = error_mesage;
                PlayerPrefs.SetString("simSelectedField", "");
                PlayerPrefs.SetString("simSelectedFieldName", "");
            }
            */
            else
            {
                if (error_type.Equals("FIELD_SELECT"))
                {
                    PlayerPrefs.SetString("simSelectedField", "");
                    PlayerPrefs.SetString("simSelectedFieldName", "");
                }
                Auxiliary.FindGameObject("ErrorScreen").SetActive(true);
                Auxiliary.FindGameObject("ErrorText").GetComponent<Text>().text = error_mesage;
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
