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
                    Port = 13001;
                    IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, Port);
                    UdpClient listener = new UdpClient(remoteIpEndPoint);
                    
                    try
                    {
                        while (_isRunning)
                        {
                            updateSignalTasks.Add(DeserializeUpdateSignalAsync(listener.ReceiveAsync()));
                        }
                    }
                    catch (SocketException e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                    }
                    finally
                    {
                        listener.Close();
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
            }

            public static int Port { get; private set; }

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
                            listenerThread.Join();
                        }
                    }
                    if (value)
                    {
                        listenerThread.Start();
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

        public static bool IsRunning
        {
            get => Server.Instance.IsRunning;
            set => Server.Instance.IsRunning = value;
        }

        public static void SetTargetQueue(ConcurrentQueue<UpdateSignals> target)
        {
            Server.Instance.packets = target;
        } 
    }
}
