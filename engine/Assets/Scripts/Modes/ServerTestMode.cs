using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Aether.Lobby;
using UnityEngine;

public class ServerTestMode : MonoBehaviour {
    
    // private Task _connectTask;

    // Start is called before the first frame update
    public void Start() {
        
        // _connectTask = NetManager.ClientManager.Instance.Connect();
        DynamicUIManager.CreateModal<ServerTestModal>();
        
        LobbyServer.Instance.InitServer();
    }

    // Update is called once per frame
    public void Update() {
        
    }
    
    public void End() {
        
    }

    public void OpenMenu() { }

    public void CloseMenu() { }

    private static ClientTestMode _instance = null;
    public static ClientTestMode Instance {
        get => _instance;
    }
    
}
