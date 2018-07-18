using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Synthesis.States
{
    public class LobbyState : State
    {
        private readonly bool host;
        private readonly string lobbyCode;

        public LobbyState(bool host, string lobbyCode)
        {
            this.host = host;
            this.lobbyCode = lobbyCode;
        }

        public override void Start()
        {
            GameObject.Find("LobbyCodeText").GetComponent<Text>().text = "Lobby Code: " + lobbyCode;

            if (!host)
            {
                MultiplayerNetwork network = MultiplayerNetwork.Instance;
                network.ConnectionStatusChanged += OnConnectionStatusChanged;
            }
        }

        public override void End()
        {
            MultiplayerNetwork network = MultiplayerNetwork.Instance;
            network.ConnectionStatusChanged -= OnConnectionStatusChanged;
            
            if (network.Host)
                network.StopHost();
            else
                network.StopClient();
        }

        private void OnConnectionStatusChanged(object sender, MultiplayerNetwork.ConnectionStatus e)
        {
            if (e == MultiplayerNetwork.ConnectionStatus.Disconnected)
            {
                UserMessageManager.Dispatch("Lost connection to the lobby!", 5f);
                MultiplayerNetwork.Instance.StopClient();
                StateMachine.PopState();
            }
        }
    }
}
