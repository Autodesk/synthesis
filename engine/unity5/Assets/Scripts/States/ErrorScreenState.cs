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
            Debug.Log("You did a good");
            Auxiliary.FindGameObject("ErrorText").GetComponent<Text>().text = AppModel.ErrorMessage;
            AppModel.ClearError();
        }

        /// <summary>
        /// Exits this <see cref="State"/> when the OK button is pressed.
        /// </summary>
        public void OnExitButtonClicked()
        {
            Debug.Log("EH?");
            if (!Application.isEditor) System.Diagnostics.Process.GetCurrentProcess().Kill();
        }

        public void OnBackToSimButtonClicked()
        {
            Debug.Log("and another good?");
            SceneManager.LoadScene("Scene");
        }
    }
}
