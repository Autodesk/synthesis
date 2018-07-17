using Synthesis.FSM;
using Synthesis.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

namespace Synthesis.States
{
    public class HostJoinState : State
    {
        public void OnHostLobbyButtonPressed()
        {
            MultiplayerNetwork network = NetworkManager.singleton as MultiplayerNetwork;
            string ip = GetLocalIP();

            network.networkAddress = ip;
            network.StartHost();
            
            StateMachine.PushState(new LobbyState(true, IPCrypt.Encrypt(ip)));
        }

        public void OnJoinLobbyButtonPressed()
        {
            StateMachine.PushState(new EnterInfoState());
        }

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
