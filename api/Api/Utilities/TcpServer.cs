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
using Google.Protobuf.WellKnownTypes;
using ProtoBuf;



namespace SynthesisAPI.Utilities
{

    public static class TcpServerManager
    {
        private sealed class Server
        {
            public readonly object packetsLock = new object();
            public readonly object clientsLock = new object();

            private List<ClientHandler> clients;
            public Dictionary<string, UpdateMessage.Types.ModifiedFields> _packets;
            public TcpListener listener;
            public Thread listenerThread;
            public Thread clientManagerThread;
            private bool _isRunning = false;
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
                public Task<UpdateMessage?> inputData;
                public MemoryStream stream;
                public TcpClient client;
            }


            private static readonly Lazy<Server> lazy = new Lazy<Server>(() => new Server());
            public static Server Instance { get { return lazy.Value; } }
            private Server()
            {
                _packets = new Dictionary<string, UpdateMessage.Types.ModifiedFields>();
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 13000);
            }

            public void Start()
            {
                clients = new List<ClientHandler>();
                listener.Start();
                
                listenerThread = new Thread(() =>
                {
                    while (IsRunning)
                    {
                        try
                        {
                            TcpClient cli = (listener.AcceptTcpClient());
                            MemoryStream ms = new MemoryStream();
                            cli.GetStream().CopyTo(ms);
                            ms.Seek(0, SeekOrigin.Begin);

                            System.Diagnostics.Debug.WriteLine(ms.Capacity);
                            clients.Add(new ClientHandler
                            {
                                client = cli,
                                stream = ms,
                                inputData = ReadUpdateMessageAsync(ms)
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
                    while (IsRunning || clients.Any())
                    {
                        lock (clientsLock)
                            for (int i = clients.Count - 1; i >= 0; i--)
                            {
                                if (clients[i].inputData.IsCompleted)
                                {
                                    if (clients[i].inputData.Result == null)
                                    {
                                        clients[i].client.Close();
                                        clients.RemoveAt(i);
                                    }
                                    else
                                    {
                                        lock (packetsLock)
                                        {
                                            _packets[clients[i].inputData.Result.Id] = clients[i].inputData.Result.Fields;
                                        }
                                        clients[i].inputData = ReadUpdateMessageAsync(clients[i].stream);


                                    }
                                }

                            }

                    }
                })
                {
                    
                };

                listenerThread.Start();
                clientManagerThread.Start();
                
            }
            
            public async Task<UpdateMessage?> ReadUpdateMessageAsync(MemoryStream stream)
            {
                if (stream.Position >= stream.Length)
                {
                    return null;
                }
                UpdateMessage message = new UpdateMessage();
                byte[] sizeBytes = new byte[sizeof(int)];
                await stream.ReadAsync(sizeBytes, 0, sizeBytes.Length);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(sizeBytes);

                int size = BitConverter.ToInt32(sizeBytes, 0);
                byte[] messageBytes = new byte[size];
                await stream.ReadAsync(messageBytes, 0, messageBytes.Length);
                message.MergeFrom(messageBytes);
                return message;
            }
        }


        
        public static Dictionary<string, UpdateMessage.Types.ModifiedFields> Packets
        {
            get
            {
                lock (Server.Instance.packetsLock)
                {
                    return Server.Instance._packets;
                }
            }
        }

        public static void Start()
        {
            Server.Instance.IsRunning = true;
        }

        public static void Stop()
        {
            Server.Instance.IsRunning = false;
        }
        
        
    }

}