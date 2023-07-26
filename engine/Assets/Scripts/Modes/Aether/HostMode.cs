using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Aether.Lobby;
using System.Collections.Generic;
using System.Threading.Tasks;

#nullable enable

public class HostMode : IMode {

    private const string SERVER_IP = "127.0.0.1";
    
    private LobbyServer? _server;
    private LobbyClient? _hostClient;

    public IReadOnlyCollection<LobbyClientInformation> AllClients
        => _server?.Clients ?? new List<LobbyClientInformation>(1).AsReadOnly();

    public bool IsServerAlive => _server != null;

    public void Start() {
        _server = new LobbyServer();

        DynamicUIManager.CreateModal<InitLobbyConnection>(SERVER_IP);
        SimulationRunner.OnGameObjectDestroyed += End;
    }

    public void StartHostClient(string username) {
        _hostClient = new LobbyClient(SERVER_IP, username);
    }

    public void End() {
        if (_server == null) {
            return;
        }

        Engine.ModuleLoader.Api.ToastLogger.SetEnabled(false);
        _server.Dispose();
        _server = null;
        Engine.ModuleLoader.Api.ToastLogger.SetEnabled(true);
    }

    public void KillServer() {
        if (_server != null) {
            _server.Dispose();
            _server = null;
        }
    }

    public void Update() {}

    public void OpenMenu() {}

    public void CloseMenu() {}
}
