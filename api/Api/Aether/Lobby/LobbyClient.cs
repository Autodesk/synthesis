using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Google.Protobuf;
using SynthesisAPI.Controller;
using SynthesisAPI.Utilities;
using SynthesisServer.Proto;

#nullable enable

namespace SynthesisAPI.Aether.Lobby {
    public class LobbyClient : IDisposable {

        private const long HEARTBEAT_FREQUENCY = 1000;

        public string IP => _instance == null ? string.Empty : _instance.IP;
        private Inner? _instance;
        
        public LobbyClient(string ip, string name) {
            _instance = new Inner(ip, name);
        }

        public Task<LobbyMessage.Types.FromGetLobbyInformation?>? GetLobbyInformation() {
            return _instance?.GetLobbyInformation();
        }

        public void SendTestSignalData(SignalData data) => _instance?.SendTestSignalData(data);

        private class Inner : IDisposable {

            private Atomic<bool> _isAlive = new Atomic<bool>(true);
            
            public readonly string IP;
            private readonly LobbyClientHandler _handler;

            private readonly Thread _heartbeatThread;

            public Inner(string ip, string name) {
                IP = ip;

                var tcp = new TcpClient();
                tcp.Connect(ip, LobbyServer.TCP_PORT);

                var handlerRes = LobbyClientHandler.InitClientSide(tcp, name);
                if (handlerRes.isError)
                    throw handlerRes.GetError();
                _handler = handlerRes.GetResult();

                // TODO: Add lifetime stuff
                _heartbeatThread = new Thread(ClientHeartbeat);
                _heartbeatThread.Start((_isAlive, _handler));
            }

            ~Inner() {
                if (_isAlive)
                    Dispose();
            }

            public Task<LobbyMessage.Types.FromGetLobbyInformation?>? GetLobbyInformation() {
                var msg = new LobbyMessage{ 
                    ToGetLobbyInformation = new LobbyMessage.Types.ToGetLobbyInformation{
                        SenderGuid = _handler.Guid
                    }
                };

                if (_handler.WriteMessage(msg).isError) {
                    return null;
                }

                return Task<LobbyMessage.Types.FromGetLobbyInformation?>.Factory.StartNew(() => {
                    var msgTask = _handler.ReadMessage();
                    msgTask.Wait();
                    var msgRes = msgTask.Result;

                    if (msgRes.isError) {
                        Logger.Log($"Failed to Read: [{msgRes.GetError().GetType().Name}] {msgRes.GetError().Message}\n\n{msgRes.GetError().StackTrace}");
                    }

                    var msg = msgRes.GetResult();

                    if (msg.MessageTypeCase != LobbyMessage.MessageTypeOneofCase.FromGetLobbyInformation) {
                        return null;
                    }

                    return msg.FromGetLobbyInformation;
                });
            }

            private static void ClientHeartbeat(object parameters) {
                (var isAlive, var handler) = ((Atomic<bool>, LobbyClientHandler))parameters;
                long lastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                while (isAlive) {
                    long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    if (currentTime - lastUpdate > HEARTBEAT_FREQUENCY) {
                        var res = handler.WriteMessage(new LobbyMessage());
                        if (res.isError)
                            Logger.Log($"[{handler.Name}] Failed to sent heartbeat");
                        lastUpdate = currentTime;
                    } else {
                        Thread.Sleep(100);
                    }
                }

                Logger.Log($"Heartbeat for client '{handler.Name}' is ending");
            }

            public void SendTestSignalData(SignalData signal) {

                if (!_isAlive)
                    return;

                var request = new LobbyMessage.Types.ToUpdateControllableState {
                    Guid = _handler.Guid
                };
                request.Data.Add(signal);
                _handler.WriteMessage(new LobbyMessage { ToUpdateControllableState = request });
            }

            public void Dispose() {

                Logger.Log($"Client '{_handler.Name}' is Disposing...");

                _isAlive.Value = false;
                _handler.Dispose();
                _heartbeatThread.Join();

				Logger.Log($"Client '{_handler.Name}' has been Disposed");
			}

		}

        public void Dispose() {
            _instance?.Dispose();
            _instance = null;
        }
        
    }
    
    internal class LobbyClientHandler : IDisposable {

        private const int READ_TIMEOUT_MS = 1000;
        private const int READ_BUFFER_SIZE = 2048;

        private readonly LobbyClientInformation _clientInformation;
        public LobbyClientInformation ClientInformation => _clientInformation.Clone();

        public DateTime LastHeartbeat { get; private set; } // Milliseconds

        public ulong Guid => _clientInformation.Guid;
        public string Name => _clientInformation.Name;

        private readonly TcpClient _tcp;
        private readonly NetworkStream _stream; // Concurrency issues?
        private readonly Mutex _streamLock;
        
        private LobbyClientHandler(TcpClient tcp, string name, ulong guid) {
            _tcp = tcp;
            _clientInformation = new LobbyClientInformation { Name = name, Guid = guid };
            _stream = _tcp.GetStream();
            _streamLock = new Mutex();
        }

        public void UpdateHeartbeat() {
            LastHeartbeat = DateTime.UtcNow;
        }

        public Task<Result<LobbyMessage, ServerReadException>> ReadMessage()
            => ReadMessage(_stream, _streamLock);

        public Result<bool, Exception> WriteMessage(LobbyMessage message)
            => WriteMessage(message, _stream, _streamLock);

        private static Task<Result<LobbyMessage, ServerReadException>> ReadMessage(NetworkStream stream, Mutex? mutex = null) {
            return Task<Result<LobbyMessage, ServerReadException>>.Factory.StartNew(() => {
                Result<LobbyMessage, ServerReadException>? result = null;
				try {

                    int s

                } catch (Exception e) {
                    if (result == null) {
                        result = new Result<LobbyMessage, ServerReadException>(
                            new ServerReadException($"Read failure:\n{e.Message}\n{e.StackTrace}")
                        );
                    }
                }

                return result!;
            });
        }

        private const bool TRUE = true;
        
        private static Result<bool, Exception> WriteMessage(LobbyMessage message, NetworkStream stream, Mutex? mutex = null) {
            try {
                int size = message.CalculateSize();
                mutex?.WaitOne();
                stream.Write(BitConverter.GetBytes(size), 0, 4); 
                message.WriteTo(stream);
                stream.Flush();
                mutex?.ReleaseMutex();
                return new Result<bool, Exception>(TRUE);
            } catch (Exception e) {
                return new Result<bool, Exception>(e);
            }
        }

        public static Result<LobbyClientHandler, Exception> InitServerSide(TcpClient tcp, ulong guid) {

            tcp.GetStream().ReadTimeout = READ_TIMEOUT_MS;

            var msgTask = ReadMessage(tcp.GetStream());
            msgTask.Wait();
            var msgRes = msgTask.Result;
            if (msgRes.isError) {
                Logger.Log($"Failed to Read: [{msgRes.GetError().GetType().Name}] {msgRes.GetError().Message}\n\n{msgRes.GetError().StackTrace}");
				return new Result<LobbyClientHandler, Exception>(new Exception("Failed to read request"));
			}

            var msg = msgRes.GetResult();

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

            tcp.GetStream().ReadTimeout = READ_TIMEOUT_MS;

            var request = new LobbyMessage.Types.ToRegisterClient { ClientInfo = new LobbyClientInformation { Name = name } };
            if (WriteMessage(new LobbyMessage { ToRegisterClient = request }, tcp.GetStream()).isError)
                return new Result<LobbyClientHandler, Exception>(new Exception("Failed to send Register request"));

            var msgTask = ReadMessage(tcp.GetStream());
            msgTask.Wait();
            var msgRes = msgTask.Result;
            if (msgRes.isError) {
                Logger.Log($"Failed to Read: [{msgRes.GetError().GetType().Name}] {msgRes.GetError().Message}\n\n{msgRes.GetError().StackTrace}");
				return new Result<LobbyClientHandler, Exception>(new Exception("Failed to read response"));
			}

            var msg = msgRes.GetResult();

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

        public override string ToString() {
            return $"[{ClientInformation.Guid}] {ClientInformation.Name} <- {System.Math.Round((DateTime.UtcNow - LastHeartbeat).TotalMilliseconds)}ms";
        }

        public void Dispose() {
            _tcp.Dispose();
        }

        public class ServerReadException : Exception {
            public ServerReadException() { }
            public ServerReadException(string msg) : base(msg) { }
        }
        public class ReadTimeoutException : ServerReadException {
            public ReadTimeoutException() : base("Timeout") { }
        }
    }
}
