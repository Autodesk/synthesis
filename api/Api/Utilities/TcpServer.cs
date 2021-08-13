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
                //_packets = new ConcurrentQueue<UpdateSignals>(); //Default queue if no queue is set
                listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 13000);
            }

            public void Start()
            {
                clients = new List<ClientHandler>();
                listener.Start();
                
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
                                        //dont need to lock im pretty sure
                                        clients[i].currentWrites.Add(SendMessageAsync(clients[i].stream, new ConnectionMessage { ConnectionResonse = new ConnectionMessage.Types.ConnectionResponse() { Confirm = true } }));
                                        break;
                                    case ConnectionMessage.MessageTypeOneofCase.ResourceOwnershipRequest:
                                        break;
                                    case ConnectionMessage.MessageTypeOneofCase.TerminateConnectionRequest:
                                        break;
                                    case ConnectionMessage.MessageTypeOneofCase.Heartbeat:
                                        break;
                                    default:
                                        System.Diagnostics.Debug.WriteLine("Invalid Message Recieved");
                                        break;
                                }
                            }
                        }
                    }
                });

                

                listenerThread.Start();
                clientManagerThread.Start();
                
            }



            
            private async Task SendMessageAsync(NetworkStream stream, ConnectionMessage message)
            {
                var ms = new MemoryStream();
                message.WriteTo(ms);

                int size = message.CalculateSize();
                ms.Seek(0, SeekOrigin.Begin);
                byte[] content = new byte[size];
                ms.Read(content, 0, size);

                byte[] metadata = new byte[sizeof(int)];
                metadata = BitConverter.GetBytes(size);

                await stream.WriteAsync(metadata, 0, metadata.Length);
                await stream.WriteAsync(content, 0, content.Length);
            }

            private async Task<ConnectionMessage> GetConnectionMessageAsync(NetworkStream stream)
            {
                ConnectionMessage connectionMessage = new ConnectionMessage();
                byte[] sizeBuffer = new byte[sizeof(Int32)];
                await stream.ReadAsync(sizeBuffer, 0, sizeof(Int32));
                byte[] messageBuffer = new byte[BitConverter.ToInt32(sizeBuffer, 0)];
                await stream.ReadAsync(messageBuffer, 0, messageBuffer.Length);
                connectionMessage.MergeFrom(messageBuffer);
                return connectionMessage;
            }

            /*
            private async Task<byte[]> GetNextMessageBufferAsync(NetworkStream stream)
            {
                byte[] bufferSize = new byte[sizeof(Int32)];
                await stream.ReadAsync(bufferSize, 0, sizeof(Int32));
                byte[] newBuffer = new byte[BitConverter.ToInt32(bufferSize, 0)];
                await stream.ReadAsync(newBuffer, 0, newBuffer.Length);
                return newBuffer;
            }
            */

            /*
            public async Task<UpdateSignals?> ReadUpdateMessageAsync(MemoryStream stream)
            {
                if (stream.Position >= stream.Length)
                {
                    return null;
                }
                UpdateSignals signals = new UpdateSignals();
                byte[] sizeBytes = new byte[sizeof(int)];
                await stream.ReadAsync(sizeBytes, 0, sizeBytes.Length);
                if (BitConverter.IsLittleEndian)
                    Array.Reverse(sizeBytes);

                int size = BitConverter.ToInt32(sizeBytes, 0);
                byte[] signalBytes = new byte[size];
                await stream.ReadAsync(signalBytes, 0, signalBytes.Length);
                signals.MergeFrom(signalBytes);
                return signals;
            }
            */
        }
        /*
        public static ConcurrentQueue<UpdateSignals> Packets
        {
            get
            {
                return Server.Instance._packets;
            }
        }
        */

        public static void Start()
        {
            if (Server.Instance.IsRunning) return;
            Server.Instance.IsRunning = true;
        }

        public static void Stop()
        {
            Server.Instance.IsRunning = false;
        }
        /*
        public static void SetTargetQueue(ConcurrentQueue<UpdateSignals> target)
        {
            Server.Instance._packets = target;
        }
        */
    }

}