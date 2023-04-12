// using System;
// using System.Threading;
// using System.Threading.Tasks;
// using SynthesisAPI.EventBus;
// using SynthesisAPI.Utilities;
// using SynthesisServer.Client;
// using UnityEngine;
// using Synthesis.PreferenceManager;
// using SynthesisServer.Proto;
// using SynthesisAPI.Simulation;
// using Synthesis.Runtime;
// using Org.BouncyCastle.Crypto.Parameters;
// using Newtonsoft.Json;
//
// using Logger = SynthesisAPI.Utilities.Logger;
//
// using PClient = SynthesisServer.Proto.Client;
// using Client = SynthesisServer.Client.Client;
// using Org.BouncyCastle.Math;
//
// #nullable enable
//
// namespace Synthesis.Networking {
//     public static class NetworkManager {
//
//         public static ConnectionStatus Status => Instance.Status;
//         public static ServerInfoResponse? LastServerInfo => Instance.LastServerInfo;
//
//         public static bool ConnectToServerAsync(string ip, string name)
//             => Instance.ConnectToServerAsync(ip, name);
//
//         public static void DisconnectFromServer() {
//             Instance.DisconnectFromServer();
//         }
//
//         public static bool CreateLobbyAsync(string name, Action<CreateLobbyResponse> callback)
//             => Instance.CreateLobbyAsync(name, callback);
//
//         public static bool UpdateServerInfoAsync(Action<ServerInfoResponse> callback)
//             => Instance.UpdateServerInfoAsync(callback);
//
//         public static void Init() {
//             _innerInstance = null;
//
//             // TODO
//             _ = Instance;
//         }
//
//         public static void Kill() {
//             _innerInstance = null;
//         }
//
//         private class Inner : IClientHandler {
//
//             // public bool IsConnected => Client.Instance.IsTcpConnected;
//
//             // private MyClientHandler _clientHandler;
//
//             public ConnectionStatus Status = ConnectionStatus.NotConnected;
//
//             public ServerInfoResponse? LastServerInfo { get; private set; }
//
//             private Action<CreateLobbyResponse>? _createLobbyCallback;
//             private Action<DeleteLobbyResponse>? _deleteLobbyCallback;
//             private Action<ServerInfoResponse>? _serverInfoCallback;
//
//             private const string DH_PARAMS_PREF = "dh_params";
//             public Inner() {
//                 Client.ErrorReport += s => Debug.Log($"Client Error: {s}");
//                 // TODO: Save the parameters for like a week or something
//                 Client.Instance.Init(
//                     this,
//                     PreferenceManager.PreferenceManager.ContainsPreference(DH_PARAMS_PREF)
//                         ? PreferenceManager.PreferenceManager.GetPreference<DHParameters>(DH_PARAMS_PREF)
//                         : null
//                 );
//
//                 SimulationRunner.OnUpdate += Update;
//             }
//
//             private Task<bool>? _connectToServerTask = null;
//             public bool ConnectToServerAsync(string _ip, string _name) {
//
//                 if (_connectToServerTask != null)
//                     return false;
//                 if (Client.Instance.IsTcpConnected)
//                     return false;
//
//                 _connectToServerTask = Task.Factory.StartNew(() => {
//                     string ip = _ip;
//                     string name = _name;
//                     if (Client.Instance.IsTcpConnected)
//                         return false;
//
//                     _ = Client.Instance.Parameters; // Wait for parameters to be available
//
//                     var res = Client.Instance.Start(ip);
//                     if (res) {
//                         // Heartbeat. Runs independent of everything else so it isn't missed due to lag
//                         Task.Factory.StartNew(() => {
//                             while (Client.Instance.IsTcpConnected && _innerInstance != null) {
//                                 Thread.Sleep(1000);
//                                 Client.Instance.TrySendHeartbeat();
//                             }
//                         });
//                         Client.Instance.TrySendChangeName(name);
//                     }
//                     return res;
//                 });
//                 return true;
//             }
//
//             public void DisconnectFromServer() {
//                 if (!Client.Instance.IsTcpConnected)
//                     return;
//
//                 Client.Instance.Stop();
//                 // Client.Instance.Stop();
//             }
//
//             public bool CreateLobbyAsync(string name, Action<CreateLobbyResponse> callback) {
//                 if (Status != ConnectionStatus.Connected)
//                     return false;
//                 if (_createLobbyCallback != null)
//                     return false;
//
//                 _createLobbyCallback = callback;
//
//                 return Client.Instance.TrySendCreateLobby(name);
//             }
//
//             public bool DeleteLobbyAsync(string name, Action<DeleteLobbyResponse> callback) {
//                 if (Status != ConnectionStatus.Connected)
//                     return false;
//                 if (_createLobbyCallback != null)
//                     return false;
//
//                 return Client.Instance.TrySendCreateLobby(name);
//             }
//
//             public bool UpdateServerInfoAsync(Action<ServerInfoResponse> callback) {
//                 if (Status != ConnectionStatus.Connected)
//                     return false;
//                 if (_serverInfoCallback != null)
//                     return false;
//
//                 _serverInfoCallback = callback;
//                 
//                 return Client.Instance.TrySendGetServerData();
//             }
//
//             public void Update() {
//                 if (Client.Instance.IsTcpConnected) {
//
//                     // Server Connection
//                     if (_connectToServerTask != null) {
//                         if (_connectToServerTask.IsCompleted) {
//                             Status = _connectToServerTask.Result ? ConnectionStatus.Connected : ConnectionStatus.NotConnected;
//                             _connectToServerTask = null;
//                         } else {
//                             Status = ConnectionStatus.Idk;
//                         }
//                     }
//
//                     // Create Lobby Callback
//                     var createLobby = CreateLobby;
//                     if (createLobby != null) {
//                         if (_createLobbyCallback != null) {
//                             _createLobbyCallback(createLobby);
//                             _createLobbyCallback = null;
//                         }
//                     }
//
//                     // Server Info Callback
//                     var serverInfo = ServerInfo;
//                     if (serverInfo != null) {
//                         if (_serverInfoCallback != null) {
//                             _serverInfoCallback(serverInfo);
//                             LastServerInfo = serverInfo;
//                             _serverInfoCallback = null;
//                         }
//                     }
//
//                 } else {
//                     if (Status == ConnectionStatus.Connected) {
//                         Status = ConnectionStatus.NotConnected; // Maybe?
//                     } else if (_connectToServerTask != null) {
//                         Status = ConnectionStatus.Idk;
//                     }
//                 }
//             }
//
//             ~Inner() {
//                 SimulationRunner.OnUpdate -= Update;
//                 Client.Instance.Stop();
//             }
//
//             private ReaderWriterLockSlim _createLobbyLock = new ReaderWriterLockSlim();
//             private CreateLobbyResponse? _createLobby;
//             private CreateLobbyResponse? CreateLobby {
//                 get {
//                     if (_createLobby == null)
//                         return null;
//                     CreateLobbyResponse? tmp;
//                     _createLobbyLock.EnterWriteLock();
//                     tmp = _createLobby;
//                     _createLobby = null;
//                     _createLobbyLock.ExitWriteLock();
//                     return tmp;
//                 }
//             }
//             public void HandleCreateLobbyResponse(CreateLobbyResponse createLobbyResponse) {
//                 _createLobbyLock.EnterWriteLock();
//                 _createLobby = createLobbyResponse;
//                 _createLobbyLock.ExitWriteLock();
//             }
//
//             public void HandleDeleteLobbyResponse(DeleteLobbyResponse deleteLobbyResponse) {
//                 throw new NotImplementedException();
//             }
//
//             public void HandleJoinLobbyResponse(JoinLobbyResponse joinLobbyResponse) {
//                 throw new NotImplementedException();
//             }
//
//             public void HandleLeaveLobbyResponse(LeaveLobbyResponse leaveLobbyResponse) {
//                 throw new NotImplementedException();
//             }
//
//             private ReaderWriterLockSlim _serverInfoLock = new ReaderWriterLockSlim();
//             private ServerInfoResponse? _serverInfo;
//             private ServerInfoResponse? ServerInfo {
//                 get {
//                     if (_serverInfo == null)
//                         return null;
//                     ServerInfoResponse? tmp;
//                     _serverInfoLock.EnterWriteLock();
//                     tmp = _serverInfo;
//                     _serverInfo = null;
//                     _serverInfoLock.ExitWriteLock();
//                     return tmp;
//                 }
//             }
//             public void HandleServerInfoResponse(ServerInfoResponse serverInfoResponse) {
//                 _serverInfoLock.EnterWriteLock();
//                 _serverInfo = serverInfoResponse;
//                 _serverInfoLock.ExitWriteLock();
//             }
//
//             public void HandleStartLobbyResponse(StartLobbyResponse startLobbyResponse) {
//                 throw new NotImplementedException();
//             }
//
//             public void HandleStatusMessage(StatusMessage statusMessage) {
//                 throw new NotImplementedException();
//             }
//
//             public void HandleSwapResponse(SwapResponse swapResponse) {
//                 throw new NotImplementedException();
//             }
//
//             public void HandleChangeNameResponse(ChangeNameResponse changeNameResponse)
//             {
//                 throw new NotImplementedException();
//             }
//
//             public void HandleConnectionDataClient(ConnectionDataClient connectionDataClient)
//             {
//                 throw new NotImplementedException();
//             }
//
//             public void HandleConnectionDataHost(ConnectionDataHost connectionDataHost)
//             {
//                 throw new NotImplementedException();
//             }
//         }
//
//         private static Inner? _innerInstance = null;
//         private static Inner Instance {
//             get {
//                 if (_innerInstance == null) {
//                     _innerInstance = new Inner();
//                 }
//                 return _innerInstance;
//             }
//         }
//     }
//
//     public enum ConnectionStatus {
//         Idk = 0, Connected = 1, NotConnected = 2
//     }
// }
//
// /// <summary>
// /// Choose when to send out an event for when a value change is detected
// /// </summary>
// /// <typeparam name="T"></typeparam>
// public class DelayedUpdate<T> where T : class {
//     private bool _changed;
//     private T? _value = null!;
//     public readonly object _writeLock = new object();
//     public T? Value {
//         get => _value;
//         set {
//             lock (_writeLock) {
//                 _value = value;
//                 _changed = true;
//             }
//         }
//     }
//
//     public (bool res, T? val) PollChange() {
//         if (_changed)
//             return (true, Value);
//         
//         return (false, Value);
//     }
// }
