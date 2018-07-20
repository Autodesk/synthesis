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

        private readonly LoadRobotState loadRobotState;

        private bool robotSelected;

        private Text readyText;

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

            loadRobotState = new LoadRobotState();
        }

        /// <summary>
        /// Initializes fields local to this state.
        /// </summary>
        public override void Awake()
        {
            robotSelected = false;
        }

        /// <summary>
        /// Starts the <see cref="LobbyState"/>.
        /// </summary>
        public override void Start()
        {
            readyText = GameObject.Find("ReadyText").GetComponent<Text>();

            PlayerIdentity.DefaultLocalPlayerTag = playerTag;

            GameObject.Find("LobbyCodeText").GetComponent<Text>().text = "Lobby Code: " + lobbyCode;

            if (!host)
            {
                MultiplayerNetwork network = MultiplayerNetwork.Instance;
                network.ClientConnectionChanged += OnClientConnectionChanged;
            }

            StateMachine.PushState(loadRobotState);
        }

        /// <summary>
        /// Updates the robot name when this state is resumed.
        /// </summary>
        public override void Resume()
        {
            if (loadRobotState.RobotChosen)
            {
                robotSelected = true;
                PlayerIdentity.LocalInstance.RobotName = PlayerPrefs.GetString("simSelectedRobotName");
            }
            else if (!robotSelected)
            {
                StateMachine.PopState();
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
        /// Launches the load robot state when the robot button is pressed.
        /// </summary>
        public void OnRobotButtonPressed()
        {
            StateMachine.PushState(loadRobotState);
        }

        /// <summary>
        /// Sends a ready signal to the server.
        /// </summary>
        public void OnReadyButtonPressed()
        {
            readyText.text = (PlayerIdentity.LocalInstance.Ready = !PlayerIdentity.LocalInstance.Ready) ?
                "UNREADY" : "READY!";
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
                StateMachine.ChangeState(new HostJoinState());
            }
        }
    }
}
