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
        /// <summary>
        /// Launches a new <see cref="LobbyState"/> as the host.
        /// </summary>
        public void OnHostLobbyButtonPressed()
        {
            MultiplayerNetwork network = NetworkManager.singleton as MultiplayerNetwork;
            string ip = GetLocalIP();

            network.networkAddress = ip;
            network.StartHost();
            
            StateMachine.PushState(new LobbyState(true, IPCrypt.Encrypt(ip), "Host"));
        }

        /// <summary>
        /// Launches a new <see cref="EnterInfoState"/>.
        /// </summary>
        public void OnJoinLobbyButtonPressed()
        {
            StateMachine.PushState(new EnterInfoState());
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
