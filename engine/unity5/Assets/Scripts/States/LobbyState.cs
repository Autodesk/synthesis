using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Network;
using UnityEngine;
using UnityEngine.UI;

namespace Synthesis.States
{
    public class LobbyState : State
    {
        private readonly bool host;
        private readonly string lobbyCode;
        private readonly string playerTag;

        /// <summary>
        /// Initializes a new <see cref="LobbyState"/> instance.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="lobbyCode"></param>
        /// <param name="playerTag"></param>
        public LobbyState(bool host, string lobbyCode, string playerTag)
        {
            this.host = host;
            this.lobbyCode = lobbyCode;
            this.playerTag = playerTag;
        }

        /// <summary>
        /// Starts the <see cref="LobbyState"/>.
        /// </summary>
        public override void Start()
        {
            PlayerIdentity.DefaultLocalPlayerTag = playerTag;

            GameObject.Find("LobbyCodeText").GetComponent<Text>().text = "Lobby Code: " + lobbyCode;

            if (!host)
            {
                MultiplayerNetwork network = MultiplayerNetwork.Instance;
                network.ClientConnectionChanged += OnClientConnectionChanged;
            }
        }

        /// <summary>
        /// Ends the <see cref="LobbyState"/>.
        /// </summary>
        public override void End()
        {
            MultiplayerNetwork network = MultiplayerNetwork.Instance;
            network.ClientConnectionChanged -= OnClientConnectionChanged;
            
            if (network.Host)
                network.StopHost();
            else
                network.StopClient();
        }

        /// <summary>
        /// Exits the <see cref="LobbyState"/> if the connection has been lost.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClientConnectionChanged(object sender, MultiplayerNetwork.ConnectionStatus e)
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
