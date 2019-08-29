//using Synthesis.FSM;
//using Synthesis.Utils;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;

//namespace Synthesis.States
//{
//    public class DisclaimerState : State
//    {
//        private readonly bool isFirstState;

//        private GameObject checkBox;
//        private GameObject continueButton;
        
//        /// <summary>
//        /// Initializes a new <see cref="DisclaimerState"/> instance.
//        /// </summary>
//        /// <param name="isFirstState"></param>
//        public DisclaimerState(bool isFirstState)
//        {
//            this.isFirstState = isFirstState;
//        }

//        /// <summary>
//        /// Establishes references to required <see cref="GameObject"/>s and
//        /// sets their properties.
//        /// </summary>
//        public override void Start()
//        {
//            checkBox = Auxiliary.FindGameObject("Checkbox");
//            continueButton = Auxiliary.FindGameObject("ContinueButton");

//            continueButton.SetActive(isFirstState);
//            checkBox.GetComponent<Toggle>().isOn = PlayerPrefs.GetInt("ShowDisclaimer", 1) == 1 ? true : false;
//        }

//        /// <summary>
//        /// Returns to the previous state if it exists. Otherwise, exits to the main
//        /// menu.
//        /// </summary>
//        public void OnBackButtonClicked()
//        {
//            if (isFirstState)
//            {
//                Auxiliary.FindGameObject("ExitingPanel").SetActive(true);
//                SceneManager.LoadScene("MainMenu");
//            }
            
//            StateMachine.PopState();
//        }

//        /// <summary>
//        /// Changes the <see cref="PlayerPrefs"/> property associated with showing
//        /// the disclaimer when the check box is checked.
//        /// </summary>
//        /// <param name="value"></param>
//        public void OnCheckboxValueChanged(bool value)
//        {
//            PlayerPrefs.SetInt("ShowDisclaimer", value ? 1 : 0);
//        }

//        /// <summary>
//        /// Launches a new <see cref="HostJoinState"/> when the continue button is pressed.
//        /// </summary>
//        public void OnContinueButtonClicked()
//        {
//            StateMachine.ChangeState(new HostJoinState());
//        }
//    }
//}
