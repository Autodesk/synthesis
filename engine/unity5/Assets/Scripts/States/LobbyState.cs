//﻿using Synthesis.FSM;
//using Synthesis.GUI;
//using Synthesis.Network;
//using Synthesis.Utils;
//using System;
//using System.Linq;
//using System.Net;
//using System.Reflection;
//using UnityEngine;
//using UnityEngine.Networking;
//using UnityEngine.UI;

//namespace Synthesis.States
//{
//    public class LobbyState : State
//    {
//        private readonly bool host;
//        private string lobbyCode;

//        private GameObject connectingPanel;
//        private GameObject startButton;
//        private Button fieldButton;
//        private Text lobbyCodeText;
//        private Text fieldText;
//        private Text readyText;

//        /// <summary>
//        /// Initializes a new <see cref="LobbyState"/> instance.
//        /// </summary>
//        /// <param name="lobbyCode">The code used to join the lobby, or null if this lobby is run by
//        /// the host.</param>
//        public LobbyState(string lobbyCode)
//        {
//            host = string.IsNullOrEmpty(lobbyCode);
//            this.lobbyCode = lobbyCode;
//        }

//        /// <summary>
//        /// Starts the <see cref="LobbyState"/>.
//        /// </summary>
//        public override void Start()
//        {
//            connectingPanel = Auxiliary.FindGameObject("ConnectingPanel");
//            startButton = Auxiliary.FindGameObject("StartButton");
//            fieldButton = GameObject.Find("FieldButton").GetComponent<Button>();
//            lobbyCodeText = GameObject.Find("LobbyCodeText").GetComponent<Text>();
//            fieldText = GameObject.Find("FieldButton").GetComponent<Text>();
//            readyText = GameObject.Find("ReadyText").GetComponent<Text>();

//            connectingPanel.SetActive(false);
//            startButton.SetActive(false);

//            MultiplayerNetwork network = MultiplayerNetwork.Instance;

//            if (host)
//            {
//                lobbyCodeText.text = "Lobby Code: " + (lobbyCode = IPCrypt.Encrypt(network.networkAddress = GetLocalIP()));

//                if (network.StartHost(null, network.maxConnections) == null)
//                {
//                    UserMessageManager.Dispatch("Could not host a lobby on this network!", 5f);
//                    StateMachine.ChangeState(new HostJoinState());
//                    return;
//                }

//                GameObject matchManager = (GameObject)UnityEngine.Object.Instantiate(Resources.Load("Prefabs/MatchManager"));
//                NetworkServer.Spawn(matchManager);
//            }
//            else
//            {
//                lobbyCodeText.text = "Lobby Code: " + lobbyCode;
//                fieldButton.interactable = false;
//                connectingPanel.SetActive(true);

//                network.networkAddress = IPCrypt.Decrypt(lobbyCode);
//                network.ClientConnectionChanged += OnClientConnectionChanged;
//                network.StartClient();
//            }
//        }

//        /// <summary>
//        /// Updates the robot name when this state is resumed.
//        /// </summary>
//        public override void Resume()
//        {
//            if (PlayerIdentity.LocalInstance != null)
//                PlayerIdentity.LocalInstance.CmdSetRobotName(PlayerPrefs.GetString("simSelectedRobotName"));

//            if (host && MatchManager.Instance != null)
//                MatchManager.Instance.FieldName = PlayerPrefs.GetString("simSelectedFieldName");
//        }

//        /// <summary>
//        /// Updates the UI elements of the lobby.
//        /// </summary>
//        public override void OnGUI()
//        {
//            if (MatchManager.Instance != null)
//                fieldText.text = "Field: " + MatchManager.Instance.FieldName;

//            if (host)
//                startButton.SetActive(UnityEngine.Object.FindObjectsOfType<PlayerIdentity>().All(p => p.ready));

//            if (PlayerIdentity.LocalInstance != null)
//                readyText.text = PlayerIdentity.LocalInstance.ready ?
//                    "UNREADY" : "READY!";
//        }

//        /// <summary>
//        /// Copies the lobby code to the clipboard when the lobby code text is pressed.
//        /// </summary>
//        public void OnLobbyCodeTextClicked()
//        {
//            GUIUtility.systemCopyBuffer = lobbyCode;
//            UserMessageManager.Dispatch("Lobby code copied to clipboard!", 8f);
//        }

//        /// <summary>
//        /// Launches a new <see cref="LoadFieldState"/> when the field button is pressed.
//        /// </summary>
//        public void OnFieldButtonClicked()
//        {
//            StateMachine.PushState(new LoadFieldState());
//        }

//        /// <summary>
//        /// Launches the load robot state when the robot button is pressed.
//        /// </summary>
//        public void OnRobotButtonClicked()
//        {
//            if (connectingPanel.activeSelf)
//                return;

//            StateMachine.PushState(new LoadRobotState());
//        }

//        /// <summary>
//        /// Sends a ready signal to the server.
//        /// </summary>
//        public void OnReadyButtonClicked()
//        {
//            if (connectingPanel.activeSelf)
//                return;

//            PlayerIdentity.LocalInstance.CmdSetReady(!PlayerIdentity.LocalInstance.ready);
//        }

//        /// <summary>
//        /// Launches a new <see cref="GatheringResourcesState"/> on each client instance.
//        /// </summary>
//        public void OnStartButtonClicked()
//        {
//            AnalyticsManager.GlobalInstance.LogTimingAsync(AnalyticsLedger.TimingCatagory.Multiplater,
//                AnalyticsLedger.TimingVarible.Customizing,
//                AnalyticsLedger.TimingLabel.MultiplayerLobbyMenu);
//            AnalyticsManager.GlobalInstance.LogEventAsync(AnalyticsLedger.EventCatagory.MultiplayerSimulator,
//                AnalyticsLedger.EventAction.Start,
//                "",
//                AnalyticsLedger.getMilliseconds().ToString());

//            MatchManager.Instance.syncing = true;
//            MatchManager.Instance.AwaitPushState<FetchingMetadataState>();
//        }

//        /// <summary>
//        /// Ends the <see cref="LobbyState"/>.
//        /// </summary>
//        public void OnBackButtonClicked()
//        {
//            MultiplayerNetwork network = MultiplayerNetwork.Instance;
//            network.ClientConnectionChanged -= OnClientConnectionChanged;

//            if (network.Host)
//                network.StopHost();
//            else
//                network.StopClient();

//            StateMachine.ChangeState(new HostJoinState());
//        }

//        /// <summary>
//        /// Exits the <see cref="LobbyState"/> if the connection has been lost.
//        /// </summary>
//        /// <param name="sender"></param>
//        /// <param name="e"></param>
//        private void OnClientConnectionChanged(object sender, MultiplayerNetwork.ConnectionStatus e)
//        {
//            switch (e)
//            {
//                case MultiplayerNetwork.ConnectionStatus.Connected:
//                    connectingPanel.SetActive(false);
//                    break;
//                case MultiplayerNetwork.ConnectionStatus.Disconnected:
//                    UserMessageManager.Dispatch("Lost connection to the lobby!", 5f);
//            }
//        }
//    }
//}