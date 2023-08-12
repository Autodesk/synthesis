using System;
using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Aether.Lobby;
using SynthesisAPI.Aether;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Google.Protobuf;
using Synthesis.Import;
using Synthesis.Util;
using SynthesisAPI.Utilities;

#nullable enable

public class HostMode : IMode {

    private const string SERVER_IP = "127.0.0.1";
    
    private LobbyServer? _server;
    private LobbyClient? _hostClient;
    public LobbyClient? HostClient => _hostClient;

    // Key: DataGuid, Value: MirabufPath
    private readonly Dictionary<ulong, (SynthesisDataDescriptor descriptor, string pathToData)> _shareableMirafiles = new();

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

        GatherAndUploadDataDescriptions();
    }

    public void GatherAndUploadDataDescriptions() {
        if (!(_hostClient?.IsAlive ?? false)) {
            Logger.Log("Host is either null or no longer alive");
            return;
        }

        LinkedList<(SynthesisDataDescriptor descriptor, string path)> dataPairings = new();

        MirabufLive.GetFieldFiles().ForEach(x => dataPairings.AddLast(
            (
                new SynthesisDataDescriptor {
                    Name = Path.GetFileNameWithoutExtension(x),
                    Description = "Field"
                },
                x
            )));
        MirabufLive.GetRobotFiles().ForEach(x => dataPairings.AddLast(
            (
                new SynthesisDataDescriptor {
                    Name = Path.GetFileNameWithoutExtension(x),
                    Description = "Robot"
                },
                x
            )));
        
        Logger.Log($"Data to Upload: {dataPairings.Count}");
        
        dataPairings.ForEach(x => {
            var makeDataAvailableTask = _hostClient!.MakeDataAvailable(x.descriptor);
            makeDataAvailableTask.Wait();
            var updatedDescriptor = makeDataAvailableTask.Result;
            Logger.Log($"File Added: OWNER [{updatedDescriptor?.Owner ?? 666}], GUID [{updatedDescriptor?.Guid ?? 666}]");
            _shareableMirafiles.Add(updatedDescriptor.Guid, (updatedDescriptor, x.path));
        });
    }

    public (Task<bool> task, AtomicReadOnly<NetworkTaskStatus>? status) UploadData() {
        if (!(_hostClient?.IsAlive ?? false))
            return (Task.FromResult(false), null);

        Atomic<NetworkTaskStatus> status = new Atomic<NetworkTaskStatus>(new NetworkTaskStatus {
            Progress = 0f,
            Message = ""
        });
        return (Task<bool>.Factory.StartNew(() => {

            var statusAtomic = status;
            var statusData = statusAtomic.Value;

            statusData.Message = "Gathering Selections...";
            statusAtomic.Value = statusData;

            var hostGuid = _hostClient!.Guid;

            var lobbyInfoTask = _hostClient!.GetLobbyInformation();
            lobbyInfoTask.Wait();
            var lobbyInfo = lobbyInfoTask.Result.GetResult()!.FromGetLobbyInformation.LobbyInformation;
            HashSet<ulong> guidsToUpload = new HashSet<ulong>();
            lobbyInfo.Clients.ForEach(x => x.Selections.ForEach(y => {
                if (y.Value.DataOwner == hostGuid) {
                    guidsToUpload.Add(y.Value.DataGuid);
                }
            }));

            float counter = 0;
            guidsToUpload.ForEach(x => {
                var dataInfo = _shareableMirafiles[x];

                statusData.Message = $"Uploading '{dataInfo.descriptor.Name}'...";
                statusData.Progress = counter / guidsToUpload.Count;
                statusAtomic.Value = statusData;
                counter++;

                ReadOnlySpan<byte> raw = File.ReadAllBytes(dataInfo.pathToData);

                SynthesisData data = new() {
                    Description = dataInfo.descriptor,
                    Buffer = ByteString.CopyFrom(raw)
                };
                
                Logger.Log($"Uploading Data: [{data.Description.Owner}:{data.Description.Guid}] {data.Description.Name}");

                _hostClient!.UploadData(data);
            });

            statusData.Progress = 1f;
            statusAtomic.Value = statusData;

            return true;

        }), status.AsReadOnly());
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
