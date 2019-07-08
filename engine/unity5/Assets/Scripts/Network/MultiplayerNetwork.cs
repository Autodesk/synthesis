//using Synthesis.GUI;
//using Synthesis.States;
//using System;
//using UnityEngine.Networking;
//using UnityEngine.SceneManagement;

//namespace Synthesis.Network
//{
//    public class MultiplayerNetwork : NetworkManager
//    {
//        /// <summary>
//        /// Indicates the status of a connection.
//        /// </summary>
//        public enum ConnectionStatus
//        {
//            Connected,
//            Disconnected,
//        }

//        /// <summary>
//        /// The global instance of this class.
//        /// </summary>
//        public static MultiplayerNetwork Instance => singleton as MultiplayerNetwork;
        
//        /// <summary>
//        /// The active <see cref="MultiplayerState"/>.
//        /// </summary>
//        public MultiplayerState State { get; set; }

//        /// <summary>
//        /// The ID of this connection.
//        /// </summary>
//        public int ConnectionID { get; private set; }

//        /// <summary>
//        /// If true, this instance is the host.
//        /// </summary>
//        public bool Host { get; private set; }

//        /// <summary>
//        /// Called on the client when the connection status has changed.
//        /// </summary>
//        public event EventHandler<ConnectionStatus> ClientConnectionChanged;

//        /// <summary>
//        /// Initializes the properties of this instance.
//        /// </summary>
//        private void Start()
//        {
//            Host = false;
//        }

//        /// <summary>
//        /// Starts the host instance.
//        /// </summary>
//        public override void OnStartHost()
//        {
//            base.OnStartHost();
//            Host = true;
//        }

//        /// <summary>
//        /// Called when a client connects to the match.
//        /// </summary>
//        /// <param name="conn"></param>
//        public override void OnClientConnect(NetworkConnection conn)
//        {
//            base.OnClientConnect(conn);

//            ConnectionID = conn.connectionId;
//            ClientConnectionChanged?.Invoke(this, ConnectionStatus.Connected);

//            ClientScene.AddPlayer(0);
//        }

//        /// <summary>
//        /// Called when a client disconnects from a match.
//        /// </summary>
//        /// <param name="conn"></param>
//        public override void OnClientDisconnect(NetworkConnection conn)
//        {
//            if (!NetworkMultiplayerUI.Instance.Visible)
//            {
//                SceneManager.LoadScene(SceneManager.GetActiveScene().name);
//                ClientConnectionChanged?.Invoke(this, ConnectionStatus.Disconnected);
//            }
//            else
//            {
//                ClientConnectionChanged?.Invoke(this, ConnectionStatus.Disconnected);
//            }
//        }

//        /// <summary>
//        /// Called on the server when a new client connects.
//        /// </summary>
//        /// <param name="conn"></param>
//        public override void OnServerConnect(NetworkConnection conn)
//        {
//            base.OnServerConnect(conn);
//        }

//        /// <summary>
//        /// Called on the server when a client disconnects.
//        /// </summary>
//        /// <param name="conn"></param>
//        public override void OnServerDisconnect(NetworkConnection conn)
//        {
//            NetworkServer.DestroyPlayersForConnection(conn);
//            MatchManager.Instance.CancelSync();
//        }
//    }
//}
