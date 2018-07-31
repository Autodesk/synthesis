using Synthesis.States;
using System;
using UnityEngine.Networking;

namespace Synthesis.Network
{
    public class MultiplayerNetwork : NetworkManager
    {
        public const int ReliableSequencedChannel = 0;

        public enum ConnectionStatus
        {
            Connected,
            Disconnected,
        }

        public static MultiplayerNetwork Instance => singleton as MultiplayerNetwork;
        
        public MultiplayerState State { get; set; }

        public int ConnectionID { get; private set; }

        public bool Host { get; private set; }

        public event EventHandler<ConnectionStatus> ClientConnectionChanged;

        //bool awaitingFieldLoad;

        private void Start()
        {
            Host = false;
            //awaitingFieldLoad = false;
        }

        public override void OnStartHost()
        {
            base.OnStartHost();
            Host = true;
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            base.OnClientConnect(conn);

            ConnectionID = conn.connectionId;
            ClientConnectionChanged?.Invoke(this, ConnectionStatus.Connected);

            ClientScene.AddPlayer(0);
        }

        public override void OnClientDisconnect(NetworkConnection conn)
        {
            base.OnClientDisconnect(conn);

            ClientConnectionChanged?.Invoke(this, ConnectionStatus.Disconnected);
        }

        public override void OnServerConnect(NetworkConnection conn)
        {
            base.OnServerConnect(conn);
        }

        public override void OnServerDisconnect(NetworkConnection conn)
        {
            NetworkServer.DestroyPlayersForConnection(conn);
        }

        public override void OnStartClient(NetworkClient client)
        {
            //if (host)
            //{
            //    if (!State.LoadField(PlayerPrefs.GetString("simSelectedField"), host))
            //    {
            //        AppModel.ErrorToMenu("Could not load field: " + PlayerPrefs.GetString("simSelectedField") + "\nHas it been moved or deleted?)");
            //        return;
            //    }
            //}
            //else
            //{
            //    awaitingFieldLoad = true;
            //}
        }

        private void Update()
        {
            //if (awaitingFieldLoad && Resources.FindObjectsOfTypeAll<NetworkElement>().Length > 1)
            //{
            //    awaitingFieldLoad = false;

            //    if (!State.LoadField(PlayerPrefs.GetString("simSelectedField"), host))
            //    {
            //        AppModel.ErrorToMenu("Could not load field: " + PlayerPrefs.GetString("simSelectedField") + "\nHas it been moved or deleted?)");
            //        return;
            //    }
            //}
        }
    }
}
