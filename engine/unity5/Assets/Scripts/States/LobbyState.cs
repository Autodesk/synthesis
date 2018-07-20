using Synthesis.FSM;
using Synthesis.GUI;
using Synthesis.Network;
using Synthesis.Utils;
using System.Net;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Synthesis.States
{
    public class LobbyState : State
    {
        private readonly string lobbyCode;

        private GameObject connectingPanel;
        private Button fieldButton;
        private Text fieldText;
        private Text readyText;

        /// <summary>
        /// Initializes a new <see cref="LobbyState"/> instance.
        /// </summary>
        /// <param name="lobbyCode">The code used to join the lobby, or null if this lobby is run by
        /// the host.</param>
        public LobbyState(string lobbyCode)
        {
            this.lobbyCode = lobbyCode;
        }

        /// <summary>
        /// Starts the <see cref="LobbyState"/>.
        /// </summary>
        public override void Start()
        {
            connectingPanel = Auxiliary.FindGameObject("ConnectingPanel");
            fieldButton = GameObject.Find("FieldButton").GetComponent<Button>();
            fieldText = GameObject.Find("FieldButton").GetComponent<Text>();
            readyText = GameObject.Find("ReadyText").GetComponent<Text>();

            connectingPanel.SetActive(false);

            MultiplayerNetwork network = MultiplayerNetwork.Instance;
            Text lobbyCodeText = GameObject.Find("LobbyCodeText").GetComponent<Text>();

            if (string.IsNullOrEmpty(lobbyCode))
            {
                lobbyCodeText.text = "Lobby Code: " + IPCrypt.Encrypt(network.networkAddress = GetLocalIP());

                if (network.StartHost() == null)
                {
                    UserMessageManager.Dispatch("Could not host a lobby on this network!", 5f);
                    StateMachine.ChangeState(new HostJoinState());
                    return;
                }
            }
            else
            {
                lobbyCodeText.text = "Lobby Code: " + lobbyCode;
                connectingPanel.SetActive(true);

                network.networkAddress = IPCrypt.Decrypt(lobbyCode);
                network.ClientConnectionChanged += OnClientConnectionChanged;
                network.StartClient();
            }
        }

        /// <summary>
        /// Updates the robot name when this state is resumed.
        /// </summary>
        public override void Resume()
        {
            if (PlayerIdentity.LocalInstance == null)
                return;

            PlayerIdentity.LocalInstance.RobotName = PlayerPrefs.GetString("simSelectedRobotName");

            if (PlayerIdentity.LocalInstance.IsHost)
                PlayerIdentity.LocalInstance.FieldName = PlayerPrefs.GetString("simSelectedFieldName");
        }

        /// <summary>
        /// Launches the load robot state when the robot button is pressed.
        /// </summary>
        public void OnRobotButtonPressed()
        {
            if (connectingPanel.activeSelf)
                return;

            StateMachine.PushState(new LoadRobotState());
        }

        /// <summary>
        /// Sends a ready signal to the server.
        /// </summary>
        public void OnReadyButtonPressed()
        {
            if (connectingPanel.activeSelf)
                return;

            readyText.text = (PlayerIdentity.LocalInstance.Ready = !PlayerIdentity.LocalInstance.Ready) ?
                "UNREADY" : "READY!";
        }

        /// <summary>
        /// Ends the <see cref="LobbyState"/>.
        /// </summary>
        public void OnBackButtonPressed()
        {
            MultiplayerNetwork network = MultiplayerNetwork.Instance;
            network.ClientConnectionChanged -= OnClientConnectionChanged;

            if (network.Host)
                network.StopHost();
            else
                network.StopClient();

            StateMachine.ChangeState(new HostJoinState());
        }

        /// <summary>
        /// Exits the <see cref="LobbyState"/> if the connection has been lost.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void OnClientConnectionChanged(object sender, MultiplayerNetwork.ConnectionStatus e)
        {
            switch (e)
            {
                case MultiplayerNetwork.ConnectionStatus.Connected:
                    connectingPanel.SetActive(false);
                    break;
                case MultiplayerNetwork.ConnectionStatus.Disconnected:
                    UserMessageManager.Dispatch("Lost connection to the lobby!", 5f);

                    MultiplayerNetwork network = MultiplayerNetwork.Instance;
                    network.ClientConnectionChanged -= OnClientConnectionChanged;
                    network.StopClient();

                    StateMachine.ChangeState(new HostJoinState());
                    break;
            }
        }

        /// <summary>
        /// Returns the local IP address of this machine.
        /// </summary>
        /// <returns></returns>
        private string GetLocalIP()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (IPAddress ip in host.AddressList)
                if (ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                    return ip.ToString();

            return string.Empty;
        }
    }
}
