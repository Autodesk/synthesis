using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.Networking.NetworkSystem;

namespace Synthesis.States
{
    public class ConnectingState : State
    {
        // TODO: Handle disconnecting, reconnecting, etc.

        private readonly string lobbyCode;
        private readonly string networkAddress;

        public ConnectingState(string lobbyCode, string networkAddress)
        {
            this.lobbyCode = lobbyCode;
            this.networkAddress = networkAddress;
        }

        public override void Start()
        {
            MultiplayerNetwork network = NetworkManager.singleton as MultiplayerNetwork;
            network.networkAddress = networkAddress;
            network.ConnectionStatusChanged += OnConnectionStatusChanged;
            network.StartClient();
        }

        private void OnError(NetworkMessage netMsg)
        {
            netMsg.ReadMessage<ErrorMessage>();
            UserMessageManager.Dispatch("Unable to connect to the lobby!", 5f);
            StateMachine.PopState();
        }

        private void OnConnectionStatusChanged(object sender, MultiplayerNetwork.ConnectionStatus status)
        {
            switch (status)
            {
                case MultiplayerNetwork.ConnectionStatus.Connected:
                    StateMachine.ChangeState(new LobbyState(false, lobbyCode), false);
                    break;
                case MultiplayerNetwork.ConnectionStatus.Failed:
                    UserMessageManager.Dispatch("Unable to connect to the lobby!", 5f);
                    StateMachine.PopState();
                    break;
            }
        }
    }
}
