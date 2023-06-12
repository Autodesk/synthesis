using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Google.Protobuf;
using JetBrains.Annotations;
using SynthesisAPI.Aether;
using SynthesisAPI.Utilities;

#nullable enable

namespace SynthesisAPI.Aether.Lobby {
    public class LobbyClient : IDisposable {

        public string IP => _instance?.IP;
        private Inner? _instance;
        
        public LobbyClient(string ip, string name) {
            _instance = new Inner(ip, name);
        }

        private class Inner : IDisposable {

            public string IP;
            private LobbyClientHandler _handler;

            public Inner(string ip, string name) {
                IP = ip;

                var tcp = new TcpClient();
                tcp.Connect(ip, LobbyServer.TCP_PORT);

                var handlerRes = LobbyClientHandler.InitClientSide(tcp, name);
                if (handlerRes.isError)
                    throw handlerRes.GetError();
                _handler = handlerRes.GetResult();
            }

            public void Dispose() { }
            
        }

        public void Dispose() {
            _instance?.Dispose();
            _instance = null;
        }
        
    }
    
    public class LobbyClientHandler {

        private readonly string _name;
        public string Name => _name;
        private readonly ulong _guid;
        public ulong Guid => _guid;

        private TcpClient _tcp;
        private NetworkStream _stream; // Concurrency issues?
        
        private LobbyClientHandler(TcpClient tcp, string name, ulong guid) {
            _tcp = tcp;
            _name = name;
            _guid = Guid;
            _stream = _tcp.GetStream();
        }

        public Task<LobbyMessage?> ReadMessage()
            => ReadMessage(_stream);

        public void WriteMessage(LobbyMessage message)
            => WriteMessage(message, _stream);

        private static Task<LobbyMessage?> ReadMessage(NetworkStream stream) {
            return Task<LobbyMessage?>.Factory.StartNew(() => {
                try {
                    byte[] intBuf = new byte[4];
                    _ = stream.Read(intBuf, 0, 4);
                    int messageSize = BitConverter.ToInt32(intBuf, 0);
                    
                    byte[] messageBuf = new byte[messageSize];
                    _ = stream.Read(messageBuf, 0, messageSize);
                    return LobbyMessage.Parser.ParseFrom(messageBuf);
                } catch (Exception e) {
                    Logger.Log($"Read failure:\n{e.Message}\n{e.StackTrace}");
                    return null;
                }
            });
        }

        private const bool TRUE = true;
        
        private static Result<bool, Exception> WriteMessage(LobbyMessage message, NetworkStream stream) {
            try {
                int size = message.CalculateSize();
                stream.Write(BitConverter.GetBytes(size), 0, 4); 
                message.WriteTo(stream);
                stream.Flush();
                return new Result<bool, Exception>(TRUE);
            } catch (Exception e) {
                return new Result<bool, Exception>(e);
            }
        }

        // private int? ReadInt32(Stream s) {
        //     byte[] buf = new byte[4];
        //     int size = s.Read(buf, 0, 4);
        //     if (size != 4)
        //         return null;
        //     return BitConverter.ToInt32(buf, 0);
        // }

        public static Result<LobbyClientHandler, Exception> InitServerSide(TcpClient tcp, ulong guid) {
            var msgTask = ReadMessage(tcp.GetStream());
            msgTask.Wait();
            var msg = msgTask.Result;
            if (msg == null)
                return new Result<LobbyClientHandler, Exception>(new Exception("Failed to read request"));

            LobbyClientHandler? handler = null;

            switch (msg.MessageTypeCase) {
                case LobbyMessage.MessageTypeOneofCase.ToRegisterClient:
                    var info = msg.ToRegisterClient.ClientInfo;
                    if (info == null)
                        break;

                    info.Guid = guid;
                    var response = new LobbyMessage.Types.FromRegisterClient { UpdatedClientInfo = info };
                    if (WriteMessage(new LobbyMessage { FromRegisterClient = response }, tcp.GetStream()).isError)
                        break;

                    handler = new LobbyClientHandler(tcp, info.Name, info.Guid);

                    break;
            }

            return handler == null ?
                new Result<LobbyClientHandler, Exception>(new Exception("Failed to create ClientHandler")) :
                new Result<LobbyClientHandler, Exception>(handler);
        }

        public static Result<LobbyClientHandler, Exception> InitClientSide(TcpClient tcp, string name) {
            var request = new LobbyMessage.Types.ToRegisterClient { ClientInfo = new LobbyClientInformation { Name = name } };
            if (WriteMessage(new LobbyMessage { ToRegisterClient = request }, tcp.GetStream()).isError)
                return new Result<LobbyClientHandler, Exception>(new Exception("Failed to send Register request"));

            var msgTask = ReadMessage(tcp.GetStream());
            msgTask.Wait();
            var msg = msgTask.Result;
            if (msg == null)
                return new Result<LobbyClientHandler, Exception>(new Exception("Failed to read response"));

            LobbyClientHandler? handler = null;
            
            switch (msg.MessageTypeCase) {
                case LobbyMessage.MessageTypeOneofCase.FromRegisterClient:
                    var info = msg.FromRegisterClient.UpdatedClientInfo;
                    if (info == null)
                        break;

                    handler = new LobbyClientHandler(tcp, info.Name, info.Guid);

                    break;
            }

            return handler == null ?
                new Result<LobbyClientHandler, Exception>(new Exception("Failed to create ClientHandler")) :
                new Result<LobbyClientHandler, Exception>(handler);
        }
            
    }
}
