using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Linq;
using Google.Protobuf;
using SynthesisAPI.Simulation;

namespace SynthesisAPI.Utilities
{

    public static class TcpServerManager
    {
        private sealed class Server
        {
            private Socket _server = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            private Dictionary<ClientState, long> _clients = new Dictionary<ClientState, long>();

            public int GlobalBufferSize { get; set; } = 1024;
            private byte[] _globalBuffer;

            private bool _isRunning;

            public int port = 13000;
            public bool _canAcceptClients;
            public bool IsRunning
            {
                get => _isRunning;
                set
                {
                    _isRunning = value;
                    if (!value && heartbeatThread.IsAlive) { heartbeatThread.Join(); }
                    if (value) { Start(); }
                }
            }

            private static readonly Lazy<Server> lazy = new Lazy<Server>(() => new Server());
            public static Server Instance { get { return lazy.Value; } }
            private Server()
            {
                _isRunning = false;
                _canAcceptClients = false;
            }

            private Thread heartbeatThread = new Thread(() => 
            { 
                while (Server.Instance._isRunning)
                {
                    List<KeyValuePair<ClientState, long>> clientList = Instance._clients.ToList<KeyValuePair<ClientState, long>>();
                    for (int i = clientList.Count - 1; i >= 0; i--)
                    {
                        if (System.DateTimeOffset.Now.ToUnixTimeMilliseconds() - clientList[i].Value >= 2000) 
                        {
                            Instance.RemoveClient(clientList[i].Key);
                        }
                    }
                    Thread.Sleep(1000);
                }
            });

            private void RemoveClient(ClientState client)
            {
                Logger.Log("Removing Client");
                if (_clients.ContainsKey(client))
                {
                    for (int i = client.resources.Count - 1; i >= 0; i--)
                    {
                        client.resources[i].State.IsFree = true;
                        client.resources.RemoveAt(i);
                    }
                    client.socket.Close();
                    _clients.Remove(client);
                }
            }

            struct ClientState
            {
                public Socket socket;
                public List<SimObject> resources;
            }
            

            private void AcceptCallback(IAsyncResult asyncResult)
            {
                Logger.Log("Client Connection Accepted");
                ClientState client = new ClientState
                {
                    socket = _server.EndAccept(asyncResult),
                    resources = new List<SimObject>()
                };
                _clients.Add(client, System.DateTimeOffset.Now.ToUnixTimeMilliseconds());
                if (_isRunning)
                {
                    client.socket.BeginReceive(_globalBuffer, 0, _globalBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), client);
                    _server.BeginAccept(new AsyncCallback(AcceptCallback), null);
                }
            }

            private void ReceiveCallback(IAsyncResult asyncResult)
            {
                ClientState client = (ClientState)asyncResult.AsyncState; //This is different from what goes in the array
                int received = client.socket.EndReceive(asyncResult);
                byte[] data = new byte[received];
                Array.Copy(_globalBuffer, data, received);
                ConnectionMessage message = ConnectionMessage.Parser.ParseFrom(data);

                _clients[client] = System.DateTimeOffset.Now.ToUnixTimeMilliseconds(); 

                switch (message.MessageTypeCase)
                {
                    case ConnectionMessage.MessageTypeOneofCase.ConnectionRequest:
                        SendConnectionMessage(client, new ConnectionMessage()
                        {
                            ConnectionResonse = new ConnectionMessage.Types.ConnectionResponse()
                            {
                                Confirm = _canAcceptClients
                            }
                        }, false);
                        break;
                    case ConnectionMessage.MessageTypeOneofCase.ResourceOwnershipRequest:
                        if (SimulationManager.SimulationObjects.TryGetValue(message.ResourceOwnershipRequest.ResourceName, out SimObject resource) && resource.State.IsFree)
                        {
                            SendConnectionMessage(client, new ConnectionMessage
                            {
                                ResourceOwnershipResponse = new ConnectionMessage.Types.ResourceOwnershipResponse()
                                {
                                    ResourceName = message.ResourceOwnershipRequest.ResourceName,
                                    Confirm = true,
                                    Guid = resource.State.Guid,
                                    Generation = resource.State.Generation
                                }
                            }, false);
                            client.resources.Add(resource);
                            resource.State.IsFree = false;
                        }
                        else
                        {
                            SendConnectionMessage(client, new ConnectionMessage
                            {
                                ResourceOwnershipResponse = new ConnectionMessage.Types.ResourceOwnershipResponse()
                                {
                                    ResourceName = message.ResourceOwnershipRequest.ResourceName,
                                    Confirm = false,
                                    Error = "Resource could not be given"
                                }
                            }, false);
                        }
                        break;
                    case ConnectionMessage.MessageTypeOneofCase.TerminateConnectionRequest:
                        SendConnectionMessage(client, new ConnectionMessage
                        {
                            TerminateConnectionResponse = new ConnectionMessage.Types.TerminateConnectionResponse()
                            {
                                Confirm = true
                            }
                        }, true);
                        break;
                    case ConnectionMessage.MessageTypeOneofCase.ReleaseResourceRequest:
                        bool hasRemoved = false;
                        for (int i = client.resources.Count - 1; i >= 0; i--)
                        {
                            if (client.resources[i].Name.Equals(message.ReleaseResourceRequest.ResourceName))
                            {
                                client.resources[i].State.IsFree = true;
                                client.resources.RemoveAt(i);
                                hasRemoved = true;
                                break;
                            }
                        }
                        if (hasRemoved)
                        {
                            SendConnectionMessage(client, new ConnectionMessage
                            {
                                ReleaseResourceResponse = new ConnectionMessage.Types.ReleaseResourceResponse()
                                {
                                    Confirm = true
                                }
                            }, false);
                        }
                        else
                        {
                            SendConnectionMessage(client, new ConnectionMessage
                            {
                                ReleaseResourceResponse = new ConnectionMessage.Types.ReleaseResourceResponse()
                                {
                                    Confirm = false,
                                    Error = "Cannot release resource"
                                }
                            }, false);
                        }
                        break;
                    case ConnectionMessage.MessageTypeOneofCase.Heartbeat:
                        break;
                    default:
                        Logger.Log("Invalid Message Received");
                        break;
                }
                if (_clients.ContainsKey(client))
                {
                    client.socket.BeginReceive(_globalBuffer, 0, _globalBuffer.Length, SocketFlags.None, new AsyncCallback(ReceiveCallback), client);
                }
            }

            // The bool value is whether or not the client should be removed after sending the message
            private void SendCallback(IAsyncResult asyncResult)
            {
                KeyValuePair<ClientState, bool> client = (KeyValuePair<ClientState, bool>)asyncResult.AsyncState;
                System.Diagnostics.Debug.WriteLine(client.Key.socket.EndSend(asyncResult));
                
                if (client.Value)
                {
                    client.Key.socket.Shutdown(SocketShutdown.Both);
                    RemoveClient(client.Key);
                }
            }

            private void SendConnectionMessage(ClientState cli, ConnectionMessage msg, bool shouldTerminate)
            {
                byte[] data = new byte[msg.CalculateSize()];
                msg.WriteTo(data);
                cli.socket.BeginSend(data, 0, data.Length, SocketFlags.None, new AsyncCallback(SendCallback), new KeyValuePair<ClientState, bool>(cli, shouldTerminate));
                
            }

            public void Start()
            {
                _canAcceptClients = true;
                _globalBuffer = new byte[GlobalBufferSize];
                _server.Bind(new IPEndPoint(IPAddress.Any, port));
                _server.Listen(5);
                _server.BeginAccept(new AsyncCallback(AcceptCallback), null);
                heartbeatThread.Start();
                Logger.Log("Starting TCP Server", LogLevel.Debug);

            }
        }

        public static void Start()
        {
            if (Server.Instance.IsRunning) { return; }
            Server.Instance.IsRunning = true;
        }
        
        public static void Stop()
        {
            Server.Instance.IsRunning = false;
        }

        public static int TcpPort
        {
            get => Server.Instance.port;
            set => Server.Instance.port = value;
        }

        public static bool CanAcceptClients
        {
            get => Server.Instance._canAcceptClients;
            set => Server.Instance._canAcceptClients = value;
        }

        public static bool IsRunning { get => Server.Instance.IsRunning; }
    }
}