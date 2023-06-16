using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Aether.Lobby;
using UnityEngine;

public class ServerTestMode : IMode {

    private LobbyServer _server;
    
    public void Start() {

        _server = new LobbyServer();
        
        // _connectTask = NetManager.ClientManager.Instance.Connect();
        DynamicUIManager.CreateModal<ServerTestModal>();
    }

    // Update is called once per frame
    public void Update() {
        
    }
    
    public void End() {
        
    }

    public void OpenMenu() { }

    public void CloseMenu() { }
    
}
