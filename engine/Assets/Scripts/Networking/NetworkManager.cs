using System;
using System.Threading;
using System.Threading.Tasks;
using SynthesisAPI.EventBus;
using SynthesisAPI.Utilities;
using SynthesisServer.Client;
using UnityEngine;

using Logger = SynthesisAPI.Utilities.Logger;

#nullable enable

namespace Synthesis.Networking {
    public static class NetworkManager {

        public static bool IsConnected => Instance.IsConnected;

        public static bool ConnectToServer(string ip) {
            return Instance.ConnectToServer(ip);
        }

        public static void DisconnectFromServer() {
            Instance.DisconnectFromServer();
        }

        public static void CreateLobby(string name) {
            Instance.CreateLobby(name);
        }

        public static Task<ServerInfoResponse?>? GetServerInfo()
            => Instance.GetServerInfo();

        public static void Init() {
            _innerInstance = null;

            // TODO
            _ = Instance;
        }

        public static void Kill() {
            _innerInstance = null;
        }

        private class Inner {

            public bool IsConnected => Client.Instance.IsTcpConnected;

            private MyClientHandler _clientHandler;

            public Inner() {
                Client.ErrorReport += s => Debug.Log($"Client Error: {s}");
                _clientHandler = new MyClientHandler();
                Client.Instance.Init(_clientHandler);
            }

            public bool ConnectToServer(string ip) {
                if (Client.Instance.IsTcpConnected)
                    return false;

                var res = Client.Instance.Start(ip);
                if (res) {
                    Task.Factory.StartNew(() => {
                        while (Client.Instance.IsTcpConnected && _innerInstance != null) {
                            Thread.Sleep(1000);
                            Client.Instance.TrySendHeartbeat();
                        }
                    });
                }
                return res;
            }

            public void DisconnectFromServer() {
                if (!Client.Instance.IsTcpConnected)
                    return;

                Client.Instance.Stop();
            }

            public void CreateLobby(string name) {
                if (!Client.Instance.IsTcpConnected)
                    return;

                Client.Instance.TrySendCreateLobby(name);
            }

            public Task<ServerInfoResponse?>? GetServerInfo() {
                if (!Client.Instance.IsTcpConnected)
                    return null;
                
                return Task<ServerInfoResponse?>.Factory.StartNew(() => {
                    if (!Client.Instance.TrySendGetServerData())
                        return null;

                    ServerInfoResponse? response = _clientHandler.ServerInfo;
                    while (response == null) {
                        Thread.Sleep(100);
                        response = _clientHandler.ServerInfo;
                    }
                    return response;
                });
            }

            ~Inner() {
                Client.Instance.Stop();
            }
        }

        private static Inner? _innerInstance = null;
        private static Inner Instance {
            get {
                if (_innerInstance == null) {
                    _innerInstance = new Inner();
                }
                return _innerInstance;
            }
        }
    }

    public class MyClientHandler : IClientHandler {
        public void HandleCreateLobbyResponse(CreateLobbyResponse createLobbyResponse) {
            if (createLobbyResponse.GenericResponse.Success) {
                Debug.Log($"Successfully Created Lobby: {createLobbyResponse.LobbyName}");
            } else {
                Debug.Log("Unsuccessful Lobby Creation");
            }
        }

        public void HandleDeleteLobbyResponse(DeleteLobbyResponse deleteLobbyResponse) {
            throw new NotImplementedException();
        }

        public void HandleJoinLobbyResponse(JoinLobbyResponse joinLobbyResponse) {
            throw new NotImplementedException();
        }

        public void HandleLeaveLobbyResponse(LeaveLobbyResponse leaveLobbyResponse) {
            throw new NotImplementedException();
        }

        private static readonly object _serverInfoLock = new object();
        private ServerInfoResponse? _serverInfo;
        public ServerInfoResponse? ServerInfo {
            get {
                if (_serverInfo != null) {
                    ServerInfoResponse tmp;
                    lock (_serverInfoLock) {
                        tmp = _serverInfo;
                        _serverInfo = null;
                    }
                    return tmp;
                }
                return null;
            }
        }
        public void HandleServerInfoResponse(ServerInfoResponse serverInfoResponse) {
            lock (_serverInfoLock) {
                _serverInfo = serverInfoResponse;
            }
        }

        public void HandleStartLobbyResponse(StartLobbyResponse startLobbyResponse) {
            throw new NotImplementedException();
        }

        public void HandleStatusMessage(StatusMessage statusMessage) {
            throw new NotImplementedException();
        }

        public void HandleSwapResponse(SwapResponse swapResponse) {
            throw new NotImplementedException();
        }

        public void HandleChangeNameResponse(ChangeNameResponse changeNameResponse)
        {
            throw new NotImplementedException();
        }
    }
}
