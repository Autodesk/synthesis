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
            private List<ClientHandler> clients;
            //public ConcurrentQueue<UpdateSignals> _packets;
            public TcpListener listener;
            public Thread listenerThread;
            public Thread clientManagerThread;
            public Thread writerThread;
            private bool _isRunning;
            public bool _canAcceptClients;
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
                public Task<ConnectionMessage?> message;
                public List<Task> currentWrites;
                public TcpClient client;
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
                                message = GetConnectionMessageAsync(ns),
                                currentWrites = new List<Task>(),
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

                clientManagerThread = new Thread(() =>
                {
                    while (_isRunning || clients.Any())
                    {
                        for (int i = clients.Count - 1; i >= 0; i--)
                        {
                            if (DateTimeOffset.Now.ToUnixTimeMilliseconds() - clients[i].state.LastHeartbeat > 5000)
                            {
                                clients[i].stream.Close();
                                clients[i].client.Close();
                                clients.RemoveAt(i);
                            }
                            else if (clients[i].message.IsCompleted)
                            {
                                switch (clients[i].message.Result.MessageTypeCase)
                                {
                                    case ConnectionMessage.MessageTypeOneofCase.ConnectionRequest:
                                        System.Diagnostics.Debug.WriteLine("Received Connection Request");
                                        clients[i].currentWrites.Add(SendMessageAsync(clients[i].stream, new ConnectionMessage 
                                        { 
                                            ConnectionResonse = new ConnectionMessage.Types.ConnectionResponse() 
                                            { 
                                                Confirm = _canAcceptClients 
                                            } 
                                        }));
                                        break;
                                    case ConnectionMessage.MessageTypeOneofCase.ResourceOwnershipRequest:
                                        //Maybe make error stuff better
                                        System.Diagnostics.Debug.WriteLine("Received Resource Ownership Request");
                                        int generation = default;
                                        ByteString guid = ByteString.Empty;
                                        if (TryGetResource(clients[i].message.Result.ResourceOwnershipRequest.ResourceName, ref guid, ref generation))
                                        {
                                            clients[i].currentWrites.Add(SendMessageAsync(clients[i].stream, new ConnectionMessage
                                            {
                                                ResourceOwnershipResponse = new ConnectionMessage.Types.ResourceOwnershipResponse()
                                                {
                                                    Confirm = true,
                                                    Guid = guid,
                                                    Generation = generation
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            clients[i].currentWrites.Add(SendMessageAsync(clients[i].stream, new ConnectionMessage
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
                                        if (RobotManager.Instance.Robots.TryGetValue(clients[i].message.Result.TerminateConnectionRequest.ResourceName, out ControllableState tmp) && clients[i].message.Result.TerminateConnectionRequest.Guid == tmp.Guid && clients[i].message.Result.TerminateConnectionRequest.Generation == tmp.Generation)
                                        {
                                            clients[i].currentWrites.Add(SendMessageAsync(clients[i].stream, new ConnectionMessage
                                            {
                                                TerminateConnectionResponse = new ConnectionMessage.Types.TerminateConnectionResponse()
                                                {
                                                    Confirm = true,
                                                    ResourceName = clients[i].message.Result.TerminateConnectionRequest.ResourceName
                                                }
                                            }));
                                        }
                                        else
                                        {
                                            clients[i].currentWrites.Add(SendMessageAsync(clients[i].stream, new ConnectionMessage
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
                                clients[i].message = GetConnectionMessageAsync(clients[i].stream);
                            }
                        }
                    }
                });


                listenerThread.Start();
                clientManagerThread.Start();

            }




            private async Task SendMessageAsync(NetworkStream stream, ConnectionMessage message)
            {
                System.Diagnostics.Debug.WriteLine("Writing: ", message.ToString());
                /*
                var ms = new MemoryStream();
                message.WriteTo(ms);

                int size = message.CalculateSize();
                ms.Seek(0, SeekOrigin.Begin);
                byte[] content = new byte[size];
                ms.Read(content, 0, size);

                byte[] metadata = new byte[sizeof(int)];
                metadata = BitConverter.GetBytes(size);

                if (!BitConverter.IsLittleEndian)
                {
                    Array.Reverse(metadata);
                    Array.Reverse(content);
                }
                */
                byte[] metadata = new byte[sizeof(int)];
                metadata = BitConverter.GetBytes(message.CalculateSize());

                await stream.WriteAsync(metadata, 0, metadata.Length);
                message.WriteTo(stream);
                //await stream.WriteAsync(content, 0, content.Length);
            }

            private async Task<ConnectionMessage> GetConnectionMessageAsync(NetworkStream stream)
            {
                ConnectionMessage connectionMessage = new ConnectionMessage();
                byte[] sizeBuffer = new byte[sizeof(Int32)];
                await stream.ReadAsync(sizeBuffer, 0, sizeof(Int32));
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(sizeBuffer);

                byte[] messageBuffer = new byte[BitConverter.ToInt32(sizeBuffer, 0)];
                await stream.ReadAsync(messageBuffer, 0, messageBuffer.Length);
                if (!BitConverter.IsLittleEndian)
                    Array.Reverse(sizeBuffer);

                connectionMessage.MergeFrom(messageBuffer);
                return connectionMessage;
            }

            
            private bool TryGetResource(string resourceName, ref ByteString guidOutput, ref int generation)
            {
                System.Diagnostics.Debug.WriteLine("TRYING TO GET RESOURCE");
                System.Diagnostics.Debug.WriteLine(RobotManager.Instance.Robots.TryGetValue(resourceName, out ControllableState asd));
                System.Diagnostics.Debug.WriteLine(asd.Guid);
                System.Diagnostics.Debug.WriteLine(asd.IsFree);
                if (RobotManager.Instance.Robots.TryGetValue(resourceName, out ControllableState tmp) && tmp.IsFree)
                {
                    guidOutput = tmp.Guid;
                    generation = tmp.Generation;
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