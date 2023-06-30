#nullable enable

using System.Collections.Generic;
using System.Threading.Tasks;
using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Aether.Lobby;
    
public class ServerTestMode : IMode {
    private LobbyServer? _server;
    // clang-format off
    public LobbyServer? Server               => _server;
    private readonly LobbyClient?[] _clients = new LobbyClient?[2];
    public LobbyClient?[] Clients            => _clients;
    // clang-format on

    public IReadOnlyCollection<string> ClientInformation => _server?.Clients ?? new List<string>();

    public void Start() {
        _server = new LobbyServer();

        for (int i = 0; i < _clients.Length; i++) {
            int j = i;
            Task.Factory.StartNew(() => _clients[j] = new LobbyClient("127.0.0.1", $"Client {j}"));
        }

        DynamicUIManager.CreateModal<ServerTestModal>();

        SimulationRunner.OnGameObjectDestroyed += End;
    }

    public void KillClient(int i) {
        _clients[i]?.Dispose();
    }

    public void KillClients() {
        foreach (var client in _clients) {
            client?.Dispose();
        }
    }

    public void Update() {}

    public void End() {
        Engine.ModuleLoader.Api.ToastLogger.SetEnabled(false);
        KillClients();
        _server?.Dispose();
        _server = null;
        Engine.ModuleLoader.Api.ToastLogger.SetEnabled(true);
    }

    public void OpenMenu() {}

    public void CloseMenu() {}
}
