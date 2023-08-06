﻿using Google.Protobuf;
using SynthesisAPI.Controller;
using SynthesisAPI.Utilities;
using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace SynthesisAPI.Aether.Lobby {

    /// <summary>
    /// LobbyClient establishes a connection to the LobbyServer and creates channels for communication
    /// of lobby updates as well as data exchanging between clients
    /// </summary>
    public class LobbyClient : IDisposable {

        private static readonly Result<LobbyMessage?, Exception> NO_INSTANCE_ERROR_RESULT
            = new Result<LobbyMessage?, Exception>(new Exception("No instance"));

        private const long HEARTBEAT_FREQUENCY = 1000;

        public string IP => _instance == null ? string.Empty : _instance.IP;
        private Inner? _instance;

        public ulong? Guid => _instance?.Handler.Guid;
        public string Name => _instance?.Handler.Name ?? "--unknown--";
        public bool IsAlive => _instance != null;

        /// <summary>
        /// Constructs a LobbyClient that will connect to <paramref name="ip" /> and attempt
        /// to set the client's lobby name to <paramref name="name" />.
        /// </summary>
        /// <param name="ip">IP to connect to. (Format: ###.###.###.###)</param>
        /// <param name="name">Username to attempt. Can potentially be corrected by Server</param>
        public LobbyClient(string ip, string name) {
            _instance = new Inner(ip, name);
        }

#region Public Instance Functions

#region Generic Requests

        /// <summary>
        /// Get Lobby Information from currently connected Lobby.
        /// </summary>
        /// <returns>LobbyInformation, mostly including a list of other clients and their information</returns>
        public Task<Result<LobbyMessage?, Exception>> GetLobbyInformation() {
            return _instance?.GetLobbyInformation() ?? Task.FromResult(NO_INSTANCE_ERROR_RESULT);
        }

#endregion

#region Runtime Requests

        /// <summary>
        /// Sends signal updates to the server to receive transform data for the rest of the clients
        /// </summary>
        /// <param name="updates">List of signal updates</param>
        /// <returns>Task that gives you the raw response</returns>
        public Task<Result<LobbyMessage?, Exception>> UpdateControllableState(List<SignalData> updates)
            => _instance?.UpdateControllableState(updates) ?? Task.FromResult(NO_INSTANCE_ERROR_RESULT);

        /// <summary>
        /// Sends transform data for all clients in the HostSimulation in exchange for the newest signal data (or updates I can't remember)
        /// </summary>
        /// <param name="transforms">Lists of transformations of all client robots</param>
        /// <returns>Task that gives you the raw response</returns>
        public Task<Result<LobbyMessage?, Exception>> UpdateTransforms(List<ServerTransforms> transforms)
            => _instance?.UpdateTransforms(transforms) ?? Task.FromResult(NO_INSTANCE_ERROR_RESULT);

#endregion

#region Data Handling Requests

        /// <summary>
        /// 
        /// </summary>
        /// <param name="description"></param>
        /// <returns>Task that gives you the raw response</returns>
        public Task<Result<LobbyMessage?, Exception>> MakeDataAvailable(SynthesisDataDescriptor description)
            => _instance?.MakeDataAvailable(description) ?? Task.FromResult(NO_INSTANCE_ERROR_RESULT);

        /// <summary>
        /// 
        /// </summary>
        /// <returns>Task that gives you the raw response</returns>
        public Task<Result<LobbyMessage?, Exception>> GetAllAvailableData()
            => _instance?.GetAllAvailableData() ?? Task.FromResult(NO_INSTANCE_ERROR_RESULT);

        public Task<Result<LobbyMessage?, Exception>> UploadData(SynthesisData data)
            => _instance?.UploadData(data) ?? Task.FromResult(NO_INSTANCE_ERROR_RESULT);

#endregion

#endregion

        private class Inner : IDisposable {

            private readonly Atomic<bool> _isAlive = new Atomic<bool>(true);

            private static readonly Result<LobbyMessage?, Exception> CLIENT_NOT_ALIVE_ERROR_RESULT
                = new Result<LobbyMessage?, Exception>(new Exception("Client no longer alive"));
            
            public readonly string IP;
            private readonly LobbyClientHandler _handler;
            public LobbyClientHandler Handler => _handler;

            public ReaderWriterLockSlim TransformDataLock;
            public Dictionary<ulong, ServerTransforms> TransformData;

            private readonly ConcurrentQueue<Task<Result<LobbyMessage?, Exception>>> _requestQueue;

            private readonly Thread _heartbeatThread;
            private readonly Thread _requestSenderThread;

            public Inner(string ip, string name) {
                IP = ip;

                TransformData = new Dictionary<ulong, ServerTransforms>();
                TransformDataLock = new ReaderWriterLockSlim();

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
            }

            ~Inner() {
                if (_isAlive)
                    Dispose();
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

#region Generic Request Handling

            public Task<Result<LobbyMessage?, Exception>> GetLobbyInformation() {
                if (!_isAlive.Value)
                    return Task.FromResult(CLIENT_NOT_ALIVE_ERROR_RESULT);

                var request = new LobbyMessage.Types.ToGetLobbyInformation {
                    SenderGuid = Handler.Guid
                };

                var task = new Task<Result<LobbyMessage?, Exception>>(() => {
                    var response = HandleResponseBoilerplate(
                        new LobbyMessage { ToGetLobbyInformation = request },
                        LobbyMessage.MessageTypeOneofCase.FromGetLobbyInformation
                    );

                    return response;
                });

                _requestQueue.Enqueue(task);
                return task;
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

#endregion

#region Shared Data Handling

            public Task<Result<LobbyMessage?, Exception>> MakeDataAvailable(SynthesisDataDescriptor description) {
                if (!_isAlive.Value)
                    return Task.FromResult(CLIENT_NOT_ALIVE_ERROR_RESULT);

                var request = new LobbyMessage.Types.ToMakeDataAvailable {
                    Description = description
                };

                var task = new Task<Result<LobbyMessage?, Exception>>(() => {
                    var response = HandleResponseBoilerplate(
                        new LobbyMessage { ToMakeDataAvailable = request },
                        LobbyMessage.MessageTypeOneofCase.FromMakeDataAvailableConfirmation
                    );

                    return response;
                });

                _requestQueue.Enqueue(task);
                return task;
            }

            public Task<Result<LobbyMessage?, Exception>> GetAllAvailableData() {
                if (!_isAlive.Value)
                    return Task.FromResult(CLIENT_NOT_ALIVE_ERROR_RESULT);

                var request = new LobbyMessage.Types.ToAllDataAvailable();

                var task = new Task<Result<LobbyMessage?, Exception>>(() => {
                    var response = HandleResponseBoilerplate(
                        new LobbyMessage { ToAllDataAvailable = request },
                        LobbyMessage.MessageTypeOneofCase.FromAllDataAvailable
                    );

                    return response;
                });

                _requestQueue.Enqueue(task);
                return task;
            }

            public Task<Result<LobbyMessage?, Exception>> UploadData(SynthesisData data) {
                if (!_isAlive.Value)
                    return Task.FromResult(CLIENT_NOT_ALIVE_ERROR_RESULT);

                var request = new LobbyMessage.Types.ToUploadSynthesisData {
                    Data = data
                };

                var task = new Task<Result<LobbyMessage?, Exception>>(() => {
                    var response = HandleResponseBoilerplate(
                        new LobbyMessage { ToUploadSynthesisData = request },
                        LobbyMessage.MessageTypeOneofCase.FromUploadSynthesisDataConfirmation
                    );

                    return response;
                });

                _requestQueue.Enqueue(task);
                return task;
            }

#endregion

#region Runtime Handling

            public Task<Result<LobbyMessage?, Exception>> UpdateControllableState(List<SignalData> updates) {
                if (!_isAlive.Value)
                    return Task.FromResult(CLIENT_NOT_ALIVE_ERROR_RESULT);

                var request = new LobbyMessage.Types.ToUpdateControllableState {
                    Guid = _handler.Guid
                };
                request.Data.Add(updates);

                var task = new Task<Result<LobbyMessage?, Exception>>(() => {
                    var response = HandleResponseBoilerplate(
                        new LobbyMessage { ToUpdateControllableState = request },
                        LobbyMessage.MessageTypeOneofCase.FromSimulationTransformData
                    );

                    return response;
                    
                });
                _requestQueue.Enqueue(task);
                return task;
            }

            public Task<Result<LobbyMessage?, Exception>> UpdateTransforms(List<ServerTransforms> transforms) {
                if (!_isAlive.Value)
                    return Task.FromResult(CLIENT_NOT_ALIVE_ERROR_RESULT);

                var request = new LobbyMessage.Types.ToUpdateTransformData();
                request.TransformData.AddRange(transforms);

                var task = new Task<Result<LobbyMessage?, Exception>>(() => {
                    var response = HandleResponseBoilerplate(
                        new LobbyMessage { ToUpdateTransformData = request },
                        LobbyMessage.MessageTypeOneofCase.FromControllableStates
                    );

                    return response;
                });
                _requestQueue.Enqueue(task);
                return task;
            }

#endregion

            /// <summary>
            /// TODO: Rename
            /// </summary>
            private Result<LobbyMessage?, Exception> HandleResponseBoilerplate(LobbyMessage request, LobbyMessage.MessageTypeOneofCase expectedMessageType) {
                var writeResult = _handler.WriteMessage(request);
                if (writeResult.isError)
                    return new Result<LobbyMessage?, Exception>(writeResult.GetError());

                var readResult = _handler.ReadMessage();
                var completedBeforeTimeout = readResult.Wait(1000);
                if (!completedBeforeTimeout)
                    return new Result<LobbyMessage?, Exception>(new Exception("Task Timeout"));
                else if (readResult.Result.isError)
                    return new Result<LobbyMessage?, Exception>(readResult.Result.GetError());

                var msg = readResult.Result.GetResult();
                if (msg == null) {
                    return new Result<LobbyMessage?, Exception>(new Exception("Invalid message"));
                } else if (msg.MessageTypeCase != expectedMessageType) {
                    return new Result<LobbyMessage?, Exception>(new Exception(
                        $"Invalid message type: '{Enum.GetName(typeof(LobbyMessage.MessageTypeOneofCase), msg.MessageTypeCase)}'"
                    ));
                }

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

        private readonly ReaderWriterLockSlim _clientInfoLock = new ReaderWriterLockSlim();
        private readonly LobbyClientInformation _clientInformation;
        public LobbyClientInformation ClientInformation {
            get {
                _clientInfoLock.EnterReadLock();
                var clone = _clientInformation.Clone();
                _clientInfoLock.ExitReadLock();
                return clone;
            }
        }

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

        public void AddSelection(ClientSelection selection) {
            _clientInfoLock.EnterWriteLock();
            _clientInformation.Selections.Add(selection.SelectionId, selection);
            _clientInfoLock.ExitWriteLock();
        }

        public bool RemoveSelection(string selectionID) {
            _clientInfoLock.EnterWriteLock();
            var res = _clientInformation.Selections.Remove(selectionID);
            _clientInfoLock.ExitWriteLock();
            return res;
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

                    // DateTime startedRead = DateTime.UtcNow;
                    // while (!stream.DataAvailable && (DateTime.UtcNow - startedRead).TotalMilliseconds < READ_TIMEOUT_MS) {
                    //     Thread.Sleep(50);
                    // }

                    // if (!stream.DataAvailable) {
                    //     result = new Result<LobbyMessage, ServerReadException>(new NoDataException());
                    //     throw result.GetError();
                    // }

                    mutex?.WaitOne();
                    isLocked = true;

                    var intBuf = new byte[4];
                    stream.Read(intBuf, 0, 4);
                    int msgSize = BitConverter.ToInt32(intBuf, 0);

                    var msgBuf = new byte[msgSize];
                    int bytesRead = 0;
                    while (bytesRead < msgSize) {
                        bytesRead += stream.Read(msgBuf, bytesRead, msgSize - bytesRead);
                    }

                    if (bytesRead != msgSize) {
                        Logger.Log($"Mismatch of read bytes. Expected '{msgSize}', read '{bytesRead}'");
                    }
                    LobbyMessage msg = LobbyMessage.Parser.ParseFrom(msgBuf);
                    
                    result = new Result<LobbyMessage, ServerReadException>(msg);

                } catch (IOException) {
                    throw new NoDataException();
                } catch (Exception e) {
                    if (result == null) {
                        result = new Result<LobbyMessage, ServerReadException>(
                            new ServerReadException($"Read failure:\n{e.Message}\n{e.StackTrace}")
                            );
                    }
                } finally {
                    if (isLocked) {
                        mutex?.ReleaseMutex();
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
            return $"[{ClientInformation.Guid}] {ClientInformation.Name} <- ({time}ms)";
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
