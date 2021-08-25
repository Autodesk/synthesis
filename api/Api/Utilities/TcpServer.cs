using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using Google.Protobuf;
using ProtoBuf;
using System.Collections.Concurrent;


namespace SynthesisAPI.Utilities
{

    public static class TcpServerManager
    {
        private sealed class Server
        {
            // May have to lock clients (read/write lock?)
            public ReaderWriterLockSlim clientsLock;
            private List<ClientHandler> clients;
            public TcpListener listener;
            public Thread listenerThread;
            public Thread clientManagerThread;
            public Thread writerThread;
            private bool _isRunning;
            public bool _canAcceptClients;
            private List<Task> _currentWrites;
            public bool IsRunning
            {
                get => _isRunning;
                set
                {
                    _isRunning = value;
                    if (!value)
                    {
                        if (listenerThread != null && listenerThread.IsAlive)
                        {
                            clientManagerThread.Join();
                            listener.Stop();
                            listenerThread.Join();
                            writerThread.Join();
                        }
                    }
                    if (value)
                    {
                        Start();
                    }
                }
            }


            private class ClientHandler
            {
                public ClientState state;
                public NetworkStream stream;
                public Task<ConnectionMessage?>? message;
                public TcpClient client;
                public List<ControllableState> currentResources;
            }

            // Send both data and type in tcp messages
            private class ClientState
            {
                public bool IsConnected { get; set; } = false;
                public string? ResourceName { get; set; }
                public long LastHeartbeat { get; set; }
            }


            private static readonly Lazy<Server> lazy = new Lazy<Server>(() => new Server());
            public static Server Instance { get { return lazy.Value; } }
            private Server()
            {
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 13000);
                _isRunning = false;
                _canAcceptClients = false;
                _currentWrites = new List<Task>();
                clientsLock = new ReaderWriterLockSlim();
            }

            public void Start()
            {
                clients = new List<ClientHandler>();
                listener.Start();
                _canAcceptClients = true;

                listenerThread = new Thread(() =>
                {
                    while (_isRunning)
                    {
                        try
                        {
                            TcpClient cli = (listener.AcceptTcpClient());
                            NetworkStream ns = cli.GetStream();

                            clients.Add(new ClientHandler
                            {
                                client = cli,
                                stream = ns,
                                message = ParseMessageAsync(ns),
                                currentResources = new List<ControllableState>(),
                                state = new ClientState()
                                {
                                    LastHeartbeat = DateTimeOffset.Now.ToUnixTimeMilliseconds()
                                }
                            });
                        }
                        catch (SocketException)
                        {
                            System.Diagnostics.Debug.WriteLine("Listener stopped succesfully.");
                        }
                    }
                });

                // NEED TO LOCK THREADS!!!! (maybe)
                clientManagerThread = new Thread(() =>
                {
                    while (_isRunning || clients.Any())
                    {
                        for (int i = clients.Count - 1; i >= 0; i--)
                        {
                            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - clients[i].state.LastHeartbeat > 7000 && clients[i].message != null && clients[i].message.IsCompleted)
                            {
                                if (!TryRemoveClient(clients[i]))
                                {
                                    System.Diagnostics.Debug.WriteLine("Could not remove client.");
                                }
                            }
                            else if (clients[i].message != null && clients[i].message.IsCompleted)
                            {
                                switch (clients[i].message.Result.MessageTypeCase)
                                {
                                    case ConnectionMessage.MessageTypeOneofCase.ConnectionRequest:
                                        System.Diagnostics.Debug.WriteLine("Received Connection Request");
                                        System.Diagnostics.Debug.WriteLine(clients[i].stream);
                                        _currentWrites.Add(SendMessageAsync(clients[i].stream, new ConnectionMessage 
                                        { 
                                            ConnectionResonse = new ConnectionMessage.Types.ConnectionResponse() 
                                            { 
                                                Confirm = _canAcceptClients 
                                            } 
                                        }));
                                        break;

                                    case ConnectionMessage.MessageTypeOneofCase.ResourceOwnershipRequest:
                                        System.Diagnostics.Debug.WriteLine("Received Resource Ownership Request");
                                        ControllableState? resource = null;
                                        if (TryGetResource(clients[i].message.Result.ResourceOwnershipRequest.ResourceName, ref resource) && resource.Owner == null)
                                        {
                                            _currentWrites.Add(SendMessageAsync(clients[i].stream, new ConnectionMessage
                                            {
                                                ResourceOwnershipResponse = new ConnectionMessage.Types.ResourceOwnershipResponse()
                                                {
                                                    Confirm = true,
                                                    Guid = resource.Guid,
                                                    Generation = resource.Generation
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            _currentWrites.Add(SendMessageAsync(clients[i].stream, new ConnectionMessage
                                            {
                                                ResourceOwnershipResponse = new ConnectionMessage.Types.ResourceOwnershipResponse()
                                                {
                                                    Confirm = false,
                                                    Error = "Resource could not be given"
                                                }
                                            }));
                                        }
                                        break;

                                    case ConnectionMessage.MessageTypeOneofCase.TerminateConnectionRequest:
                                        System.Diagnostics.Debug.WriteLine("Received Terminate Connection Request");
                                        if (RobotManager.Instance.Robots.TryGetValue(clients[i].message.Result.TerminateConnectionRequest.ResourceName, out ControllableState tmp) && clients[i].message.Result.TerminateConnectionRequest.Guid.Equals(tmp.Guid) && clients[i].message.Result.TerminateConnectionRequest.Generation.Equals(tmp.Generation))
                                        {
                                            _currentWrites.Add(SendMessageAsync(clients[i].stream, new ConnectionMessage
                                            {
                                                TerminateConnectionResponse = new ConnectionMessage.Types.TerminateConnectionResponse()
                                                {
                                                    Confirm = true,
                                                    ResourceName = clients[i].message.Result.TerminateConnectionRequest.ResourceName
                                                }
                                            }));
                                            
                                            for (int j = clients[i].currentResources.Count - 1; j >= 0; j--)
                                            {
                                                if (clients[i].currentResources[j].Guid.Equals(clients[i].message.Result.TerminateConnectionRequest.Guid))
                                                {
                                                    clients[i].currentResources[j].ReleaseResource();
                                                    clients[i].currentResources.RemoveAt(j);
                                                }
                                            }
                                            
                                        }
                                        else
                                        {
                                            _currentWrites.Add(SendMessageAsync(clients[i].stream, new ConnectionMessage
                                            {
                                                TerminateConnectionResponse = new ConnectionMessage.Types.TerminateConnectionResponse()
                                                {
                                                    Confirm = false,
                                                    Error = "Cannot terminate connection from resource"
                                                }
                                            }));
                                        }
                                        break;

                                    case ConnectionMessage.MessageTypeOneofCase.Heartbeat:
                                        System.Diagnostics.Debug.WriteLine("Received Heartbeat");
                                        clients[i].state.LastHeartbeat = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                                        break;

                                    default:
                                        System.Diagnostics.Debug.WriteLine("Invalid Message Recieved");
                                        break;
                                }
                                clients[i].message = null;
                            }
                            if (clients[i].message == null && clients[i].stream.DataAvailable)
                            {
                                clients[i].message = ParseMessageAsync(clients[i].stream);
                            }
                        }
                    }
                });

                writerThread = new Thread(() =>
                {
                    while (IsRunning || _currentWrites.Count > 0)
                    {
                        for (int i = _currentWrites.Count - 1; i >= 0; i--)
                        {
                            if (_currentWrites[i] != null && _currentWrites[i].IsCompleted)
                            {
                                _currentWrites.RemoveAt(i);
                            }
                        }
                    }
                });


                listenerThread.Start();
                clientManagerThread.Start();
                writerThread.Start();

            }

            private async Task<ConnectionMessage> ParseMessageAsync(NetworkStream stream)
            {
                return await Task.Run(() => { return ConnectionMessage.Parser.ParseDelimitedFrom(stream); });
            }
            private async Task SendMessageAsync(NetworkStream stream, ConnectionMessage message)
            {
                System.Diagnostics.Debug.WriteLine("Writing: ", message.ToString());
                await Task.Run(() => message.WriteDelimitedTo(stream));
            }

            private bool TryRemoveClient(ClientHandler clientHandler)
            {
                System.Diagnostics.Debug.WriteLine("Removing Client");
                for (int i = clientHandler.currentResources.Count - 1; i >= 0; i--)
                {
                    clientHandler.currentResources[i].ReleaseResource();
                    clientHandler.currentResources.RemoveAt(i);
                }
                clientHandler.stream.Close();
                clientHandler.client.Close();
                return clients.Remove(clientHandler);
            }

            private bool TryGetResource(string resourceName, ref ControllableState? resource)
            {
                System.Diagnostics.Debug.WriteLine("TRYING TO GET RESOURCE");
                System.Diagnostics.Debug.WriteLine(RobotManager.Instance.Robots.TryGetValue(resourceName, out ControllableState asd));
                System.Diagnostics.Debug.WriteLine(asd.Guid);
                
                if (RobotManager.Instance.Robots.TryGetValue(resourceName, out resource))
                {
                    return true;
                }
                return false;
                
            }

        }

        public static void Start()
        {
            if (Server.Instance.IsRunning) return;
            Server.Instance.IsRunning = true;
        }
        
        public static void Stop()
        {
            Server.Instance.IsRunning = false;
        }

        public static bool CanAcceptClients
        {
            get => Server.Instance._canAcceptClients;
            set => Server.Instance._canAcceptClients = value;
        }
    }
}