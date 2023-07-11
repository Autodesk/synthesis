using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;
using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Aether.Lobby;
using SynthesisAPI.Controller;
using SynthesisAPI.Utilities;
using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

public class ConnectToServerTestMode : IMode {
    private LobbyClient? _client;

    public bool IsClientAlive => _client == null ? false : _client.IsAlive;

    public Task<Result<LobbyMessage?, Exception>> UploadRobotData(DataRobot robot) {
        if (_client == null) {
            return Task.FromResult(new Result<LobbyMessage?, Exception>(new Exception("Client is null.")));
        } else {
            return _client.UploadRobotData(robot);
        }
    }

    public void Start() {
        Task.Factory.StartNew(() => _client = new LobbyClient("127.0.0.1", "Test Client")).ContinueWith(t => {
            if (t.IsFaulted) {
                Logger.Log("Failed to connect to server", LogLevel.Error);
            } else {
                Logger.Log("Test Client connected to server", LogLevel.Info);
            }
        });

        DynamicUIManager.CreateModal<ConnectToServerTestModal>();

        SimulationRunner.OnGameObjectDestroyed += End;
    }

    public void KillClient() {
        _client?.Dispose();
    }

    public void Update() {}

    public void End() {
        Engine.ModuleLoader.Api.ToastLogger.SetEnabled(false);
        _client?.Dispose();
        _client = null;
        Engine.ModuleLoader.Api.ToastLogger.SetEnabled(true);
    }

    public void OpenMenu() {}

    public void CloseMenu() {}
}
