using Google.Protobuf;
using SynthesisAPI.Controller;
using SynthesisAPI.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace SynthesisAPI.Aether.Lobby {
    public class LobbyClient : IDisposable {

        private const long HEARTBEAT_FREQUENCY = 1000;

        public string IP => _instance == null ? string.Empty : _instance.IP;
        private Inner? _instance;

        public ulong? Guid => _instance?.Handler.Guid;
        public string Name => _instance?.Handler.Name ?? "--unknown--";
        public bool IsAlive => _instance != null;

        public List<DataRobot> RobotsFromServer => _instance?.RobotsFromServer ?? new List<DataRobot>();

        public LobbyClient(string ip, string name) {
            _instance = new Inner(ip, name);
        }

        public Task<LobbyMessage.Types.FromGetLobbyInformation?>? GetLobbyInformation() {
            return _instance?.GetLobbyInformation();
        }

        public Task<Result<LobbyMessage?, Exception>> UploadRobotData(DataRobot robot) 
            => _instance?.UploadRobotData(robot) ?? Task.FromResult(new Result<LobbyMessage?, Exception>(new Exception("No instance")));

        public Task<Result<LobbyMessage?, Exception>> RequestServerRobotData()
            => _instance?.RequestServerRobotData() ?? Task.FromResult(new Result<LobbyMessage?, Exception>(new Exception("No instance")));

        public Task<Result<LobbyMessage?, Exception>> UpdateControllableState(List<SignalData> updates)
            => _instance?.UpdateControllableState(updates) ?? Task.FromResult(new Result<LobbyMessage?, Exception>(new Exception("No instance")));

        public Task<Result<LobbyMessage?, Exception>> UpdateTransforms(List<ServerTransforms> transforms)
            => _instance?.UpdateTransforms(transforms) ?? Task.FromResult(new Result<LobbyMessage?, Exception>(new Exception("No instance")));

        private class Inner : IDisposable {

            private Atomic<bool> _isAlive = new Atomic<bool>(true);
            
            public readonly string IP;
            private readonly LobbyClientHandler _handler;
            public LobbyClientHandler Handler => _handler;

            private ReaderWriterLockSlim _transformDataLock;
            private Dictionary<ulong, ServerTransforms> _transformData;

            private ConcurrentQueue<Task<Result<LobbyMessage?, Exception>>> _requestQueue;

            private readonly Thread _heartbeatThread;
            private readonly Thread _requestSenderThread;

            public List<DataRobot> RobotsFromServer { get; private set; }

            public Inner(string ip, string name) {
                IP = ip;

                _transformData = new Dictionary<ulong, ServerTransforms>();
                _transformDataLock = new ReaderWriterLockSlim();

                _requestQueue = new ConcurrentQueue<Task<Result<LobbyMessage?, Exception>>>();

                var tcp = new TcpClient();
                tcp.Connect(ip, LobbyServer.TCP_PORT);

                var handlerRes = LobbyClientHandler.InitClientSide(tcp, name);
                if (handlerRes.isError)
                    throw handlerRes.GetError();
                _handler = handlerRes.GetResult();

                // TODO: Add lifetime stuff
                _heartbeatThread = new Thread(ClientHeartbeat);
                _heartbeatThread.Start();

                _requestSenderThread = new Thread(RequestQueueProcessor);
                _requestSenderThread.Start();

                RobotsFromServer = new List<DataRobot>();
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

            private void RequestQueueProcessor() {
                while (_isAlive) {
                    var success = _requestQueue.TryDequeue(out var task);
                    if (!success)
                        continue;

                    task.Start();
                    task.Wait();
                }
            }

            private void ClientHeartbeat() {
                long lastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                while (_isAlive) {
                    long currentTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    if (currentTime - lastUpdate > HEARTBEAT_FREQUENCY) {
                        var task = new Task<Result<LobbyMessage?, Exception>>(() => {
                            var res = _handler.WriteMessage(new LobbyMessage { ToClientHeartbeat = new LobbyMessage.Types.ToClientHeartbeat() });
                            if (res.isError)
                                return new Result<LobbyMessage?, Exception>(res.GetError());
                            return new Result<LobbyMessage?, Exception>(val: null);
                        });
                        _requestQueue.Enqueue(task);
                        
                        lastUpdate = currentTime;
                    } else {
                        Thread.Sleep(100);
                    }
                }
            }

            public Task<Result<LobbyMessage?, Exception>> UploadRobotData(DataRobot robot) {
                if (!_isAlive.Value)
                    return Task.FromResult(new Result<LobbyMessage?, Exception>(new Exception("Client no longer alive")));

                var request = new LobbyMessage.Types.ToDataRobot {
                    Guid = _handler.Guid,
                    DataRobot = robot
                };

                var task = new Task<Result<LobbyMessage?, Exception>>(() => {
                    var response = HandleResponseBoilerplate(new LobbyMessage { ToDataRobot = request });
                    if (response.isError) {
                        return response;
                    }

                    var msg = response.GetResult()!;
                    if (msg.MessageTypeCase != LobbyMessage.MessageTypeOneofCase.FromDataRobot) {
                        return new Result<LobbyMessage?, Exception>(new Exception("Invalid message"));
                    }

                    return new Result<LobbyMessage?, Exception>(msg);
                });

                _requestQueue.Enqueue(task);
                return task;
            }

            public Task<Result<LobbyMessage?, Exception>> RequestServerRobotData() {
                if (!_isAlive.Value)
                    return Task.FromResult(new Result<LobbyMessage?, Exception>(new Exception("Client no longer alive")));

                var request = new LobbyMessage.Types.ToRequestDataRobots {
                    Guid = _handler.Guid
                };

                var task = new Task<Result<LobbyMessage?, Exception>>(() => {
                    var response = HandleResponseBoilerplate(new LobbyMessage { ToRequestDataRobots = request });
                    if (response.isError) {
                        return response;
                    }

                    var msg = response.GetResult()!;
                    if (msg.MessageTypeCase != LobbyMessage.MessageTypeOneofCase.FromRequestDataRobots) {
                        return new Result<LobbyMessage?, Exception>(new Exception("Invalid message"));
                    }

                    RobotsFromServer = new List<DataRobot>(msg.FromRequestDataRobots.AllAvailableRobots);
                    return new Result<LobbyMessage?, Exception>(msg);
                });

                _requestQueue.Enqueue(task);
                return task;
            }

            public Task<Result<LobbyMessage?, Exception>> UpdateControllableState(List<SignalData> updates) {
                if (!_isAlive.Value)
                    return Task.FromResult(new Result<LobbyMessage?, Exception>(new Exception("Client no longer alive")));

                var request = new LobbyMessage.Types.ToUpdateControllableState {
                    Guid = _handler.Guid
                };
                request.Data.Add(updates);

                var task = new Task<Result<LobbyMessage?, Exception>>(() => {

                    var response = HandleResponseBoilerplate(new LobbyMessage { ToUpdateControllableState = request });
                    if (response.isError) {
                        return response;
                    }

                    var msg = response.GetResult()!;
                    switch (msg.MessageTypeCase) {
                        case LobbyMessage.MessageTypeOneofCase.FromSimulationTransformData:
                            // TODO: Update transform data
                            Logger.Log("Received transform response");
                            break;
                        default:
                            return new Result<LobbyMessage?, Exception>(new Exception("Invalid message"));
                    }

                    return new Result<LobbyMessage?, Exception>(msg);
                    
                });
                _requestQueue.Enqueue(task);
                return task;
            }

            public Task<Result<LobbyMessage?, Exception>> UpdateTransforms(List<ServerTransforms> transforms) {
                if (!_isAlive.Value)
                    return Task.FromResult(new Result<LobbyMessage?, Exception>(new Exception("Client no longer alive")));

                var request = new LobbyMessage.Types.ToUpdateTransformData();
                request.TransformData.AddRange(transforms);

                var task = new Task<Result<LobbyMessage?, Exception>>(() => {

                    var response = HandleResponseBoilerplate(new LobbyMessage { ToUpdateTransformData = request });
                    if (response.isError) {
                        return response;
                    }

                    var msg = response.GetResult()!;
                    switch (msg.MessageTypeCase) {
                        case LobbyMessage.MessageTypeOneofCase.FromControllableStates:
                            // TODO: Update signal data
                            Logger.Log("Received controllable state response");
                            break;
                        default:
                            return new Result<LobbyMessage?, Exception>(new Exception("Invalid message"));
                    }

                    return new Result<LobbyMessage?, Exception>(msg);

                });
                _requestQueue.Enqueue(task);
                return task;
            }

            /// <summary>
            /// TODO: Rename
            /// </summary>
            private Result<LobbyMessage?, Exception> HandleResponseBoilerplate(LobbyMessage request) {
                var writeResult = _handler.WriteMessage(request);
                if (writeResult.isError)
                    return new Result<LobbyMessage?, Exception>(writeResult.GetError());

                var readResult = _handler.ReadMessage();
                var completedBeforeTimeout = readResult.Wait(1000);
                if (!completedBeforeTimeout)
                    return new Result<LobbyMessage?, Exception>(new Exception("Task Timeout"));
                else if (readResult.Result.isError)
                    return new Result<LobbyMessage?, Exception>(readResult.Result.GetError());

                return new Result<LobbyMessage?, Exception>(readResult.Result.GetResult());
            }

            public void Dispose() {
                _isAlive.Value = false;
                _heartbeatThread.Join();
                _requestSenderThread.Join();
                _handler.Dispose();
			}

		}

        public void Dispose() {
            _instance?.Dispose();
            _instance = null;
        }
        
    }
    
    internal class LobbyClientHandler : IDisposable {

        private const int READ_TIMEOUT_MS = 10000;
        private const int READ_BUFFER_SIZE = 2048;

        private readonly LobbyClientInformation _clientInformation;
        public LobbyClientInformation ClientInformation => _clientInformation.Clone();

        public DateTime? LastHeartbeat { get; private set; } // Milliseconds

        public ulong Guid => _clientInformation.Guid;
        public string Name => _clientInformation.Name;

        private readonly TcpClient _tcp;
        private NetworkStream _stream => _tcp.GetStream(); // Concurrency issues?
        private readonly Mutex _streamLock;
        
        private LobbyClientHandler(TcpClient tcp, string name, ulong guid) {
            _tcp = tcp;
            _clientInformation = new LobbyClientInformation { Name = name, Guid = guid };
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
                bool isLocked = false;
				try {

                    DateTime startedRead = DateTime.UtcNow;
                    while (!stream.DataAvailable && (DateTime.UtcNow - startedRead).TotalMilliseconds < READ_TIMEOUT_MS) {
                        Thread.Sleep(50);
                    }

                    if (!stream.DataAvailable) {
                        result = new Result<LobbyMessage, ServerReadException>(new NoDataException());
                        throw result.GetError();
                    }

                    mutex?.WaitOne();
                    isLocked = true;

                    var intBuf = new byte[4];
                    stream.Read(intBuf, 0, 4);
                    int msgSize = BitConverter.ToInt32(intBuf, 0);

                    var msgBuf = new byte[msgSize];
                    int bytesRead = 0;
                    var startRead = DateTime.UtcNow;
                    while (bytesRead != msgSize && (DateTime.UtcNow - startRead).TotalMilliseconds < READ_TIMEOUT_MS) {
                        bytesRead += stream.Read(msgBuf, bytesRead, msgSize - bytesRead);
                        Thread.Sleep(10);
                    }

                    if (bytesRead != msgSize) {
                        Logger.Log($"Mismatch of read bytes. Expected '{msgSize}', read '{bytesRead}'");
                    }
                    LobbyMessage msg = LobbyMessage.Parser.ParseFrom(msgBuf);

                    mutex?.ReleaseMutex();

                    result = new Result<LobbyMessage, ServerReadException>(msg);

                } catch (Exception e) {
                    if (result == null) {
                        if (isLocked) {
                            mutex?.ReleaseMutex();
                        }

                        result = new Result<LobbyMessage, ServerReadException>(
                            new ServerReadException($"Read failure:\n{e.Message}\n{e.StackTrace}")
                        );
                    }
                }

                return result;
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

                return new Result<bool, Exception>(TRUE);
            } catch (Exception e) {
                return new Result<bool, Exception>(e);
            } finally {
                mutex?.ReleaseMutex();
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
            int time = LastHeartbeat.HasValue ? (int)System.Math.Round((DateTime.UtcNow - LastHeartbeat.Value).TotalMilliseconds) : -1;
            return $"[{ClientInformation.Guid}] {ClientInformation.Name} <- {time}ms";
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
        public class NoDataException : ServerReadException {
            public NoDataException() : base("No Data") { }
        }
    }
}
