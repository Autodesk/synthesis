#nullable enable

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
    
    public static FileStream LogFile { get; } = File.Create(@"../server.log");
    public static UTF8Encoding encoding { get; } = new UTF8Encoding(true);
    public static long CurrentTime {
        get => DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
    }
    
    public static void Log(string message) {
        // LogFile.Write(encoding.GetBytes($"[{CurrentTime}] [ENGINE] {message}\n"));
    }

    public void Start() {
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
        // _ghost.RobotNode.GetComponentsInChildren<Collider>().ForEach(c => c.enabled = false);
        _ghost.DriversEnabled = false;

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
        
        if (_ghost.Client is not null) {
            var ghostTransform = new ServerTransforms() {
                Guid = _ghost.Client.Guid.Value
            };

            Log("Adding ghost transforms");
            foreach (var node in _ghost.RobotNode.GetComponentsInChildren<Rigidbody>()) {
                Matrix4x4 m = node.transform.localToWorldMatrix;
                ServerTransformData data = new ServerTransformData() {
                    MatrixData = {
                        m[0, 0], m[0, 1], m[0, 2], m[0, 3],
                        m[1, 0], m[1, 1], m[1, 2], m[1, 3],
                        m[2, 0], m[2, 1], m[2, 2], m[2, 3],
                        m[3, 0], m[3, 1], m[3, 2], m[3, 3]
                    }
                };
                ghostTransform.Transforms.Add(node.name, data);
            }
            transformsList.Add(ghostTransform);
            Log("Done adding ghost transforms");
        }

        if (_host.Client is not null) {
            var hostTransform = new ServerTransforms() {
                Guid = _host.Client.Guid.Value
            };

            Log("Adding host transforms");
            foreach (var node in _host.RobotNode.GetComponentsInChildren<Rigidbody>()) {
                Matrix4x4 m = node.transform.localToWorldMatrix;
                ServerTransformData data = new ServerTransformData() {
                    MatrixData = {
                        m[0, 0], m[0, 1], m[0, 2], m[0, 3],
                        m[1, 0], m[1, 1], m[1, 2], m[1, 3],
                        m[2, 0], m[2, 1], m[2, 2], m[2, 3],
                        m[3, 0], m[3, 1], m[3, 2], m[3, 3]
                    }
                };
                hostTransform.Transforms.Add(node.name, data);
            }
            transformsList.Add(hostTransform);
            Log("Done adding host transforms");
            
            Log("Sending transforms from host");
            _host.Client.UpdateTransforms(transformsList).ContinueWith((x, o) => {
                Log("Received message");
                var msg = x.Result.GetResult();
                Log("Message has result, updating states");
                msg?.FromControllableStates.AllUpdates.ForEach(update => {
                    // TODO handle guid
                    Log("Started updating signals");
                    update.UpdatedSignals.ForEach(signal => {
                        // TODO choose robot based on client guid rather than hardcoding host
                        SimulationManager.Drivers[_host.Name].ForEach(driver => {
                            driver.State.SignalMap.Values.Where(sd => sd.Name == signal.Name).ToList()[0].Value = signal.Value;
                        });
                    });
                    Log("Done updating signals");
                });
                Log("Done updating states");
            }, null);
            Log("Done sending transforms from host");
        }
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
