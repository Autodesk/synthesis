using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Aether.Lobby;
using System.Collections.Generic;
using System.Threading.Tasks;

public class ServerHostingMode : IMode {
    private LobbyServer _server;

    public IReadOnlyCollection<string> ClientInformation =>
        _server.Clients == null ? new List<string>() : _server.Clients;

    public bool IsServerAlive => _server != null;

    public void Start() {
        _server = new LobbyServer();

        DynamicUIManager.CreateModal<ServerHostingModal>();
        SimulationRunner.OnGameObjectDestroyed += End;
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
