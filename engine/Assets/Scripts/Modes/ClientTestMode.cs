using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.Rendering.UI;

public class ClientTestMode : IMode {

    private Task _connectTask;

    // Start is called before the first frame update
    public void Start() {
        _instance = this;
        
        // _connectTask = NetManager.ClientManager.Instance.Connect();
        DynamicUIManager.CreateModal<ClientTestModal>();
    }

    // Update is called once per frame
    public void Update() {

        if (_connectTask != null && _connectTask.Status == TaskStatus.RanToCompletion) {
            ClientTestModal.TrySetStatus("Connected");
        } 
        
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
