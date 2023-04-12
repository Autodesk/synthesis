using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using SynthesisAPI.Aether;
using SynthesisAPI.Utilities;

namespace SynthesisAPI.Aether.Lobby {
    public class LobbyClient {

        private LobbyClientInformation _clientInformation;
        private UdpClient _udpClient;
        private IPEndPoint _remoteSendEndPoint, _remoteReceiveEndPoint; // TODO: Need a receive endpoint?

        public LobbyClient(string username, int localPort, IPAddress remoteIP) {
            _remoteSendEndPoint = new IPEndPoint(remoteIP, LobbyServer.UDP_RECEIVE_PORT);
            // _remoteReceiveEndPoint = new IPEndPoint(remoteIP, LobbyServer.UDP_RECEIVE_PORT);
            _udpClient = new UdpClient(localPort);
            _clientInformation = new LobbyClientInformation();
            _clientInformation.Username = username;
        }

        public Task ConnectToServer() =>
            Task.Factory.StartNew(() => {
                var request = new LobbyMessage() { ToRegisterClient = new LobbyMessage.Types.ToRegisterClient() { ClientInfo = _clientInformation } };
                var buf = request.GetBuffer();
                _udpClient.Connect(_remoteSendEndPoint);
                Logger.Log("Connected, sending buffer", LogLevel.Info);
                _udpClient.Send(buf, buf.Length);
                IPEndPoint ipep = null;
                Logger.Log("Waiting for response", LogLevel.Info);
                var response = LobbyMessage.Types.FromRegisterClient.Parser.ParseFrom(_udpClient.Receive(ref ipep));
                Logger.Log("Response received", LogLevel.Info);
                _clientInformation = response.UpdatedClientInfo;
            });
        
        public (LobbyClientInformation host, IEnumerable<LobbyClientInformation> clients) GetConnectedClients() {
            var request = new LobbyMessage() { ToGetFellowClients = new LobbyMessage.Types.ToGetFellowClients() { SenderGuid = _clientInformation.Guid } };
            var buf = request.GetBuffer();
            _udpClient.Send(buf, buf.Length);
            IPEndPoint ipep = null;
            var response = LobbyMessage.Types.FromGetFellowClients.Parser.ParseFrom(_udpClient.Receive(ref ipep));
            return (response.HostInfo, response.ClientsInfo);
        }
    }
}
