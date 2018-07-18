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

        private bool connected;

        public ConnectingState(string lobbyCode, string networkAddress)
        {
            this.lobbyCode = lobbyCode;
            this.networkAddress = networkAddress;

        }

        public override void Start()
        {
            connected = false;

            MultiplayerNetwork network = MultiplayerNetwork.Instance;
            network.networkAddress = networkAddress;
            network.ConnectionStatusChanged += OnConnectionStatusChanged;
            network.StartClient();
        }

        public override void End()
        {
            MultiplayerNetwork network = MultiplayerNetwork.Instance;
            network.ConnectionStatusChanged -= OnConnectionStatusChanged;

            if (!connected)
                network.StopClient();
        }

        private void OnConnectionStatusChanged(object sender, MultiplayerNetwork.ConnectionStatus status)
        {
            switch (status)
            {
                case MultiplayerNetwork.ConnectionStatus.Connected:
                    connected = true;
                    StateMachine.ChangeState(new LobbyState(false, lobbyCode), false);
                    break;
                case MultiplayerNetwork.ConnectionStatus.Disconnected:
                    UserMessageManager.Dispatch("Unable to connect to the lobby!", 5f);
                    StateMachine.PopState();
                    break;
            }
        }
    }
}
