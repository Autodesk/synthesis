using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Synthesis.UI.Dynamic;
using UnityEngine;
using UnityEngine.Rendering.UI;
using SynthesisAPI.Aether.Lobby;
using System.Text;

#nullable enable

public class ServerTestMode : IMode {
    private LobbyServer? _server;
    // private Task<LobbyClient>? _connectTask;
    private LobbyClient[]? _clients;

    public IReadOnlyCollection<string> ClientInformation => _server == null ? new List<string>() : _server.Clients;

    public void Start() {
        _server = new LobbyServer();

        //int clientCount = 10;

        //_clients = new LobbyClient[clientCount];

        //for (int i = 0; i < clientCount; i++) {
        //    int j = i;
        //    Task.Factory.StartNew(() => _clients[j] = new LobbyClient("127.0.0.1", $"Client {j}"));
        //}

        DynamicUIManager.CreateModal<ServerTestModal>();
    }

    public void KillClient(int i) {
        _clients![i].Dispose();
    }

    public void Update() {}

    public void End() {
        _server?.Dispose();
        _server = null;
    }

    public void OpenMenu() {}

    public void CloseMenu() {}
}
