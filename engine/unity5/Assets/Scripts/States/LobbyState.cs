using Synthesis.FSM;
using Synthesis.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Synthesis.States
{
    public class LobbyState : State
    {
        private readonly bool host;
        private readonly string lobbyCode;

        public LobbyState(bool host, string lobbyCode)
        {
            this.host = host;
            this.lobbyCode = lobbyCode;
        }

        public override void Start()
        {
            GameObject.Find("LobbyCodeText").GetComponent<Text>().text = "Lobby Code: " + lobbyCode;
        }

        public override void End()
        {
            MultiplayerNetwork network = NetworkManager.singleton as MultiplayerNetwork;
            
            if (network.Host)
                network.StopHost();
            else
                network.StopClient();
        }
    }
}
