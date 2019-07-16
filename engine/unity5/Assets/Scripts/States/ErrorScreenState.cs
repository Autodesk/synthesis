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
            Auxiliary.FindGameObject("ErrorText").GetComponent<Text>().text = AppModel.ErrorMessage;
            AppModel.ClearError();
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
            SceneManager.LoadScene("Scene");
        }
    }
}
