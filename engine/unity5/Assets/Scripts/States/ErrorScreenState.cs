using Synthesis.FSM;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Experimental.UIElements;
using UnityEngine.UI;

namespace Synthesis.States
{
    public class ErrorScreenState : State
    {
        private GameObject navigationPanel;

        /// <summary>
        /// Initailizes references to requried <see cref="GameObject"/>s and sets the error message text.
        /// </summary>
        public override void Start()
        {
            navigationPanel = Auxiliary.FindGameObject("NavigationPanel");
            navigationPanel.SetActive(false);

            Auxiliary.FindGameObject("ErrorText").GetComponent<Text>().text = AppModel.ErrorMessage;
            AppModel.ClearError();
        }

        /// <summary>
        /// Enables the navigation panel when this <see cref="State"/> exits.
        /// </summary>
        public override void End()
        {
            navigationPanel.SetActive(true);
        }

        /// <summary>
        /// Exits this <see cref="State"/> when the OK button is pressed.
        /// </summary>
        public void OnOkButtonPressed()
        {
            StateMachine.ChangeState(new HomeTabState());
        }
    }
}
