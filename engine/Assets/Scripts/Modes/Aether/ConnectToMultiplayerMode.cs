using System.Collections.Generic;
using System.Threading.Tasks;
using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Controller;
using SynthesisAPI.Utilities;
using SynthesisAPI.Aether.Lobby;
using System.IO;
using System.Linq;
using Google.Protobuf;

using Google.Protobuf;
using SynthesisAPI.Controller;
using SynthesisAPI.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Logger = SynthesisAPI.Utilities.Logger;

public class ConnectToMultiplayerMode : IMode {
    private const string SERVER_IP   = "127.0.0.1";
    private const string CLIENT_NAME = "Client 3.1415";
    // clang-format off
    private static readonly string[] _robotFolders = {
        "Mira",
        "Mira/Multiplayer"
    };
    // clang-format on
    private string _multiplayerRobotFolder => _robotFolders[1];

    private LobbyClient _client;

    public ClientConnectionState ConnectionState { get; private set; } = ClientConnectionState.Disconnected;
    public ClientActionState ActionState { get; private set; }         = ClientActionState.Idle;

    public List<string> GetAvailableRobots() {
        List<string> robots = new List<string>();

        foreach (string folder in _robotFolders) {
            string path = ParsePath(Path.Combine("$appdata/Autodesk/Synthesis", folder), '/');
            if (!Directory.Exists(path)) {
                Directory.CreateDirectory(path);
            }

            robots.AddRange(Directory.GetFiles(path).Where(x => Path.GetExtension(x).Equals(".mira")));
        }

        return robots;
    }

    public Task<Result<LobbyMessage?, Exception>> SelectRobot(string pathToRobotMira) {
        if (!File.Exists(pathToRobotMira)) {
            Logger.Log("Robot does not exist", LogLevel.Error);
            return Task.FromResult(new Result<LobbyMessage?, Exception>(new Exception("Robot file does not exist.")));
        }

        var robot = new DataRobot {
            Name = Path.GetFileNameWithoutExtension(pathToRobotMira),
            Data = ByteString.CopyFrom(File.ReadAllBytes(pathToRobotMira)),
            // Guid = _client.Guid
        };

        ActionState = ClientActionState.UploadingData;

        Task < Result < LobbyMessage ?, Exception >> task;
        try {
            task = _client.UploadRobotData(robot);
        } catch (Exception e) {
            Logger.Log("Failed to upload robot data", LogLevel.Error);
            ActionState = ClientActionState.Idle;
            return Task.FromResult(new Result<LobbyMessage?, Exception>(e));
        }

        ActionState = ClientActionState.Idle;
        return task;
    }

    public void Start() {
        DynamicUIManager.CreateModal<ConnectToMultiplayerModal>();
        SimulationRunner.OnGameObjectDestroyed += End;

        ConnectionState = ClientConnectionState.Connecting;
        Task.Factory.StartNew(() => _client = new LobbyClient(SERVER_IP, CLIENT_NAME)).ContinueWith(t => {
            if (t.IsFaulted) {
                Logger.Log("Failed to connect to server", LogLevel.Error);
                ConnectionState = ClientConnectionState.Disconnected;
            } else {
                Logger.Log("Test Client connected to server", LogLevel.Info);
                ConnectionState = ClientConnectionState.Connected;
            }
        });
    }

    public void RequestServerRobotData() {
        ActionState = ClientActionState.DownloadingData;

        _client.RequestServerRobotData().ContinueWith(t => {
            if (t.IsFaulted) {
                Logger.Log("Failed to request robot data from server", LogLevel.Error);
                ActionState = ClientActionState.Idle;
                return;
            }

            string root = ParsePath(Path.Combine("$appdata/Autodesk/Synthesis", _multiplayerRobotFolder), '/');

            foreach (DataRobot robot in _client.RobotsFromServer) {
                string path = Path.Combine(root, robot.Name + ".mira");
                var bytes   = robot.Data.ToByteArray();
                if (!File.Exists(path)) {
                    File.Create(path);
                }

                File.WriteAllBytes(path, bytes);
            }
        });

        ActionState = ClientActionState.Idle;
    }

    public void Update() {}

    public void End() {
        if (_client == null) {
            return;
        }

        Engine.ModuleLoader.Api.ToastLogger.SetEnabled(false);
        _client.Dispose();
        _client = null;
        Engine.ModuleLoader.Api.ToastLogger.SetEnabled(true);
    }

    public void OpenMenu() {}

    public void CloseMenu() {}

    public enum ClientConnectionState {
        Disconnected,
        Connecting,
        Connected
    }

    public enum ClientActionState {
        Idle,
        UploadingData,
        DownloadingData
    }

    private static string ParsePath(string p, char c) {
        string[] a = p.Split(c);
        string b   = "";
        for (int i = 0; i < a.Length; i++) {
            switch (a[i]) {
                case "$appdata":
                    b += System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData);
                    break;
                default:
                    b += a[i];
                    break;
            }

            if (i != a.Length - 1) {
                b += System.IO.Path.AltDirectorySeparatorChar;
            }
        }

        return b;
    }
}
