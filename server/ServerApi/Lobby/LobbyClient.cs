using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using SynthesisServer.Utilities;

namespace ServerApi.Lobby {
    public class LobbyClient {

        private LobbyClientInformation _clientInformation;
        private UdpClient _udpClient;
        private IPEndPoint _remoteSendEndPoint, _remoteReceiveEndPoint; // TODO: Need a receive endpoint?

        public LobbyClient(string username, int localPort, IPAddress remoteIP) {
            _remoteSendEndPoint = new IPEndPoint(remoteIP, LobbyServer.UDP_SEND_PORT);
            _remoteReceiveEndPoint = new IPEndPoint(remoteIP, LobbyServer.UDP_RECEIVE_PORT);
            _udpClient = new UdpClient(localPort);
            _clientInformation = new LobbyClientInformation();
            _clientInformation.Username = username;
        }

        public void ConnectToServer() {
            var request = new LobbyMessage.Types.ToRegisterClient();
            request.ClientInfo = _clientInformation;
            var buf = request.GetBuffer();
            _udpClient.Send(buf, buf.Length);
            IPEndPoint ipep = null;
            var response = LobbyMessage.Types.FromRegisterClient.Parser.ParseFrom(_udpClient.Receive(ref ipep));
            _clientInformation = response.UpdatedClientInfo;
        }
        
        public (LobbyClientInformation host, IEnumerable<LobbyClientInformation> clients) GetConnectedClients() {
            var request = new LobbyMessage.Types.ToGetFellowClients();
            request.SenderGuid = _clientInformation.Guid;
            var buf = request.GetBuffer();
            _udpClient.Send(buf, buf.Length);
            IPEndPoint ipep = null;
            var response = LobbyMessage.Types.FromGetFellowClients.Parser.ParseFrom(_udpClient.Receive(ref ipep));
            return (response.HostInfo, response.ClientsInfo);
        }
    }
}
