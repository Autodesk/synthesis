using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using Org.BouncyCastle.Math.EC.Rfc7748;
using Org.BouncyCastle.Tls;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.Aether.Lobby {
    public class LobbyServer : IDisposable {

        public const int TCP_PORT = 23456;
        public const int CLIENT_LISTEN_TIMEOUT_MS = 3000;

        private Inner _instance;

        public IReadOnlyCollection<string> Clients => _instance.Clients;

        public LobbyServer() {
            _instance = new Inner();
        }
        
        private class Inner : IDisposable {

            private ulong _nextGuid = 1;
            private readonly Atomic<bool> _isAlive = true;

            private readonly ReaderWriterLockSlim _clientsLock;
            
            private readonly TcpListener _listener;
            private readonly Dictionary<ulong, LobbyClientHandler> _clients;
            private readonly LinkedList<Thread> _clientThreads;

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

                Logger.Log("Starting server");

                _clientsLock = new ReaderWriterLockSlim();

                _clients = new Dictionary<ulong, LobbyClientHandler>();
                _clientThreads = new LinkedList<Thread>();
                
                _listener = new TcpListener(IPAddress.Parse("127.0.0.1"), TCP_PORT);
                _listener.Start();
                _listener.BeginAcceptTcpClient(AcceptTcpClient, null);

                Logger.Log("Server Started");
            }

            ~Inner() {
                Dispose();
            }

            private void AcceptTcpClient(IAsyncResult result) {
                try
                {
                    var clientTcp = _listener.EndAcceptTcpClient(result);

                    if (clientTcp != null)
                    {

                        var client = LobbyClientHandler.InitServerSide(clientTcp, _nextGuid++);
                        if (!client.isError)
                        {
                            _clientsLock.EnterWriteLock();
                            var clientResult = client.GetResult();
                            _clients.Add(clientResult.Guid, clientResult);
                            var clientThread = new Thread(() => ClientListener(client));
                            clientThread.Start();
                            _clientThreads.AddLast(clientThread);
                            _clientsLock.ExitWriteLock();
                        }

                    }
                } catch (Exception e)
                {
                    UnityEngine.Debug.Log(e.StackTrace);
                }

                if (_isAlive)
                    _listener.BeginAcceptTcpClient(AcceptTcpClient, null);
            }

            private void ClientListener(LobbyClientHandler handler) {
                while (_isAlive!) {
                    var msgTask = handler.ReadMessage();
                    var finishedBeforeTimeout = msgTask.Wait(CLIENT_LISTEN_TIMEOUT_MS);
                    if (!finishedBeforeTimeout || msgTask.Result == null) {
                        // TODO: Handle disconnect
                        continue;
                    }

                    var msgRes = msgTask.Result;
                    if (msgRes.isError) {
                        if (!(msgRes.GetError() is LobbyClientHandler.ReadTimeoutException)) {
                            Logger.Log($"Failed to Read: [{msgRes.GetError().GetType().Name}] {msgRes.GetError().Message}\n\n{msgRes.GetError().StackTrace}");
                        }
                        return;
                    }

                    var msg = msgRes.GetResult();
                    switch (msg.MessageTypeCase) {
                        case LobbyMessage.MessageTypeOneofCase.ToGetLobbyInformation:
                            OnGetLobbyInformation(msg.ToGetLobbyInformation, handler);
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
                _clientsLock.EnterReadLock();

                LobbyMessage.Types.FromGetLobbyInformation response = new LobbyMessage.Types.FromGetLobbyInformation {
                    LobbyInformation = new LobbyInformation()
                };
                _clients.Values.ForEach(x => response.LobbyInformation.Clients.Add(x.ClientInformation));
                
                _clientsLock.ExitReadLock();
                
                handler.WriteMessage(new LobbyMessage { FromGetLobbyInformation = response });
            }

            public void Dispose() {

                Logger.Log("Disposing Server");

                _isAlive.Value = false;
                _listener.Stop();
                _clients.ForEach(x => x.Value.Dispose());
                _clientThreads.ForEach(x => x.Join());

                Logger.Log("Server Disposed");
            }
        }

        public void Dispose() {
            _instance.Dispose();
            _instance = null;
        }
    }
}
