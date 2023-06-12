using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using SynthesisAPI.Aether;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.Aether.Lobby {
    public class LobbyServer : IDisposable {

        public const int TCP_PORT = 23456;

        private Inner _instance;

        public LobbyServer() {
            _instance = new Inner();
        }
        
        private class Inner : IDisposable {

            private ulong _nextGuid = 1;
            private bool _isAlive = true;

            private Mutex _clientsMutex;
            
            private TcpListener _listener;
            private LinkedList<LobbyClientHandler> _clients;

            public Inner() {

                _clientsMutex = new Mutex();

                _clients = new LinkedList<LobbyClientHandler>();
                
                _listener = new TcpListener(TCP_PORT);
                _listener.BeginAcceptTcpClient(AcceptTcpClient, null);
            }

            private void AcceptTcpClient(IAsyncResult result) {
                var client = LobbyClientHandler.InitServerSide(_listener.EndAcceptTcpClient(result), _nextGuid++);
                if (client == null)
                    return;
                _clientsMutex.WaitOne();
                _clients.AddLast(client);
                _clientsMutex.ReleaseMutex();
            }

            public void Dispose() {
                _isAlive = false;
            }
        }

        public void Dispose() {
            _instance.Dispose();
            _instance = null;
        }
    }
}
