using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace SynthesisAPI.Aether.Lobby {
    public class LobbyServer : IDisposable {

        public const int TCP_PORT = 23456;
        public const int CLIENT_LISTEN_TIMEOUT_MS = 3000;

        private Inner _instance;

        public LobbyServer() {
            _instance = new Inner();
        }
        
        private class Inner : IDisposable {

            private ulong _nextGuid = 1;
            private bool _isAlive = true;

            private readonly Mutex _clientsMutex;
            
            private readonly TcpListener _listener;
            private LinkedList<LobbyClientHandler> _clients;
            private LinkedList<Thread> _clientThreads;

            public Inner() {

                _clientsMutex = new Mutex();

                _clients = new LinkedList<LobbyClientHandler>();
                
                _listener = new TcpListener(IPAddress.Any, TCP_PORT);
                _listener.Start(3);
                _listener.BeginAcceptTcpClient(AcceptTcpClient, null);
            }

            private void AcceptTcpClient(IAsyncResult result) {
                var client = LobbyClientHandler.InitServerSide(_listener.EndAcceptTcpClient(result), _nextGuid++);
                if (client.isError)
                    return;
                _clientsMutex.WaitOne();
                var node = _clients.AddLast(client);
                var clientThread = new Thread(() => ClientListener(node));
                clientThread.Start();
                _clientThreads.AddLast(clientThread);
                _clientsMutex.ReleaseMutex();

                if (_isAlive)
                    _listener.BeginAcceptTcpClient(AcceptTcpClient, null);
            }

            private void ClientListener(LinkedListNode<LobbyClientHandler> handler) {
                var msgTask = handler.Value.ReadMessage();
                var finishedBeforeTimeout = msgTask.Wait(CLIENT_LISTEN_TIMEOUT_MS);
                if (!finishedBeforeTimeout || msgTask.Result == null) {
                    // TODO: Handle disconnect
                    return;
                }

                var msg = msgTask.Result!;
                switch (msg.MessageTypeCase) {
                    case LobbyMessage.MessageTypeOneofCase.ToGetLobbyInformation:
                        OnGetLobbyInformation(msg.ToGetLobbyInformation, handler.Value);
                        break;
                    case LobbyMessage.MessageTypeOneofCase.ToDataDump:
                    case LobbyMessage.MessageTypeOneofCase.ToClientHeartbeat:
                    default:
                        break;
                }
            }

            private void OnGetLobbyInformation(LobbyMessage.Types.ToGetLobbyInformation request, LobbyClientHandler handler) {
                _clientsMutex.WaitOne();

                LobbyMessage.Types.FromGetLobbyInformation response = new LobbyMessage.Types.FromGetLobbyInformation();
                response.LobbyInformation = new LobbyInformation();
                _clients.ForEach(x => response.LobbyInformation.Clients.Add(x.ClientInformation));
                
                _clientsMutex.ReleaseMutex();
                
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
