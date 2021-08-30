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
using SynthesisAPI.Simulation;
using UnityEngine;

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
                updates = new ConcurrentQueue<UpdateSignals>();
                _isRunning = false;
                updateSignalTasks = new List<Task<UpdateSignals?>>();

                listenerThread = new Thread(() =>
                {
                    listenerPort = 13000;
                    listenerClient = new UdpClient(listenerPort);
                    listenerIpEndPoint = new IPEndPoint(IPAddress.Any, listenerPort);
                    try
                    {
                        while (_isRunning)
                        {
                            //may need to fix this
                            var data = listenerClient.Receive(ref listenerIpEndPoint);
                            
                            System.Diagnostics.Debug.WriteLine(UpdateSignals.Parser.ParseDelimitedFrom(new MemoryStream(data)));
                            Task.Run(() =>
                            {
                                updates.Enqueue(UpdateSignals.Parser.ParseDelimitedFrom(new MemoryStream(data)));
                            });
                        }
                    }
                    catch (SocketException e)
                    {
                        Logger.Log("UDP Listener Stopped Successfully", LogLevel.Debug);
                    }
                    catch (AggregateException)
                    {

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


                            outputClient.Send(sendBuffer, sendBuffer.Length, outputIpEndPoint);
                            
                        }
                    }
                });
                queueThread = new Thread(() =>
                {
                    
                    if (SimObjectsTarget == null)
                    {
                        Logger.Log("A target for Simulation Object updates has not been set", LogLevel.Debug);
                    }
                    else
                    {
                        while (_isRunning)
                        {
                            if (updates.TryDequeue(out UpdateSignals tmp))
                            {
                                SimObjectsTarget[tmp.ResourceName].State.Update(tmp);
                            }
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

            public Dictionary<string, SimObject>? SimObjectsTarget { get; set; }
            private ConcurrentQueue<UpdateSignals> updates;
            private List<Task<UpdateSignals?>> updateSignalTasks;
            private Thread listenerThread;
            private Thread outputThread;
            private Thread queueThread;

            private bool _isRunning = false;
            public bool IsRunning
            {
                get => _isRunning;
                set
                {
                    _isRunning = value;
                    if (!value)
                    {
                        if (outputClient != null && outputClient.Client.Connected) { outputClient.Close(); }
                        if (listenerClient != null) { listenerClient.Close(); }
                        if (listenerThread != null && listenerThread.IsAlive) { listenerThread.Join(); }
                        if (outputThread != null && outputThread.IsAlive) { outputThread.Join(); }
                        if (queueThread != null && queueThread.IsAlive) { queueThread.Join(); }
                    }
                    if (value)
                    {
                        Logger.Log("Starting UDP Server", LogLevel.Debug);
                        listenerThread.Start();
                        outputThread.Start();
                        queueThread.Start();
                    }
                }
            }
        }

        public static void Start() { Server.Instance.IsRunning = true; }

        public static void Stop() { Server.Instance.IsRunning = false; }

        public static Dictionary<string, SimObject> SimulationObjectsTarget 
        { 
            get => Server.Instance.SimObjectsTarget; 
            set => Server.Instance.SimObjectsTarget = value; 
        }
    }
    
}
