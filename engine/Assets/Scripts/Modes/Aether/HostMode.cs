using System;
using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Aether.Lobby;
using SynthesisAPI.Aether;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
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

    public NetworkTask<bool> StartHostClient(string username) {
        _hostClient = new LobbyClient(SERVER_IP, username);

        return GatherAndUploadDataDescriptions();
    }

    public NetworkTask<bool> GatherAndUploadDataDescriptions() {
        if (!(_hostClient?.IsAlive ?? false)) {
            return NetworkTask<bool>.FromResult(false, "Host is either null or no longer alive");
        }

        var status = new Atomic<NetworkTaskStatus>(new NetworkTaskStatus {
            Progress = 0f,
            Message = ""
        });
        var task = Task<bool>.Factory.StartNew(() => {
            
            var statusAtomic = status;
            var statusData = statusAtomic.Value;
            
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

            int count = 0;
            dataPairings.ForEach(x => {
                count++;

                statusData.Progress = count / (float)dataPairings.Count;
                statusData.Message = $"Making '{x.descriptor.Name}' available...";
                statusAtomic.Value = statusData;
                
                var makeDataAvailableTask = _hostClient!.MakeDataAvailable(x.descriptor);
                makeDataAvailableTask.Wait();
                Thread.Sleep(200);
                var updatedDescriptor = makeDataAvailableTask.Result;
                _shareableMirafiles.Add(updatedDescriptor!.Guid, (updatedDescriptor, x.path));
            });

            return true;
        });
        
        return new NetworkTask<bool>(task, status);
    }

    public NetworkTask<bool> UploadData() {
        if (!(_hostClient?.IsAlive ?? false))
            return NetworkTask<bool>.FromResult(false, "Host is either null or no longer alive");

        var status = new Atomic<NetworkTaskStatus>(new NetworkTaskStatus {
            Progress = 0f,
            Message = ""
        });
        var task = Task<bool>.Factory.StartNew(() => {

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

                counter++;
                statusData.Message = $"Uploading '{dataInfo.descriptor.Name}'...";
                statusData.Progress = counter / guidsToUpload.Count;
                statusAtomic.Value = statusData;

                ReadOnlySpan<byte> raw = File.ReadAllBytes(dataInfo.pathToData);
                SHA256 sha = SHA256.Create();
                var stream = new ByteStream(raw.ToArray().ToList().GetEnumerator(), raw.Length);
                string checksum = Convert.ToBase64String(sha.ComputeHash(stream));

                SynthesisData data = new() {
                    Description = dataInfo.descriptor,
                    Buffer = ByteString.CopyFrom(raw)
                };
                
                Logger.Log($"Uploading Data: [{data.Description.Owner}:{data.Description.Guid}] {data.Description.Name}");

                var uploadTask = _hostClient!.UploadData(data);
                uploadTask.Wait();

                if (uploadTask.Result?.Equals(checksum) ?? false) {
                    Logger.Log("Upload Succeeded");
                } else {
                    Logger.Log("Upload Failed");
                }
            });

            statusData.Progress = 1f;
            statusAtomic.Value = statusData;

            return true;

        });

        return new NetworkTask<bool>(task, status);
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
