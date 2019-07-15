//using Synthesis.FSM;
//using Synthesis.GUI;
//using Synthesis.Network;
//using Synthesis.States;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.Networking;
//using UnityEngine.UI;

//namespace Synthesis.States
//{
//    public class EnterTagState : State
//    {
//        /// <summary>
//        /// Joins the lobby with the given code and player tag when the join button
//        /// is pressed.
//        /// </summary>
//        public void OnCreateButtonClicked()
//        {
//            string playerTag = GameObject.Find("HostTagText").GetComponent<Text>().text;

//            if (playerTag.Length == 0)
//            {
//                UserMessageManager.Dispatch("Please enter a player tag.", 5f);
//                return;
//            }

//            PlayerIdentity.DefaultLocalPlayerTag = playerTag;

//            StateMachine.PushState(new LoadFieldState(new LoadRobotState(new LobbyState(null))));
//        }

//        /// <summary>
//        /// Pops this <see cref="State"/> when the back button is pressed.
//        /// </summary>
//        public void OnBackButtonClicked()
//        {
//            StateMachine.PopState();
//        }
//    }
//}
