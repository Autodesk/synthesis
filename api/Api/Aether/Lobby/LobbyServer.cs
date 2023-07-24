using Org.BouncyCastle.Crypto.Tls;
using Org.BouncyCastle.Math.EC.Rfc7748;
using SynthesisAPI.Controller;
using SynthesisAPI.Utilities;
using SynthesisServer.Proto;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Threading;

#nullable enable

namespace SynthesisAPI.Aether.Lobby {
    public class LobbyServer : IDisposable {

        public const int TCP_PORT = 23456;
        public const int CLIENT_LISTEN_TIMEOUT_MS = 3000;

        private Inner? _instance;

        public IReadOnlyCollection<string> Clients => _instance?.Clients ?? new List<string>(1).AsReadOnly();
        public IReadOnlyCollection<SynthesisDataDescriptor> AvailableSynthesisData
            => _instance?.AvailableSynthesisData ?? new List<SynthesisDataDescriptor>(1).AsReadOnly();

        public LobbyServer() {
            _instance = new Inner();
        }

        public ControllableState? GetControllableState(ulong guid)
            => _instance?.GetControllableState(guid);
        
        private class Inner : IDisposable {

            private ulong _nextGuid = 1;
            private readonly Mutex _nextGuidLock = new Mutex();
            private readonly Atomic<bool> _isAlive = new Atomic<bool>(true);

            private readonly ReaderWriterLockSlim _clientsLock;
            private readonly ReaderWriterLockSlim _remoteDataLock;
            
            private readonly TcpListener _listener;
            private readonly Dictionary<ulong, LobbyClientHandler> _clients;
            private readonly LinkedList<Thread> _clientThreads;

            private readonly Dictionary<ulong, RemoteData> _remoteData;

            private readonly SHA256 _shaHashFunction = SHA256.Create();

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

            public IReadOnlyCollection<SynthesisDataDescriptor> AvailableSynthesisData {
                get {
                    _remoteDataLock.EnterReadLock();
                    List<RemoteData> allClientData = new List<RemoteData>();
                    _remoteData.ForEach(x => allClientData.Add(x.Value));
                    _remoteDataLock.ExitReadLock();

                    List<SynthesisDataDescriptor> availableData = new List<SynthesisDataDescriptor>();

                    foreach (var data in allClientData) {
                        data.EnterReadLock();

                        data.OwnedData.ForEach(x => availableData.Add(x.Value.Description.Clone()));

                        data.ExitReadLock();
                    }

                    return availableData.AsReadOnly();
                }
            }

            public Inner() {
                _clientsLock = new ReaderWriterLockSlim();
                _remoteDataLock = new ReaderWriterLockSlim();

                _clients = new Dictionary<ulong, LobbyClientHandler>();
                _clientThreads = new LinkedList<Thread>();

                _remoteData = new Dictionary<ulong, RemoteData>();
                
                _listener = new TcpListener(IPAddress.Any, TCP_PORT);
                _listener.Start();
                _listener.BeginAcceptTcpClient(AcceptTcpClient, null);
            }

            ~Inner() {
                if (_isAlive)
                    Dispose();
            }

            private ulong NextGuid() {
                _nextGuidLock.WaitOne();
                ulong guid = ++_nextGuid;
                _nextGuidLock.ReleaseMutex();
                return guid;
            }

            private void AcceptTcpClient(IAsyncResult result) {
                try {
                    var clientTcp = _listener.EndAcceptTcpClient(result);

                    if (clientTcp != null) {
                        var client = LobbyClientHandler.InitServerSide(clientTcp, NextGuid());
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
                        case LobbyMessage.MessageTypeOneofCase.ToUpdateControllableState:
                            OnControllableStateUpdate(msg.ToUpdateControllableState, handler);
                            break;
                        case LobbyMessage.MessageTypeOneofCase.ToUpdateTransformData:
                            OnTransformDataUpdate(msg.ToUpdateTransformData, handler);
                            break;
                        case LobbyMessage.MessageTypeOneofCase.ToClientHeartbeat:
							handler.UpdateHeartbeat();
                            break;
                        // Data Selection
                        case LobbyMessage.MessageTypeOneofCase.ToSelectData:
                            OnSelectData(msg.ToSelectData, handler);
                            break;
                        case LobbyMessage.MessageTypeOneofCase.ToUnselectData:
                            OnUnselectData(msg.ToUnselectData, handler);
                            break;
                        // Data Handling
                        case LobbyMessage.MessageTypeOneofCase.ToMakeDataAvailable:
                            OnMakeDataAvailable(msg.ToMakeDataAvailable, handler);
                            break;
                        case LobbyMessage.MessageTypeOneofCase.ToAllDataAvailable:
                            OnAllDataAvailable(msg.ToAllDataAvailable, handler);
                            break;
                        case LobbyMessage.MessageTypeOneofCase.ToUploadSynthesisData:
                            OnUploadData(msg.ToUploadSynthesisData, handler);
                            break;
                        case LobbyMessage.MessageTypeOneofCase.ToDownloadSynthesisData:
                            OnDownloadData(msg.ToDownloadSynthesisData, handler);
                            break;
                        default:
                            Logger.Log($"Received unknown message type: {msg.MessageTypeCase}");
                            break;
                    }
                }
            }

            private void OnSelectData(LobbyMessage.Types.ToSelectData selection, LobbyClientHandler handler) {
                LobbyMessage.Types.FromSelectData response;

                _remoteDataLock.EnterReadLock();
                if (_remoteData.ContainsKey(selection.DataOwner)) {
                    var ownerData = _remoteData[selection.DataOwner];
                    ownerData.EnterReadLock();
                    if (ownerData.OwnedData.ContainsKey(selection.DataGuid)) {
                        handler.AddSelection(new ClientSelection {
                            DataGuid = selection.DataGuid,
                            SelectionId = NextGuid().ToString(),
                            DataOwner = selection.DataOwner
                        });
                    }
                    ownerData.ExitReadLock();
                }
                _remoteDataLock.ExitReadLock();

                response = new LobbyMessage.Types.FromSelectData {
                    ClientInfo = handler.ClientInformation
                };
                handler.WriteMessage(new LobbyMessage { FromSelectData = response });
            }

            private void OnUnselectData(LobbyMessage.Types.ToUnselectData unselection, LobbyClientHandler handler) {
                LobbyMessage.Types.FromUnselectData response;

                handler.RemoveSelection(unselection.SelectionId);

                response = new LobbyMessage.Types.FromUnselectData {
                    ClientInfo = handler.ClientInformation
                };
                handler.WriteMessage(new LobbyMessage { FromUnselectData = response });
            }

            private void OnMakeDataAvailable(LobbyMessage.Types.ToMakeDataAvailable makeDataAvailable, LobbyClientHandler handler) {
                _remoteDataLock.EnterReadLock();
                var data = _remoteData[handler.Guid];
                _remoteDataLock.ExitReadLock(); // Before or after?
                data.EnterWriteLock();
                var dataGuid = NextGuid();
                var description = makeDataAvailable.Description;
                description.Guid = dataGuid;
                description.Owner = handler.Guid;
                description.DataAvailable = false;
                data.OwnedData.Add(dataGuid, new SynthesisData { Description = description });
                data.ExitWriteLock();

                handler.WriteMessage(new LobbyMessage {
                    FromMakeDataAvailableConfirmation = new LobbyMessage.Types.FromMakeDataAvailableConfirmation {
                        DataGuid = dataGuid
                    }
                });
            }

            private void OnAllDataAvailable(LobbyMessage.Types.ToAllDataAvailable _, LobbyClientHandler handler) {
                var response = new LobbyMessage.Types.FromAllDataAvailable();
                response.AvailableData.AddRange(AvailableSynthesisData);
                handler.WriteMessage(new LobbyMessage { FromAllDataAvailable = response });
            }

            private void OnUploadData(LobbyMessage.Types.ToUploadSynthesisData uploadSynthesisData, LobbyClientHandler handler) {

                var data = uploadSynthesisData.Data;
                data.Description.Owner = handler.Guid;

                var checksum = "ERROR";

                _remoteDataLock.EnterReadLock();
                if (_remoteData.ContainsKey(handler.Guid)) {
                    var ownerData = _remoteData[handler.Guid];
                    ownerData.EnterWriteLock();
                    ownerData.OwnedData[data.Description.Guid] = data;
                    ownerData.OwnedData[data.Description.Guid].Description.DataAvailable = true;
                    ownerData.ExitWriteLock();
                    ownerData.EnterReadLock();
                    checksum = Convert.ToBase64String(_shaHashFunction.ComputeHash(
                        new ByteStream(
                            data.Buffer.GetEnumerator(),
                            data.Buffer.Length
                        )
                    ));
                    ownerData.ExitReadLock();
                }
                _remoteDataLock.ExitReadLock();
                
                handler.WriteMessage(new LobbyMessage {
                    FromUploadSynthesisDataConfirmation = new LobbyMessage.Types.FromUploadSynthesisDataConfirmation {
                        Checksum = checksum
                    }
                });
            }

            private void OnDownloadData(LobbyMessage.Types.ToDownloadSynthesisData toDownloadSynthesisData, LobbyClientHandler handler) {
                
                var response = new LobbyMessage.Types.FromDownloadSynthesisData {
                    DataAvailable = false
                };

                _remoteDataLock.EnterReadLock();
                if (_remoteData.ContainsKey(toDownloadSynthesisData.DataOwner)) {
                    var ownerData = _remoteData[handler.Guid];
                    ownerData.EnterReadLock();

                    if (
                        ownerData.OwnedData.ContainsKey(toDownloadSynthesisData.DataGuid)
                        && ownerData.OwnedData[toDownloadSynthesisData.DataGuid].Description.DataAvailable
                    ) {
                        response.DataAvailable = true;
                        response.Data = ownerData.OwnedData[toDownloadSynthesisData.DataGuid].Clone();
                    }

                    ownerData.ExitReadLock();
                }
                _remoteDataLock.ExitReadLock();
                
                handler.WriteMessage(new LobbyMessage {
                    FromDownloadSynthesisData = response
                });
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
                        data.State.SignalMap[x.SignalGuid].Name = x.Name;
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

        public Dictionary<ulong, SynthesisData> OwnedData;

        public RemoteData(ulong owningClient) {
            _owningClient = owningClient;
            _lock = new ReaderWriterLockSlim();

            State           = new ControllableState();
            Transforms      = new ServerTransforms();
            OwnedData       = new Dictionary<ulong, SynthesisData>();
            Transforms.Guid = owningClient;
        }

        public void EnterReadLock() => _lock.EnterReadLock();
        public void EnterWriteLock() => _lock.EnterWriteLock();
        public void ExitReadLock() => _lock.ExitReadLock();
        public void ExitWriteLock() => _lock.ExitWriteLock();
    }
}
