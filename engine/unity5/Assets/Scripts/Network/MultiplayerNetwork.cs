using Synthesis.States;
using Synthesis.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Synthesis.Network
{
    public class MultiplayerNetwork : NetworkManager
    {
        public MultiplayerState State { get; set; }

        public int ConnectionID { get; private set; }

        bool host;
        bool awaitingFieldLoad;

        private void Awake()
        {
            host = false;
            awaitingFieldLoad = false;
        }

        public override void OnStartHost()
        {
            base.OnStartHost();

            host = true;
        }

        public override void OnClientConnect(NetworkConnection conn)
        {
            ConnectionID = conn.connectionId;
            ClientScene.Ready(conn);
            ClientScene.AddPlayer(0);
        }

        public override void OnStartClient(NetworkClient client)
        {
            if (host)
            {
                if (!State.LoadField(PlayerPrefs.GetString("simSelectedField"), host))
                {
                    AppModel.ErrorToMenu("Could not load field: " + PlayerPrefs.GetString("simSelectedField") + "\nHas it been moved or deleted?)");
                    return;
                }
            }
            else
            {
                awaitingFieldLoad = true;
            }
        }

        private void Update()
        {
            if (awaitingFieldLoad && Resources.FindObjectsOfTypeAll<NetworkElement>().Length > 1)
            {
                awaitingFieldLoad = false;

                if (!State.LoadField(PlayerPrefs.GetString("simSelectedField"), host))
                {
                    AppModel.ErrorToMenu("Could not load field: " + PlayerPrefs.GetString("simSelectedField") + "\nHas it been moved or deleted?)");
                    return;
                }
            }
        }
    }
}
