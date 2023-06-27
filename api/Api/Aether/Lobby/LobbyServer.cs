using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SynthesisAPI.Aether.Lobby {
    public class LobbyServer : IDisposable {

        public const int TCP_PORT = 23456;
        public const int CLIENT_LISTEN_TIMEOUT_MS = 3000;

        private Inner _instance;

        public ReadOnlyCollection<LobbyClientInformation> Clients => _instance.Clients;

        public LobbyServer() {
            _instance = new Inner();
        }
        
        private class Inner : IDisposable {

            private ulong _nextGuid = 1;
            private bool _isAlive = true;

            private readonly ReaderWriterLockSlim _clientsLock;
            
            private readonly TcpListener _listener;
            private Dictionary<ulong, LobbyClientHandler> _clients;
            private LinkedList<Thread> _clientThreads;

            public ReadOnlyCollection<LobbyClientInformation> Clients {
                get {
                    // _clientsLock.EnterReadLock();
                    var clients = _clients.Values.Select(x => x.ClientInformation).ToList().AsReadOnly();
                    // _clientsLock.ExitReadLock();
                    return clients;
                }
            }

            public Inner() {

                _clientsLock = new ReaderWriterLockSlim();

                _clients = new Dictionary<ulong, LobbyClientHandler>();
                
                _listener = new TcpListener(IPAddress.Any, TCP_PORT);
                _listener.Start(3);
                _listener.BeginAcceptTcpClient(AcceptTcpClient, null);
            }

            private void AcceptTcpClient(IAsyncResult result) {

                var clientTcp = _listener.EndAcceptTcpClient(result);

                if (clientTcp != null) {

                    var client = LobbyClientHandler.InitServerSide(clientTcp, _nextGuid++);
                    if (!client.isError) {
                        // _clientsLock.EnterWriteLock();
                        var clientResult = client.GetResult();
                        _clients.Add(clientResult.Guid, clientResult);
                        var clientThread = new Thread(() => ClientListener(client));
                        clientThread.Start();
                        _clientThreads.AddLast(clientThread);
                        // _clientsLock.ExitWriteLock();
                    }

                }

                if (_isAlive)
                    _listener.BeginAcceptTcpClient(AcceptTcpClient, null);
            }

            private void ClientListener(LobbyClientHandler handler) {
                while (_isAlive) {
                    var msgTask = handler.ReadMessage();
                    var finishedBeforeTimeout = msgTask.Wait(CLIENT_LISTEN_TIMEOUT_MS);
                    if (!finishedBeforeTimeout || msgTask.Result == null) {
                        // TODO: Handle disconnect
                        continue;
                    }

                    var msg = msgTask.Result!;
                    switch (msg.MessageTypeCase) {
                        case LobbyMessage.MessageTypeOneofCase.ToGetLobbyInformation:
                            OnGetLobbyInformation(msg.ToGetLobbyInformation, handler);
                            break;
                        case LobbyMessage.MessageTypeOneofCase.ToDataDump:
                        case LobbyMessage.MessageTypeOneofCase.ToClientHeartbeat:
                        default:
                            break;
                    }
                }
            }

            private void OnGetLobbyInformation(LobbyMessage.Types.ToGetLobbyInformation request, LobbyClientHandler handler) {
                // _clientsLock.EnterReadLock();

                LobbyMessage.Types.FromGetLobbyInformation response = new LobbyMessage.Types.FromGetLobbyInformation();
                response.LobbyInformation = new LobbyInformation();
                _clients.Values.ForEach(x => response.LobbyInformation.Clients.Add(x.ClientInformation));
                
                // _clientsLock.ExitReadLock();
                
                handler.WriteMessage(new LobbyMessage { FromGetLobbyInformation = response });
            }

            public void Dispose() {
                _isAlive = false;
                _listener.Stop();
            }
        }

        public void Dispose() {
            _instance.Dispose();
            _instance = null;
        }
    }
}
