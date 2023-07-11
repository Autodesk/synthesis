using System.Collections.Generic;
using System.Threading.Tasks;
using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Aether.Lobby;

#nullable enable

public class EmptyServerTestMode : IMode {
    private LobbyServer? _server;

    public IReadOnlyCollection<string> ClientInformation => _server?.Clients ?? new List<string>();
    public bool IsServerAlive => _server != null;

    public void Start() {
        _server = new LobbyServer();

        DynamicUIManager.CreateModal<EmptyServerTestModal>();

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
