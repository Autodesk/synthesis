using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NUnit.Framework;
using SynthesisAPI.AssetManager;
using SynthesisAPI.Utilities;
using System.Collections.Generic;
using System.Threading.Tasks;
using ProtoBuf;
using Google.Protobuf;
using Mirabuf.Signal;
using Mirabuf;
using Google.Protobuf.WellKnownTypes;
using SynthesisAPI.Simulation;

namespace TestApi 
{
    [TestFixture]
    public static class TestServers
    {

        private static ConnectionMessage connectionRequest;
        private static ConnectionMessage resourceOwnershipRequest;
        private static ConnectionMessage terminateConnectionRequest;
        private static ConnectionMessage heartbeat;

        private static ConnectionMessage secondResourceOwnershipRequest;
        private static ConnectionMessage secondTerminateConnectionRequest;

        private static ConnectionMessage response;
        private static ConnectionMessage secondResponse;
        private static ByteString guid;
        private static int generation;
        private static ByteString secondGuid;
        private static int secondGeneration;

        private static bool isRunning = true;

        private static TcpClient client;
        private static int tcpPort = 13000;
        private static int udpListenPort = 13001;
        private static int udpSendPort = 13000;
        private static NetworkStream firstStream;
        private static NetworkStream secondStream;


        private static IPEndPoint remoteIpEndPoint;
        private static UdpClient udpClient;
        private static Socket updateSendSocket;
        private static IPEndPoint updateEndpoint;
        private static UpdateSignals update;

        private static Thread heartbeatThread;
        /*
        [Test]
        public static void TestUpdating()
        {
            connectionRequest = new ConnectionMessage()
            {
                ConnectionRequest = new ConnectionMessage.Types.ConnectionRequest()
            };
            resourceOwnershipRequest = new ConnectionMessage()
            {
                ResourceOwnershipRequest = new ConnectionMessage.Types.ResourceOwnershipRequest()
                {
                    ResourceName = "Robot"
                }
            };
            heartbeat = new ConnectionMessage()
            {
                Heartbeat = new ConnectionMessage.Types.Heartbeat()
            };

            heartbeatThread = new Thread(() =>
            {
                while (isRunning)
                {
                    Thread.Sleep(100);
                    SendData(heartbeat, firstStream);
                }
            });
            Thread udpReceiveThread = new Thread(() =>
            {
                
                udpClient.JoinMulticastGroup(IPAddress.Parse("224.100.0.1"));
                System.Diagnostics.Debug.WriteLine("Start Udp stuff...");
                while (isRunning)
                {
                    try
                    {
                        var data = UpdateSignals.Parser.ParseDelimitedFrom(new MemoryStream(udpClient.Receive(ref remoteIpEndPoint)));
                        //System.Diagnostics.Debug.WriteLine(data);
                    }
                    catch (SocketException e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                    }
                }
            });
            Thread udpSendThread = new Thread(() =>
            {
                var ms = new MemoryStream();
                update.WriteDelimitedTo(ms);
                System.Diagnostics.Debug.WriteLine("Sending update test");
                System.Diagnostics.Debug.WriteLine(update);
                updateSendSocket.SendTo(ms.ToArray(), updateEndpoint);
            });

            var signals = new Signals()
            {
                Info = new Info()
                {
                    Name = "Robot",
                    GUID = Guid.NewGuid().ToString()
                }
            };
            signals.SignalMap.Add("DigitalOutput", new Signal()
            {
                Info = new Info()
                {
                    Name = "signal",
                    GUID = Guid.NewGuid().ToString()
                },
                DeviceType = "Digital",
                Io = IOType.Output
            });


            SimulationManager.RegisterSimObject(new SimObject(signals.Info.Name, new ControllableState() { CurrentSignalLayout = signals }));

            isRunning = true;
            heartbeatThread.Start();
            TcpServerManager.Start();
            StartClient("127.0.0.1", ref firstStream);
            remoteIpEndPoint = new IPEndPoint(IPAddress.Any, udpListenPort);
            udpClient = new UdpClient(udpListenPort);
            updateSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            updateEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), udpSendPort);

            

            System.Diagnostics.Debug.WriteLine("Sending Connection Request");
            SendData(connectionRequest, firstStream);

            response = ReadData(firstStream);
            if (response.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.ConnectionResonse && response.ConnectionResonse.Confirm)
            {
                System.Diagnostics.Debug.WriteLine("Sending Resource Ownership Request");
                SendData(resourceOwnershipRequest, firstStream);
            }

            response = ReadData(firstStream);
            System.Diagnostics.Debug.WriteLine(response);
            if (response.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.ResourceOwnershipResponse && response.ResourceOwnershipResponse.Confirm)
            {
                guid = response.ResourceOwnershipResponse.Guid;
                generation = response.ResourceOwnershipResponse.Generation;
            }
            System.Diagnostics.Debug.WriteLine("Guid is: {0}", guid);


            update = new UpdateSignals()
            {
                Generation = generation,
                Guid = guid,
                ResourceName = "Robot"
            };
            update.SignalMap.Add("DigitalOutput", new UpdateSignal()
            {
                DeviceType = "Digital",
                Io = UpdateIOType.Output,
                Value = Value.ForNumber(4.2)
            });

            UdpServerManager.SimulationObjectsTarget = SimulationManager._simulationObject;
            System.Diagnostics.Debug.WriteLine(UdpServerManager.SimulationObjectsTarget);
            UdpServerManager.Start();
            udpReceiveThread.Start();
            udpSendThread.Start();

            Thread.Sleep(2000);

            terminateConnectionRequest = new ConnectionMessage()
            {
                TerminateConnectionRequest = new ConnectionMessage.Types.TerminateConnectionRequest()
                {
                    ResourceName = "Robot",
                    Guid = guid,
                    Generation = generation
                }
            };
            System.Diagnostics.Debug.WriteLine("Sending Terminate Connection Request");
            SendData(terminateConnectionRequest, firstStream);

            response = ReadData(firstStream);
            if (response.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.TerminateConnectionResponse && response.TerminateConnectionResponse.Confirm)
            {
                System.Diagnostics.Debug.WriteLine("Termination Successful");
                StopClient(firstStream);
                TcpServerManager.Stop();
            }
            isRunning = false;
            udpClient.Close();
            System.Diagnostics.Debug.WriteLine("End Udp stuff");
            udpReceiveThread.Join();
            udpSendThread.Join();
            UdpServerManager.Stop();
            System.Diagnostics.Debug.WriteLine(SimulationManager.SimulationObjects["Robot"].State.CurrentSignals["DigitalOutput"]);

            heartbeatThread.Join();
            System.Diagnostics.Debug.WriteLine("Test finished");
            Assert.IsTrue(true);
        }
        */

        [Test]
        public static void TestConnecting()
        {
            connectionRequest = new ConnectionMessage()
            {
                ConnectionRequest = new ConnectionMessage.Types.ConnectionRequest()
            };
            resourceOwnershipRequest = new ConnectionMessage()
            {
                ResourceOwnershipRequest = new ConnectionMessage.Types.ResourceOwnershipRequest()
                {
                    ResourceName = "Robot"
                }
            };
            heartbeat = new ConnectionMessage()
            {
                Heartbeat = new ConnectionMessage.Types.Heartbeat()
            };

            heartbeatThread = new Thread(() =>
            {
                Thread.Sleep(100);
                SendData(heartbeat, firstStream);
            });

            SimulationManager.RegisterSimObject(new SimObject("Robot", new ControllableState() { CurrentSignalLayout = new Signals()
            {
                Info = new Info()
                {
                    Name = "Robot",
                    GUID = Guid.NewGuid().ToString()
                }
            }}));

            heartbeatThread.Start();
            TcpServerManager.Start();
            StartClient("127.0.0.1", ref firstStream);

            System.Diagnostics.Debug.WriteLine("Sending Connection Request");
            SendData(connectionRequest, firstStream);

            response = ReadData(firstStream);
            if (response.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.ConnectionResonse && response.ConnectionResonse.Confirm)
            {
                System.Diagnostics.Debug.WriteLine("Sending Resource Ownership Request");
                SendData(resourceOwnershipRequest, firstStream);
            }

            response = ReadData(firstStream);
            System.Diagnostics.Debug.WriteLine(response);
            if (response.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.ResourceOwnershipResponse && response.ResourceOwnershipResponse.Confirm)
            {
                guid = response.ResourceOwnershipResponse.Guid;
                generation = response.ResourceOwnershipResponse.Generation;
            }
            System.Diagnostics.Debug.WriteLine("Guid is: {0}", guid);
            Thread.Sleep(1000);

            


            terminateConnectionRequest = new ConnectionMessage()
            {
                TerminateConnectionRequest = new ConnectionMessage.Types.TerminateConnectionRequest()
                {
                    ResourceName = "Robot",
                    Guid = guid,
                    Generation = generation
                }
            };
            System.Diagnostics.Debug.WriteLine("Sending Terminate Connection Request");
            SendData(terminateConnectionRequest, firstStream);
            if (response.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.TerminateConnectionResponse && response.TerminateConnectionResponse.Confirm)
            {
                StopClient(firstStream);
                Assert.IsTrue(true);
            }
        }

        [Test]
        public static void TestMultipleConnections()
        {
            SimulationManager.RegisterSimObject(new SimObject("Robot1", new ControllableState()
            {
                CurrentSignalLayout = new Signals()
                {
                    Info = new Info()
                    {
                        Name = "Robot1",
                        GUID = Guid.NewGuid().ToString()
                    }
                }
            }));
            SimulationManager.RegisterSimObject(new SimObject("Robot2", new ControllableState()
            {
                CurrentSignalLayout = new Signals()
                {
                    Info = new Info()
                    {
                        Name = "Robot2",
                        GUID = Guid.NewGuid().ToString()
                    }
                }
            }));

            connectionRequest = new ConnectionMessage()
            {
                ConnectionRequest = new ConnectionMessage.Types.ConnectionRequest()
            };
            resourceOwnershipRequest = new ConnectionMessage()
            {
                ResourceOwnershipRequest = new ConnectionMessage.Types.ResourceOwnershipRequest()
                {
                    ResourceName = "Robot1"
                }
            };
            secondResourceOwnershipRequest = new ConnectionMessage()
            {
                ResourceOwnershipRequest = new ConnectionMessage.Types.ResourceOwnershipRequest()
                {
                    ResourceName = "Robot2"
                }
            };
            heartbeat = new ConnectionMessage()
            {
                Heartbeat = new ConnectionMessage.Types.Heartbeat()
            };
            heartbeatThread = new Thread(() =>
            {
                Thread.Sleep(100);
                SendData(heartbeat, firstStream);
                SendData(heartbeat, secondStream);
            });

            heartbeatThread.Start();
            TcpServerManager.Start();
            StartClient("127.0.0.1", ref firstStream);
            StartClient("127.0.0.1", ref secondStream);

            System.Diagnostics.Debug.WriteLine("Sending Connection Requests");
            SendData(connectionRequest, firstStream);
            SendData(connectionRequest, secondStream);

            response = ReadData(firstStream);
            secondResponse = ReadData(secondStream);

            if (response.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.ConnectionResonse && response.ConnectionResonse.Confirm)
            {
                System.Diagnostics.Debug.WriteLine("Sending Resource Ownership Request1");
                SendData(resourceOwnershipRequest, firstStream);
            }
            if (secondResponse.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.ConnectionResonse && secondResponse.ConnectionResonse.Confirm)
            {
                System.Diagnostics.Debug.WriteLine("Sending Resource Ownership Request2");
                SendData(resourceOwnershipRequest, secondStream);
            }

            response = ReadData(firstStream);
            secondResponse = ReadData(secondStream);

            if (response.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.ResourceOwnershipResponse && response.ResourceOwnershipResponse.Confirm)
            {
                guid = response.ResourceOwnershipResponse.Guid;
                generation = response.ResourceOwnershipResponse.Generation;
            }
            if (secondResponse.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.ResourceOwnershipResponse && secondResponse.ResourceOwnershipResponse.Confirm)
            {
                secondGuid = secondResponse.ResourceOwnershipResponse.Guid;
                secondGeneration = secondResponse.ResourceOwnershipResponse.Generation;
            }

            System.Diagnostics.Debug.WriteLine("Guid1", guid);
            System.Diagnostics.Debug.WriteLine("Guid1", secondGuid);
            Thread.Sleep(1000);

            terminateConnectionRequest = new ConnectionMessage()
            {
                TerminateConnectionRequest = new ConnectionMessage.Types.TerminateConnectionRequest()
                {
                    ResourceName = "Robot1",
                    Guid = guid,
                    Generation = generation
                }
            };
            secondTerminateConnectionRequest = new ConnectionMessage()
            {
                TerminateConnectionRequest = new ConnectionMessage.Types.TerminateConnectionRequest()
                {
                    ResourceName = "Robot2",
                    Guid = secondGuid,
                    Generation = secondGeneration
                }
            };
            System.Diagnostics.Debug.WriteLine("Sending Terminate Connection Requests");
            SendData(terminateConnectionRequest, firstStream);
            SendData(secondTerminateConnectionRequest, secondStream);
            if (response.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.TerminateConnectionResponse && response.TerminateConnectionResponse.Confirm)
            {
                StopClient(firstStream);
            }
            if (secondResponse.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.TerminateConnectionResponse && secondResponse.TerminateConnectionResponse.Confirm)
            {
                StopClient(secondStream);
            }
            Assert.IsTrue(true);
        }

        public static void StartClient(string server, ref NetworkStream clientStream)
        {
            client = new TcpClient(server, tcpPort);
            clientStream = client.GetStream();
        }

        public static void StopClient(NetworkStream clientStream)
        {
            //may need error handling
            clientStream.Close();
            client.Close();
        }

        public static void SendData(ConnectionMessage message, NetworkStream stream)
        {
            try
            {
                message.WriteDelimitedTo(stream);
            }
            catch (ObjectDisposedException e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
            
        }

        public static ConnectionMessage ReadData(NetworkStream clientStream)
        {
            return ConnectionMessage.Parser.ParseDelimitedFrom(clientStream);
        }
    }
    
}
