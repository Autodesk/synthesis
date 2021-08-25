using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Google;
using Google.Protobuf;

namespace SynthesisAPI.Utilities
{
    public static class UdpServerManager
    {
        private sealed class Server
        {
            private static readonly Lazy<Server> lazy = new Lazy<Server>(() => new Server());
            public static Server Instance { get { return lazy.Value; } }
            private Server()
            {
                packets = new ConcurrentQueue<UpdateSignals>();
                _isRunning = false;

                

                listenerThread = new Thread(() =>
                {
                    try
                    {
                        while (_isRunning)
                        {
                            //updateSignalTasks.Add(DeserializeUpdateSignalAsync(client.ReceiveAsync()));
                        }
                    }
                    catch (SocketException e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                    }
                });
                managerThread = new Thread(() =>
                {
                    while (_isRunning || updateSignalTasks.Any())
                    {
                        for (int i = updateSignalTasks.Count - 1; i >= 0; i--)
                        {
                            if (updateSignalTasks[i].IsCompleted && updateSignalTasks[i].Result != null)
                            {
                                packets.Enqueue(updateSignalTasks[i].Result);
                                updateSignalTasks.RemoveAt(i);
                            }
                        }
                    }
                });
                outputThread = new Thread(() =>
                {
                    multicastAddress = IPAddress.Parse("224.100.0.1");
                    outputPort = 13001;
                    outputIpEndPoint = new IPEndPoint(multicastAddress, outputPort);
                    outputClient = new UdpClient(AddressFamily.InterNetwork);
                    outputClient.JoinMulticastGroup(multicastAddress);
                    MemoryStream outputStream = new MemoryStream();
                    while (_isRunning)
                    {
                        // maybe lock robots with ReaderWriterLockSlim
                        for (int i = 0; i < RobotManager.Instance.Robots.Count; i++)
                        {
                            // Resets outputStream without initializing a new MemoryStream every time
                            byte[] buffer = outputStream.GetBuffer();
                            Array.Clear(buffer, 0, buffer.Length);
                            outputStream.Position = 0;
                            outputStream.SetLength(0);

                            string[] robotKeys = RobotManager.Instance.Robots.Keys.ToArray<string>();
                            UpdateSignals update = new UpdateSignals()
                            {
                                ResourceName = robotKeys[i]
                            };
                            update.SignalMap.Add(RobotManager.Instance.Robots[robotKeys[i]].CurrentSignals);

                            update.WriteDelimitedTo(outputStream);

                            byte[] sendBuffer = outputStream.ToArray();

                            System.Diagnostics.Debug.WriteLine("Sending update");

                            //outputSocket.SendTo(sendBuffer, remoteIpEndPoint);
                            outputClient.Send(sendBuffer, sendBuffer.Length, outputIpEndPoint);
                            
                        }
                    }
                });
            }



            private UdpClient listenerClient;
            private IPEndPoint listenerIpEndPoint;
            private int listenerPort;

            private IPAddress multicastAddress;
            private UdpClient outputClient;
            private IPEndPoint outputIpEndPoint;
            private int outputPort;
            


            private bool _isRunning = false;
            public bool IsRunning
            {
                get => _isRunning;
                set
                {
                    _isRunning = value;
                    if (!value)
                    {
                        if (listenerThread != null && listenerThread.IsAlive) { listenerThread.Join(); }
                        if (managerThread != null && managerThread.IsAlive) { managerThread.Join(); }
                        if (outputThread != null && outputThread.IsAlive) { outputThread.Join(); }
                        //if (client != null && client.Client.Connected) { client.Close(); }
                        if (outputClient != null && outputClient.Client.Connected) { outputClient.Close(); }
                    }
                    if (value)
                    {
                        //listenerThread.Start();
                        outputThread.Start();
                    }
                }
            }

            public ConcurrentQueue<UpdateSignals> packets;
            private List<Task<UpdateSignals?>> updateSignalTasks;
            private Thread listenerThread;
            private Thread managerThread;
            private Thread outputThread;

            private async Task<UpdateSignals?> DeserializeUpdateSignalAsync(Task<UdpReceiveResult> receiveTask)
            {
                byte[] data = receiveTask.Result.Buffer;
                try
                {
                    UpdateSignals signals = new UpdateSignals();
                    MemoryStream stream = new MemoryStream(data);
                    byte[] sizeBytes = new byte[sizeof(int)];
                    await stream.ReadAsync(sizeBytes, 0, sizeBytes.Length);
                    

                    int size = BitConverter.ToInt32(sizeBytes, 0);
                    byte[] signalBytes = new byte[size];
                    await stream.ReadAsync(signalBytes, 0, signalBytes.Length);
                    signals.MergeFrom(signalBytes);
                    return signals;
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                    return null;
                }
            }
        }

        public static void Start() { Server.Instance.IsRunning = true; }

        public static void Stop() { Server.Instance.IsRunning = false; }

        public static void SetTargetQueue(ConcurrentQueue<UpdateSignals> target)
        {
            Server.Instance.packets = target;
        } 
    }
}
