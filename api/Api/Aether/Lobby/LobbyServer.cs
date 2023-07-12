using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Xml.Linq;
using Org.BouncyCastle.Math.EC.Rfc7748;
using Org.BouncyCastle.Tls;
using SynthesisAPI.Controller;
using SynthesisAPI.Utilities;

#nullable enable

namespace SynthesisAPI.Aether.Lobby {
    public class LobbyServer : IDisposable {

        public const int TCP_PORT = 23456;
        public const int CLIENT_LISTEN_TIMEOUT_MS = 3000;

        private Inner? _instance;

        public IReadOnlyCollection<string> Clients => _instance?.Clients ?? new List<string>(1).AsReadOnly();

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
            
            public static FileStream LogFile { get; } = File.Create(@"../server_api.log");
            public static UTF8Encoding encoding { get; } = new UTF8Encoding(true);
            public static long CurrentTime {
                get => DateTime.Now.Ticks / TimeSpan.TicksPerMillisecond;
            }
    
            public static void Log(string message) {
                // var b = encoding.GetBytes($"[{CurrentTime}] [API] {message}\n");
                // LogFile.Write(b, 0, b.Length);
            }

            public IReadOnlyCollection<string> Clients {
                get {
                    _clientsLock.EnterReadLock();
                    var clientsInfo = new List<string>(_clients.Count);
                    _clients.Values.ForEach(x => clientsInfo.Add(x.ToString()));
                    _clientsLock.ExitReadLock();
                    return clientsInfo.AsReadOnly();
                }
            }

            public Inner() {
                _clientsLock = new ReaderWriterLockSlim();
                _remoteDataLock = new ReaderWriterLockSlim();

                _clients = new Dictionary<ulong, LobbyClientHandler>();
                _clientThreads = new LinkedList<Thread>();

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
                            _clientsLock.ExitWriteLock();
                        }

                    }
                } catch (ObjectDisposedException) {
                } catch (Exception e) {
                    Logger.Log($"Failed to accept new client: {e.Message}");
                }


                if (_isAlive)
                    _listener.BeginAcceptTcpClient(AcceptTcpClient, null);
            }

            private void ClientListener(LobbyClientHandler handler) {
                while (_isAlive) {
                    Log("Reading message");
                    var msgTask = handler.ReadMessage();
                    Log("Done reading message");
                    var finishedBeforeTimeout = msgTask.Wait(CLIENT_LISTEN_TIMEOUT_MS);
                    Log("Done waiting for message");
                    if (!finishedBeforeTimeout || msgTask.Result == null) {
                        Log("Message not received");
                        continue;
                    }
                    Log("Message received");

                    var msgRes = msgTask.Result;
                    if (msgRes.isError) {
                        Log("Message result is error");
                        if (!(msgRes.GetError() is LobbyClientHandler.ReadTimeoutException) && !(msgRes.GetError() is LobbyClientHandler.NoDataException)) {
                            Logger.Log($"Failed to Read: [{msgRes.GetError().GetType().Name}] {msgRes.GetError().Message}\n\n{msgRes.GetError().StackTrace}");
                        }
                        continue;
                    }
                    Log("Message result is not error");

                    Log("Getting message result");
                    var msg = msgRes.GetResult();
                    Log("Done getting message result");
                    switch (msg.MessageTypeCase) {
                        case LobbyMessage.MessageTypeOneofCase.ToGetLobbyInformation:
                            Log("Getting lobby information");
                            OnGetLobbyInformation(msg.ToGetLobbyInformation, handler);
                            Log("Done getting lobby information");
                            break;
                        case LobbyMessage.MessageTypeOneofCase.ToUpdateControllableState:
                            Log("Getting controllable state update");
                            OnControllableStateUpdate(msg.ToUpdateControllableState, handler);
                            Log("Done getting controllable state update");
                            break;
                        case LobbyMessage.MessageTypeOneofCase.ToUpdateTransformData:
                            Log("Getting transform data update");
                            OnTransformDataUpdate(msg.ToUpdateTransformData, handler);
                            Log("Done getting transform data update");
                            break;
                        case LobbyMessage.MessageTypeOneofCase.ToDataDump:
                        case LobbyMessage.MessageTypeOneofCase.ToClientHeartbeat:
                            Log("Updating heartbeat");
							handler.UpdateHeartbeat();
                            Log("Done updating heartbeat");
                            break;
                        default:
                            break;
                    }
                }
            }

            private void OnGetLobbyInformation(LobbyMessage.Types.ToGetLobbyInformation _, LobbyClientHandler handler) {
                _clientsLock.EnterReadLock();

                LobbyMessage.Types.FromGetLobbyInformation response = new LobbyMessage.Types.FromGetLobbyInformation {
                    LobbyInformation = new LobbyInformation()
                };
                _clients.Values.ForEach(x => response.LobbyInformation.Clients.Add(x.ClientInformation));
                
                _clientsLock.ExitReadLock();
                
                handler.WriteMessage(new LobbyMessage { FromGetLobbyInformation = response });
            }

            private void OnControllableStateUpdate(LobbyMessage.Types.ToUpdateControllableState updateRequest, LobbyClientHandler handler) {

                _remoteDataLock.EnterReadLock();
                var data = _remoteData[updateRequest.Guid];
                _remoteDataLock.ExitReadLock();

                data.EnterWriteLock();

                updateRequest.Data.ForEach(x => {
                    data.State.SetValue(x.SignalGuid, x.Value);
                });

                data.ExitWriteLock();

                var response = new LobbyMessage.Types.FromSimulationTransformData();
                
                _remoteDataLock.EnterReadLock();
                _remoteData.ForEach(kvp => {
                    kvp.Value.EnterReadLock();
                    response.TransformData.Add(kvp.Value.Transforms);
                    kvp.Value.ExitReadLock();
                });
                _remoteDataLock.ExitReadLock();

                handler.WriteMessage(new LobbyMessage { FromSimulationTransformData = response });
            }

            private void OnTransformDataUpdate(LobbyMessage.Types.ToUpdateTransformData updateRequest, LobbyClientHandler handler) {
                _remoteDataLock.EnterReadLock();

                updateRequest.TransformData.ForEach(x => {
                    var data = _remoteData[x.Guid];
                    data.EnterWriteLock();

                    x.Transforms.ForEach(kvp => {
                        data.Transforms.Transforms[kvp.Key] = kvp.Value;
                    });

                    data.ExitWriteLock();
                });

                var response = new LobbyMessage.Types.FromControllableStates();

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

                _remoteDataLock.ExitReadLock();

                handler.WriteMessage(new LobbyMessage { FromControllableStates = response });
            }

            public ControllableState? GetControllableState(ulong guid) {
                if (!_remoteData.ContainsKey(guid))
                    return null;

                var data = _remoteData[guid];
                data.EnterReadLock();
                var state = new ControllableState(data.State);
                data.ExitReadLock();
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

            State           = new ControllableState();
            Transforms      = new ServerTransforms();
            Transforms.Guid = owningClient;
        }

        public void EnterReadLock() => _lock.EnterReadLock();
        public void EnterWriteLock() => _lock.EnterWriteLock();
        public void ExitReadLock() => _lock.ExitReadLock();
        public void ExitWriteLock() => _lock.ExitWriteLock();
    }
}
