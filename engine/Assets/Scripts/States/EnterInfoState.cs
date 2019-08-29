//using Synthesis.FSM;
//using Synthesis.GUI;
//using Synthesis.Network;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using UnityEngine;
//using UnityEngine.Networking;
//using UnityEngine.UI;

//namespace Synthesis.States
//{
//    public class EnterInfoState : State
//    {
//        /// <summary>
//        /// Joins the lobby with the given code and player tag when the join button
//        /// is pressed.
//        /// </summary>
//        public void OnJoinButtonClicked()
//        {
//            string playerTag = GameObject.Find("PlayerTagText").GetComponent<Text>().text;

//            if (playerTag.Length == 0)
//            {
//                UserMessageManager.Dispatch("Please enter a player tag.", 5f);
//                return;
//            }

//            PlayerIdentity.DefaultLocalPlayerTag = playerTag;

//            string lobbyCode = GameObject.Find("LobbyCodeText").GetComponent<Text>().text;

//            if (IPCrypt.Decrypt(lobbyCode).Equals(string.Empty))
//                UserMessageManager.Dispatch("Invalid lobby code!", 5f);
//            else
//                StateMachine.PushState(new LoadRobotState(
//                    new LobbyState(lobbyCode)));
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
