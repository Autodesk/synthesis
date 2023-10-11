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

public class ClientMode : IMode {
    
    private LobbyClient? _client;
    public LobbyClient? Client => _client;

    public void Start() {
        DynamicUIManager.CreateModal<InitLobbyConnection>(false, (string?)null);
        SimulationRunner.OnGameObjectDestroyed += End;
    }

    public void StartClient(string ip, string username) {
        _client = new LobbyClient(ip, username);
    }

    public void End() {
        if (_client == null) {
            return;
        }

        Engine.ModuleLoader.Api.ToastLogger.SetEnabled(false);
        _client.Dispose();
        _client = null;
        Engine.ModuleLoader.Api.ToastLogger.SetEnabled(true);
    }

    public void KillServer() {
        if (_client != null) {
            _client.Dispose();
            _client = null;
        }
    }

    public void Update() {}

    public void OpenMenu() {}

    public void CloseMenu() {}
}
