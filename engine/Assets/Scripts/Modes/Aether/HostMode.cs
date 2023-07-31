using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Aether.Lobby;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Synthesis.Import;

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

    public void UploadSelectionDescriptions() {
        if (_hostClient?.IsAlive ?? false)
            return;

        MirabufLive.GetFieldFiles().Select(x => new SynthesisDataDescriptor {
            Name = Path.GetFileNameWithoutExtension(x),
            Description = "Field"
        }).ForEach(x => _hostClient?.MakeDataAvailable(x));
        MirabufLive.GetRobotFiles().Select(x => new SynthesisDataDescriptor {
            Name = Path.GetFileNameWithoutExtension(x),
            Description = "Robot"
        }).ForEach(x => _hostClient?.MakeDataAvailable(x));
    }

    public void UploadSelections() {
        if (_hostClient?.IsAlive ?? false)
            return;

        _hostClient.GetLobbyInformation();
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
