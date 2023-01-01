using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

#nullable enable

namespace SynthesisAPI.WS {

    public class WebSocketServer {

        private TcpListener _listener;
        private Mutex _clientDictMut = new Mutex();

        private Dictionary<Guid, WSClientHandler> _clientDict = new Dictionary<Guid, WSClientHandler>();
        public IReadOnlyList<Guid> Clients => _clientDict.Keys.ToList().AsReadOnly();

        public event Action<Guid, string> OnMessage;
        public event Action<Guid> OnConnect;
        public event Action<Guid> OnDisconnect;

        public WebSocketServer(string hostname, int port) {
            _listener = new TcpListener(IPAddress.Parse(hostname), port);
            _listener.Start(2);
            _listener.BeginAcceptTcpClient(AcceptTcpClient, null);
        }

        ~WebSocketServer() {
            _listener.Stop();
        }

        public void Close() {
            _listener.Stop();
            _clientDict.ForEach(x => x.Value.Kill());
            _clientDict.Clear();
        }

        private void AcceptTcpClient(IAsyncResult ar) {
            var clientTcp = _listener.EndAcceptTcpClient(ar);

            _listener.BeginAcceptTcpClient(AcceptTcpClient, null);
            
            if (!clientTcp.Connected) {
                return;
            }

            _clientDictMut.WaitOne();
            var client = new WSClientHandler(Guid.NewGuid(), clientTcp);
            _clientDict.Add(client.GUID, client);
            _clientDictMut.ReleaseMutex();

            if (OnConnect != null)
                OnConnect(client.GUID);

            while (clientTcp.Available < 3) ;
            byte[] bytes = new byte[clientTcp.Available];
            clientTcp.GetStream().Read(bytes, 0, clientTcp.Available);
            string request = Encoding.UTF8.GetString(bytes);

            // Console.WriteLine(request);

            string eol = "\r\n"; // Apparently this is a thing
            string response = "HTTP/1.1 101 Switching Protocols\n";

            response += "Connection: Upgrade\n"
                        + "Upgrade: websocket\n";

            string key = request.Substring(request.IndexOf("Sec-WebSocket-Key: ") + 19);
            key = key.Substring(0, key.IndexOf("==") + 2);
            string what = key + "258EAFA5-E914-47DA-95CA-C5AB0DC85B11";
            // Console.WriteLine(what);
            var wsAccept = Convert.ToBase64String(
                System.Security.Cryptography.SHA1.Create().ComputeHash(
                    Encoding.UTF8.GetBytes(what)
                )
            );

            response += "Sec-WebSocket-Accept: " + wsAccept + eol;
            response += eol;


            client.Write(response);

            try {
                while (client.Connected) {

                    var frame = client.ReadWS();

                    if (OnMessage != null)
                        OnMessage(client.GUID, frame.ParseAsPlainText());
                }
            } catch (IOException _) {
            } finally {
                _clientDictMut.WaitOne();
                if (_clientDict.ContainsKey(client.GUID))
                    _clientDict[client.GUID].Kill();
                _clientDict.Remove(client.GUID);
                _clientDictMut.ReleaseMutex();
                if (OnDisconnect != null)
                    OnDisconnect(client.GUID);
            }
        }

        public void SendToClient(Guid client, string message) {
            if (!_clientDict.ContainsKey(client))
                throw new Exception($"No client with guid '{client}'");
            _clientDict[client].WriteWS(message);
        }
    }

    public struct WSFrame {

        public bool finished;
        public int opcode;
        public byte[] payload;

        public static WSFrame ParseNextFrame(NetworkStream stream) {
            byte[] buff = new byte[2];
            stream.Read(buff, 0, 2);

            WSFrame frame;

            var bitfield = buff[0];
            // Console.WriteLine(bitfield);
            bool fin = (bitfield & 0x80) != 0;
            int opcode = (bitfield & 0x0F);
            bool mask = (buff[1] & 0x80) != 0;
            ulong payloadLength = (ulong)(buff[1] & 0x7F);

            frame.opcode = opcode;
            frame.finished = fin;

            if (payloadLength == 126) {
                buff = new byte[2];
                stream.Read(buff, 0, 2);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(buff);
                payloadLength = BitConverter.ToUInt16(buff, 0);
            } else if (payloadLength == 127) {
                buff = new byte[8];
                stream.Read(buff, 0, 8);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(buff);
                payloadLength = BitConverter.ToUInt64(buff, 0); // I can't actually do a payload of more than 32 signed int long
            }

            if (mask) {
                byte[] maskBytes = new byte[4];
                stream.Read(maskBytes, 0, 4);
                frame.payload = new byte[payloadLength];
                for (uint i = 0; i < payloadLength; i++) {
                    frame.payload[i] = (byte)(stream.ReadByte() ^ maskBytes[i % 4]);
                }
            } else {
                frame.payload = new byte[payloadLength];
                if (payloadLength < UInt16.MaxValue) {
                    stream.Read(frame.payload, 0, (int)payloadLength);
                } else {
                    for (uint i = 0; i < payloadLength; i++) {
                        frame.payload[i] = (byte)stream.ReadByte();
                    }
                }
            }

            return frame;
        }

        public static byte[] Make(string message) {
            byte[] payload = Encoding.UTF8.GetBytes(message);

            List<byte> buff = new List<byte>();

            buff.Add(0x81);

            if (payload.Length > UInt16.MaxValue) { // 65536+
                buff.Add(0x7f);
                byte[] sizeBytes = BitConverter.GetBytes(payload.Length);
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(sizeBytes);
                }
                buff.AddRange(new byte[4]);
                buff.AddRange(sizeBytes);
            } else if (payload.Length > 126 && payload.Length <= UInt16.MaxValue) { // 126 - 65535
                buff.Add(0x7e);
                byte[] sizeBytes = BitConverter.GetBytes(payload.Length);
                if (BitConverter.IsLittleEndian) {
                    Array.Reverse(sizeBytes);
                }
                buff.AddRange(sizeBytes.Skip(2));
            } else { // 0 - 125
                buff.Add((byte)payload.Length);
            }

            buff.AddRange(payload);

            return buff.ToArray();
        }

        public string ParseAsPlainText() {
            return Encoding.UTF8.GetString(payload);
            //if (payload.Length < 126) {
            //    return ""; // 
            //} else {
            //    return $"Too Volatile\nSize {payload.Length}";
            //}
        }
    }

    public class WSClientHandler {

        public Guid GUID { get; private set; }
        private TcpClient _client;
        private NetworkStream _stream;

        public int Available => _client.Available;
        public bool Connected => _client.Connected;

        public WSClientHandler(Guid guid, TcpClient client) {
            GUID = guid;
            _client = client;
            _stream = client.GetStream();
        }

        public void Write(string message) {
            Write(Encoding.UTF8.GetBytes(message));
        }

        public void Write(byte[] buff) {
            _stream.Write(buff, 0, buff.Length);
            _stream.Flush();
        }

        public void WriteWS(string message) {
            byte[] buff = WSFrame.Make(message);
            Write(buff);
        }

        public void Read(byte[] buff, int offset, int length) {
            _stream.Read(buff, offset, length);
        }

        public WSFrame ReadWS()
            => WSFrame.ParseNextFrame(_stream);

        public void Kill() {
            _stream.Close();
            _client.Close();
        }
    }

}
