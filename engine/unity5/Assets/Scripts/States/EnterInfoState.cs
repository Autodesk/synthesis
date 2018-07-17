using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Synthesis.States
{
    public class EnterInfoState : State
    {
        public void OnJoinButtonPressed()
        {
            string lobbyCode = GameObject.Find("LobbyCodeText").GetComponent<Text>().text;
            string networkAddress = IPCrypt.Decrypt(lobbyCode);

            if (networkAddress.Equals(string.Empty))
            {
                UserMessageManager.Dispatch("Invalid lobby code!", 5f);
                return;
            }

            StateMachine.PushState(new ConnectingState(lobbyCode, networkAddress));
        }
    }
}
