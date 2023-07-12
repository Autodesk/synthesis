using SynthesisAPI.Controller;
using SynthesisAPI.Utilities;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Security.Cryptography;

#nullable enable

namespace SynthesisAPI.Aether.Lobby {
    public class LobbyServer : IDisposable {

        public const int TCP_PORT = 23456;
        public const int CLIENT_LISTEN_TIMEOUT_MS = 3000;

        private Inner? _instance;

        public IReadOnlyCollection<string> Clients => _instance?.Clients ?? new List<string>(1).AsReadOnly();
        public IReadOnlyCollection<DataRobot> AvailableRobots => _instance?.AvailableRobots ?? new List<DataRobot>(1).AsReadOnly();

        public LobbyServer() {
            _instance = new Inner();
        }

        public ControllableState? GetControllableState(ulong guid)
            => _instance?.GetControllableState(guid);
        
        private class Inner : IDisposable {

            private ulong _nextGuid = 1;
            private readonly Atomic<bool> _isAlive = new Atomic<bool>(true);

            private readonly ReaderWriterLockSlim _clientsLock;
            private readonly ReaderWriterLockSlim _remoteDataLock;
            
            private readonly TcpListener _listener;
            private readonly Dictionary<ulong, LobbyClientHandler> _clients;
            private readonly LinkedList<Thread> _clientThreads;

            private readonly Dictionary<ulong, RemoteData> _remoteData;

            public IReadOnlyCollection<string> Clients {
                get {
                    List<string> clientsInfo;
                    _clientsLock.EnterReadLock();
                    try {
                        clientsInfo = new List<string>(_clients.Count);
                        _clients.Values.ForEach(x => clientsInfo.Add(x.ToString()));
                    } finally {
                        _clientsLock.ExitReadLock();
                    }

                    return clientsInfo.AsReadOnly();
                }
            }

            private readonly ReaderWriterLockSlim _robotDataLock;
            private List<DataRobot> _availableRobots;
            public IReadOnlyCollection<DataRobot> AvailableRobots {
                get {
                    List<DataRobot> robots;
                    _robotDataLock.EnterReadLock();
                    try {
                        robots = new List<DataRobot>(_availableRobots);
                    } finally {
                        _robotDataLock.ExitReadLock();
                    }

                    return robots.AsReadOnly();
                }
            }

            public Inner() {
                _clientsLock = new ReaderWriterLockSlim();
                _remoteDataLock = new ReaderWriterLockSlim();
                _robotDataLock = new ReaderWriterLockSlim();

                _clients = new Dictionary<ulong, LobbyClientHandler>();
                _clientThreads = new LinkedList<Thread>();
                _availableRobots = new List<DataRobot>();

                _remoteData = new Dictionary<ulong, RemoteData>();
                
                _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), TCP_PORT);
                _listener.Start();
                _listener.BeginAcceptTcpClient(AcceptTcpClient, null);
            }

            ~Inner() {
                if (_isAlive)
                    Dispose();
            }

            private void AcceptTcpClient(IAsyncResult result) {
                try {
                    var clientTcp = _listener.EndAcceptTcpClient(result);

                    if (clientTcp != null) {
                        var client = LobbyClientHandler.InitServerSide(clientTcp, _nextGuid++);
                        if (!client.isError) {
                            _clientsLock.EnterWriteLock();
                            var clientResult = client.GetResult();
                            _clients.Add(clientResult.Guid, clientResult);

                            _remoteDataLock.EnterWriteLock();
                            _remoteData.Add(clientResult.Guid, new RemoteData(clientResult.Guid));
                            _remoteDataLock.ExitWriteLock();

                            var clientThread = new Thread(() => ClientListener(client));
                            clientThread.Start();
                            _clientThreads.AddLast(clientThread);
                        }

                    }
                } catch (ObjectDisposedException) {
                } catch (Exception e) {
                    Logger.Log($"Failed to accept new client: {e.Message}");
                } finally {
                    _clientsLock.ExitWriteLock();
                }

                if (_isAlive)
                    _listener.BeginAcceptTcpClient(AcceptTcpClient, null);
            }

            private void ClientListener(LobbyClientHandler handler) {
                while (_isAlive) {
                    var msgTask = handler.ReadMessage();
                    var finishedBeforeTimeout = msgTask.Wait(CLIENT_LISTEN_TIMEOUT_MS);
                    if (!finishedBeforeTimeout || msgTask.Result == null) {
                        Logger.Log("Read Time Out");
                        continue;
                    }

                    var msgRes = msgTask.Result;
                    if (msgRes.isError) {
                        if (!(msgRes.GetError() is LobbyClientHandler.ReadTimeoutException) && !(msgRes.GetError() is LobbyClientHandler.NoDataException)) {
                            Logger.Log($"Failed to Read: [{msgRes.GetError().GetType().Name}] {msgRes.GetError().Message}\n\n{msgRes.GetError().StackTrace}");
                        }
                        continue;
                    }

                    var msg = msgRes.GetResult();
                    switch (msg.MessageTypeCase) {
                        case LobbyMessage.MessageTypeOneofCase.ToGetLobbyInformation:
                            OnGetLobbyInformation(msg.ToGetLobbyInformation, handler);
                            break;
                        case LobbyMessage.MessageTypeOneofCase.ToDataRobot:
                            Logger.Log("Received Robot Upload");
                            AcceptRobotData(msg.ToDataRobot, handler);
                            break;
                        case LobbyMessage.MessageTypeOneofCase.ToUpdateControllableState:
                            OnControllableStateUpdate(msg.ToUpdateControllableState, handler);
                            break;
                        case LobbyMessage.MessageTypeOneofCase.ToUpdateTransformData:
                            OnTransformDataUpdate(msg.ToUpdateTransformData, handler);
                            break;
                        case LobbyMessage.MessageTypeOneofCase.ToDataDump:
                        case LobbyMessage.MessageTypeOneofCase.ToClientHeartbeat:
							handler.UpdateHeartbeat();
                            break;
                        default:
                            break;
                    }
                }
            }

            private void OnGetLobbyInformation(LobbyMessage.Types.ToGetLobbyInformation _, LobbyClientHandler handler) {
                LobbyMessage.Types.FromGetLobbyInformation response;
                _clientsLock.EnterReadLock();

                try {
                    response = new LobbyMessage.Types.FromGetLobbyInformation {
                        LobbyInformation = new LobbyInformation()
                    };
                    _clients.Values.ForEach(x => response.LobbyInformation.Clients.Add(x.ClientInformation));
                } finally {
                    _clientsLock.ExitReadLock();
                }
                
                handler.WriteMessage(new LobbyMessage { FromGetLobbyInformation = response });
            }

            private void AcceptRobotData(LobbyMessage.Types.ToDataRobot robot, LobbyClientHandler handler) {
                _robotDataLock.EnterWriteLock();
                try {
                    SHA256 sha = SHA256.Create();
                    var checksum = Convert.ToBase64String(sha.ComputeHash(robot.DataRobot.Data.ToByteArray()));
                    _availableRobots.Add(new DataRobot(robot.DataRobot));
                } finally {
                    _robotDataLock.ExitWriteLock();
                }

                var response = new LobbyMessage.Types.FromDataRobot();

                _robotDataLock.EnterReadLock();
                try {
                    response.AllAvailableRobots.AddRange(_availableRobots.Select(x => new DataRobot { Name = x.Name, Description = x.Description, Guid = x.Guid }));
                } finally {
                    _robotDataLock.ExitReadLock();
                }

                var respResult = handler.WriteMessage(new LobbyMessage { FromDataRobot = response });
                if (respResult.isError) {
                    Logger.Log($"Response write encountered error: {respResult.GetError().Message}");
                }
            }

            private void OnControllableStateUpdate(LobbyMessage.Types.ToUpdateControllableState updateRequest, LobbyClientHandler handler) {
                RemoteData data;

                _remoteDataLock.EnterReadLock();
                try {
                    data = _remoteData[updateRequest.Guid];
                } finally {
                    _remoteDataLock.ExitReadLock();
                }

                data.EnterWriteLock();
                try {
                    updateRequest.Data.ForEach(x => {
                        data.State.SetValue(x.SignalGuid, x.Value);
                    });
                } finally {
                    data.ExitWriteLock();
                }

                var response = new LobbyMessage.Types.FromSimulationTransformData();
                
                _remoteDataLock.EnterReadLock();
                try {
                    _remoteData.ForEach(kvp => {
                        kvp.Value.EnterReadLock();
                        response.TransformData.Add(kvp.Value.Transforms);
                        kvp.Value.ExitReadLock();
                    });
                } finally {
                    _remoteDataLock.ExitReadLock();
                }

                handler.WriteMessage(new LobbyMessage { FromSimulationTransformData = response });
            }

            private void OnTransformDataUpdate(LobbyMessage.Types.ToUpdateTransformData updateRequest, LobbyClientHandler handler) {
                var response = new LobbyMessage.Types.FromControllableStates();
                _remoteDataLock.EnterReadLock();
                try {
                    updateRequest.TransformData.ForEach(x => {
                        var data = _remoteData[x.Guid];
                        data.EnterWriteLock();

                        x.Transforms.ForEach(kvp => {
                            data.Transforms.Transforms[kvp.Key] = kvp.Value;
                        });

                        data.ExitWriteLock();
                    });


                    _remoteData.ForEach(kvp => {
                        kvp.Value.EnterWriteLock();
                        if (kvp.Value.State.HasUpdates) {
                            SignalUpdates updates = new SignalUpdates {
                                Guid = kvp.Key
                            };
                            updates.UpdatedSignals.AddRange(kvp.Value.State.CompileChanges());
                            response.AllUpdates.Add(updates);
                        }
                        kvp.Value.ExitWriteLock();
                    });
                } finally {
                    _remoteDataLock.ExitReadLock();
                }

                handler.WriteMessage(new LobbyMessage { FromControllableStates = response });
            }

            public ControllableState? GetControllableState(ulong guid) {
                if (!_remoteData.ContainsKey(guid))
                    return null;

                var data = _remoteData[guid];
                ControllableState state;
                data.EnterReadLock();
                try {
                    state = new ControllableState(data.State);
                } finally {
                    data.ExitReadLock();
                }

                return state;
            }

            public void Dispose() {

                Logger.Log("Disposing Server");

                _isAlive.Value = false;
                _listener.Stop();
                _clientThreads.ForEach(x => x.Join());
                _clients.ForEach(x => x.Value.Dispose());

                Logger.Log("Server Disposed");
            }
        }

        public void Dispose() {
            _instance?.Dispose();
            _instance = null;
        }
    }

    public class RemoteData {
        private readonly ulong _owningClient;
        public ulong OwningClient { get => _owningClient; }

        private readonly ReaderWriterLockSlim _lock;

        public ControllableState State;
        public ServerTransforms Transforms;

        public RemoteData(ulong owningClient) {
            _owningClient = owningClient;
            _lock = new ReaderWriterLockSlim();

            State = new ControllableState();
            Transforms = new ServerTransforms();
        }

        public void EnterReadLock() => _lock.EnterReadLock();
        public void EnterWriteLock() => _lock.EnterWriteLock();
        public void ExitReadLock() => _lock.ExitReadLock();
        public void ExitWriteLock() => _lock.ExitWriteLock();
    }
}
