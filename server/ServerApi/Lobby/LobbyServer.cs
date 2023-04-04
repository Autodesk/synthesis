using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using SynthesisServer.Utilities;

namespace ServerApi.Lobby {
    public class LobbyServer {

        public const int UDP_RECEIVE_PORT = 12435;
        public const int UDP_SEND_PORT = 12436;

        private UdpClient _udpReceive;
        private UdpClient _udpSend;

        private Thread _receiveThread;

        private ulong nextGuid = 1;
        private bool _shouldListen = true;

        private LobbyClientInformation _hostInformation;
        private Dictionary<ulong, LobbyClientInformation> _clients;

        private LobbyServer() { }

        public void InitServer() {
            _clients = new Dictionary<ulong, LobbyClientInformation>();
            
            _udpReceive = new UdpClient(UDP_RECEIVE_PORT);
            _udpSend = new UdpClient(UDP_SEND_PORT);

            _receiveThread = new Thread(ReceiveLoop);
            _receiveThread.Start();
        }

        private void ReceiveLoop() {
            
            // TODO: Dispatch each message into a different thread?
            while (_shouldListen) {
                var receiveTask = _udpReceive.ReceiveAsync();
                receiveTask.Wait();
                var result = receiveTask.Result;
                LobbyMessage message = LobbyMessage.Parser.ParseFrom(result.Buffer);

                byte[] response = null;
                switch (message.MessageTypeCase) {
                    case LobbyMessage.MessageTypeOneofCase.ToRegisterClient:
                        response = ProcessRegisterClient(message.ToRegisterClient).GetBuffer();
                        break;
                    case LobbyMessage.MessageTypeOneofCase.ToGetFellowClients:
                        response = ProcessGetFellowClients(message.ToGetFellowClients).GetBuffer();
                        break;
                    default:
                        // Unsupported Message Type
                        break;
                }
                if (response != null && response.Length > 0)
                    _udpSend.Send(response, response.Length, result.RemoteEndPoint);
            }
        }

        private LobbyMessage.Types.FromRegisterClient ProcessRegisterClient(LobbyMessage.Types.ToRegisterClient toRegisterClient) {
            var currentInformation = toRegisterClient.ClientInfo;
            // TODO: Perform checks against things like username and maybe a pre-existing GUID

            currentInformation.Guid = nextGuid;
            nextGuid++;
            // TODO: We might want to keep track of IPs
            _clients[currentInformation.Guid] = currentInformation;

            LobbyMessage.Types.FromRegisterClient response = new LobbyMessage.Types.FromRegisterClient();
            response.UpdatedClientInfo = currentInformation;
            return response;
        }

        private LobbyMessage.Types.FromGetFellowClients ProcessGetFellowClients(LobbyMessage.Types.ToGetFellowClients toGetFellowClients) {
            var response = new LobbyMessage.Types.FromGetFellowClients();
            response.HostInfo = _hostInformation;
            response.ClientsInfo.Add(_clients.Values);
            return response;
        }

        private LobbyServer _instance = null;
        public LobbyServer Instance {
            get {
                _instance = _instance ?? new LobbyServer();
                return _instance;
            }
        }

    }
}
