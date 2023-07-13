#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Synthesis.Physics;
using Synthesis.Runtime;
using Synthesis.UI.Dynamic;
using SynthesisAPI.Aether.Lobby;
using SynthesisAPI.Controller;
using SynthesisAPI.Simulation;
using UnityEngine;

public class ServerTestMode : IMode {
    private LobbyServer? _server;
    // clang-format off
    public LobbyServer? Server               => _server;
    private readonly LobbyClient?[] _clients = new LobbyClient?[2];
    public LobbyClient?[] Clients            => _clients;
    // clang-format on

    private RobotSimObject _host;
    private RobotSimObject _ghost;

    public IReadOnlyCollection<string> ClientInformation => _server?.Clients ?? new List<string>();
    
    public void Start() {
        DynamicUIManager.CloseActiveModal();
        // MainHUD.AddItemToDrawer("Multibot", b => DynamicUIManager.CreatePanel<RobotSwitchPanel>());
        
        _server = new LobbyServer();

        // DynamicUIManager.CreateModal<ServerTestModal>();
        // TODO remove and allow user to choose robot
        string dozer = "Dozer_v9.mira";
        string tmm   = "Team 2471 (2018)_v5.mira";
        RobotSimObject.SpawnRobot(AddRobotModal.ParsePath("$appdata/Autodesk/Synthesis/Mira/" + dozer, '/'), new Vector3(-2, 1, 0), Quaternion.Euler(0, 0, 0), false);
        _host                   = RobotSimObject.GetCurrentlyPossessedRobot();
        _host.RobotNode.name    = "host";
        _host.BehavioursEnabled = false;
        RobotSimObject.SpawnRobot(AddRobotModal.ParsePath("$appdata/Autodesk/Synthesis/Mira/" + dozer, '/'), new Vector3(2, 1, 0), Quaternion.Euler(0, 0, 0), false);
        _ghost                = RobotSimObject.GetCurrentlyPossessedRobot();
        _ghost.RobotNode.name = "ghost";
        _ghost.DriversEnabled = false;
        _ghost.Freeze();
        
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

    public void Update() {
        var transformsList = new List<ServerTransforms>();
        if (_ghost is null || _host is null) return;
        if (_ghost.Client is null || _host.Client is null) return;

        var ghostTransform = new ServerTransforms() {
            Guid = _ghost.Client.Guid.Value
        };

        foreach (var node in _host.AllRigidbodies) {
            Matrix4x4 m = node.transform.localToWorldMatrix;
            ServerTransformData data = (ServerTransformData)m;
            ghostTransform.Transforms.Add(node.name, data);
        }
        transformsList.Add(ghostTransform);
        
        _host.Client.UpdateTransforms(transformsList).ContinueWith((x, o) => {
            var msg = x.Result.GetResult();
            msg?.FromControllableStates.AllUpdates.ForEach(update => {
                // TODO handle guid
                update.UpdatedSignals.ForEach(signal => {
                    // TODO choose robot based on client guid rather than hardcoding host
                    SimulationManager.Drivers[_host.Name].ForEach(driver => {
                        driver.State.SignalMap.Values.Where(sd => sd.SignalGuid == signal.SignalGuid).ToList()[0].Value = signal.Value;
                    });
                });
            });
        }, null);
    }

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
