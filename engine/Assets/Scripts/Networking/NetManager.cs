using SynthesisAPI.Aether.Lobby;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using UnityEngine;

public class NetManager {

    public class ClientManager {

        private LobbyClient _lobbyClient;
        
        private ClientManager() {
            _lobbyClient = new LobbyClient("unknown01", 21345, IPAddress.Parse("127.0.0.1"));
            _lobbyClient.ConnectToServer();
        }

        public Task Connect() => _lobbyClient.ConnectToServer();

        private static ClientManager _instance = null;
        public static ClientManager Instance {
            get {
                _instance = _instance ?? new ClientManager();
                return _instance;
            }
        }

    }
    
    public class ServerManager {
        
        private ServerManager() {
            LobbyServer.Instance.InitServer();
        }

        private static ServerManager _instance = null;
        public static ServerManager Instance {
            get {
                _instance = _instance ?? new ServerManager();
                return _instance;
            }
        }

    }

}
