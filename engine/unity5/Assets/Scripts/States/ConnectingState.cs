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
        private readonly string lobbyCode;
        private readonly string playerTag;
        private readonly string networkAddress;

        private bool connected;

        /// <summary>
        /// Initializes a new <see cref="ConnectingState"/>.
        /// </summary>
        /// <param name="lobbyCode"></param>
        /// <param name="playerTag"></param>
        /// <param name="networkAddress"></param>
        public ConnectingState(string lobbyCode, string playerTag, string networkAddress)
        {
            this.lobbyCode = lobbyCode;
            this.playerTag = playerTag;
            this.networkAddress = networkAddress;
        }

        /// <summary>
        /// Starts the <see cref="ConnectingState"/>.
        /// </summary>
        public override void Start()
        {
            connected = false;

            MultiplayerNetwork network = MultiplayerNetwork.Instance;
            network.networkAddress = networkAddress;
            network.ClientConnectionChanged += OnConnectionStatusChanged;
            network.StartClient();
        }

        /// <summary>
        /// Ends the <see cref="ConnectingState"/>.
        /// </summary>
        public override void End()
        {
            MultiplayerNetwork network = MultiplayerNetwork.Instance;
            network.ClientConnectionChanged -= OnConnectionStatusChanged;

            if (!connected)
                network.StopClient();
        }

        /// <summary>
        /// Proceeds to a new <see cref="LobbyState"/> if the connection is successful,
        /// otherwise returns to the previous <see cref="State"/>.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="status"></param>
        private void OnConnectionStatusChanged(object sender, MultiplayerNetwork.ConnectionStatus status)
        {
            switch (status)
            {
                case MultiplayerNetwork.ConnectionStatus.Connected:
                    connected = true;
                    StateMachine.ChangeState(new LobbyState(false, lobbyCode, playerTag), false);
                    break;
                case MultiplayerNetwork.ConnectionStatus.Disconnected:
                    UserMessageManager.Dispatch("Unable to connect to the lobby!", 5f);
                    StateMachine.PopState();
                    break;
            }
        }
    }
}
