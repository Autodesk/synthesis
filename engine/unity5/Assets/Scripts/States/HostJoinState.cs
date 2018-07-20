using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Network;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Synthesis.States
{
    public class HostJoinState : State
    {
        /// <summary>
        /// Launches a new <see cref="LobbyState"/> as the host.
        /// </summary>
        public void OnHostLobbyButtonPressed()
        {
            StateMachine.PushState(new EnterTagState());
        }

        /// <summary>
        /// Launches a new <see cref="EnterInfoState"/>.
        /// </summary>
        public void OnJoinLobbyButtonPressed()
        {
            StateMachine.PushState(new EnterInfoState());
        }

        /// <summary>
        /// Returns to the main menu when the back button is pressed.
        /// </summary>
        public void OnBackButtonPressed()
        {
            Auxiliary.FindGameObject("ExitingPanel").SetActive(true);
            SceneManager.LoadScene("MainMenu");

            StateMachine.PopState();
        }
    }
}
