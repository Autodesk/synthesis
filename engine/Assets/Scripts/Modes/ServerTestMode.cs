#nullable enable

using System.Collections.Generic;
using System.Linq;
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

    public void Start() {
        // MainHUD.AddItemToDrawer("Multibot", b => DynamicUIManager.CreatePanel<RobotSwitchPanel>());
        
        _server = new LobbyServer();

        // DynamicUIManager.CreateModal<ServerTestModal>();
        // TODO remove and allow user to choose robot
        string dozer = "Dozer_v9.mira";
        string tmm   = "Team 2471 (2018)_v5.mira";
        RobotSimObject.SpawnRobot(AddRobotModal.ParsePath("$appdata/Autodesk/Synthesis/Mira/" + tmm, '/'), new Vector3(-2, 1, 0), Quaternion.Euler(0, 0, 0), false);
        _host                   = RobotSimObject.GetCurrentlyPossessedRobot();
        _host.RobotNode.name    = "host";
        _host.BehavioursEnabled = false;
        RobotSimObject.SpawnRobot(AddRobotModal.ParsePath("$appdata/Autodesk/Synthesis/Mira/" + tmm, '/'), new Vector3(2, 1, 0), Quaternion.Euler(0, 0, 0), false);
        _ghost                = RobotSimObject.GetCurrentlyPossessedRobot();
        _ghost.RobotNode.name = "ghost";
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
        
        if (_ghost.Client is not null) {
            var ghostTransform = new ServerTransforms() {
                Guid = _ghost.Client.Guid.Value
            };

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
        }

        if (_host.Client is not null) {
            var hostTransform = new ServerTransforms() {
                Guid = _host.Client.Guid.Value
            };

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
            
            _host.Client.UpdateTransforms(transformsList).ContinueWith((x, o) => {
                var msg = x.Result.GetResult();
                msg?.FromControllableStates.AllUpdates.ForEach(update => {
                    // TODO handle guid
                    update.UpdatedSignals.ForEach(signal => {
                        SimulationManager.Drivers[_host.Name].ForEach(driver => {
                            driver.State.SignalMap.Values.Where(sd => sd.Name == signal.Name).ToList()[0].Value = signal.Value;
                        });
                    });
                });
            }, null);
        }
        
        foreach (RobotSimObject robot in new RobotSimObject[]{_host, _ghost}) {
            if (robot.Client is not null) {
                List<SignalData> changedSignals = new List<SignalData>();
                foreach (var driver in SimulationManager.Drivers[robot.Name]) {
                    List<SignalData> changes = driver.State.CompileChanges();
                    foreach (var signal in changes)
                        changedSignals.Add(signal);
                }

                robot.Client.UpdateControllableState(changedSignals).ContinueWith((x, o) => {
                    var msg = x.Result.GetResult();
                    msg?.FromSimulationTransformData.TransformData.ForEach(transform => {
                        // if (transform.Guid == _client?.Guid) {
                        foreach (var td in transform.Transforms) {
                            var SpatialMatrix = td.Value.MatrixData;
                            Matrix4x4 matrix = new Matrix4x4(
                                new Vector4(SpatialMatrix[0], SpatialMatrix[4], SpatialMatrix[8], SpatialMatrix[12]),
                                new Vector4(SpatialMatrix[1], SpatialMatrix[5], SpatialMatrix[9], SpatialMatrix[13]),
                                new Vector4(SpatialMatrix[2], SpatialMatrix[6], SpatialMatrix[10], SpatialMatrix[14]),
                                new Vector4(SpatialMatrix[3], SpatialMatrix[7], SpatialMatrix[11], SpatialMatrix[15])
                                );
                            Transform nodeTransform = robot.RobotNode.transform.Find(td.Key);
                            nodeTransform.position   = matrix.GetPosition();
                            nodeTransform.rotation   = matrix.rotation;
                            nodeTransform.localScale = matrix.lossyScale;
                        }
                        // }
                    });
                }, null);
            }
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
