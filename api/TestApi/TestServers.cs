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
        private static Socket _client;

        [Test]
        public static void TestConnecting()
        {
            ConnectionMessage connectionRequest = new ConnectionMessage()
            {
                ConnectionRequest = new ConnectionMessage.Types.ConnectionRequest() { }
            };
            ConnectionMessage resourceOwnershipRequest = new ConnectionMessage()
            {
                ResourceOwnershipRequest = new ConnectionMessage.Types.ResourceOwnershipRequest()
                {
                    ResourceName = "Robot"
                }
            };
            ConnectionMessage heartbeat = new ConnectionMessage()
            {
                Heartbeat = new ConnectionMessage.Types.Heartbeat()
            };
            ConnectionMessage terminateConnectionRequest = new ConnectionMessage()
            {
                TerminateConnectionRequest = new ConnectionMessage.Types.TerminateConnectionRequest()
                {
                }
            };

            Thread heartbeatThread = new Thread(() =>
            {
                Thread.Sleep(100);
                //heartbeat.WriteDelimitedTo(tmpStream);
            });

            SimulationManager.RegisterSimObject(new SimObject("Robot", new ControllableState()
            {
                CurrentSignalLayout = new Signals()
                {
                    Info = new Info()
                    {
                        Name = "Robot",
                        GUID = Guid.NewGuid().ToString()
                    }
                }
            }));

            //heartbeatThread.Start();
            ByteString guid = ByteString.Empty;
            int generation = 0;
            _client = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            TcpServerManager.Start();
            int connectionAttempts = 0;
            while (!_client.Connected)
            {
                try
                {
                    connectionAttempts += 1;
                    _client.Connect(IPAddress.Loopback, 13000);
                }
                catch (SocketException e)
                {
                    System.Diagnostics.Debug.WriteLine(connectionAttempts);
                }
            }

            System.Diagnostics.Debug.WriteLine("CLIENT HAS CONNECTED");

            ConnectionMessage response;
            /*
            System.Diagnostics.Debug.WriteLine("Sending Connection Request");
            byte[] receiveBuffer = new byte[256];
            byte[] buffer = new byte[connectionRequest.CalculateSize()];
            connectionRequest.WriteTo(buffer);
            System.Diagnostics.Debug.WriteLine("BUFFER");
            _client.Send(buffer);
            int rec = _client.Receive(receiveBuffer);
            System.Diagnostics.Debug.WriteLine("Recieved data back");
            System.Diagnostics.Debug.WriteLine(rec);

            byte[] data = new byte[rec];
            Array.Copy(receiveBuffer, data, rec);
            response = ConnectionMessage.Parser.ParseFrom(data);
            */
            response = SendReceiveData(connectionRequest);
            if (response.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.ConnectionResonse && response.ConnectionResonse.Confirm)
            {
                System.Diagnostics.Debug.WriteLine("Sending Resource Ownership Request");
                //resourceOwnershipRequest.WriteDelimitedTo(tmpStream);
            }

            //response = ReadData(firstStream);
            //System.Diagnostics.Debug.WriteLine(response);
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
            //terminateConnectionRequest.WriteDelimitedTo(firstStream);
            if (response.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.TerminateConnectionResponse && response.TerminateConnectionResponse.Confirm)
            {
                //StopClient(firstStream);
                Assert.IsTrue(true);
            }
        }

        public static ConnectionMessage SendReceiveData(ConnectionMessage msg)
        {
            byte[] receiveBuffer = new byte[256];
            byte[] buffer = new byte[msg.CalculateSize()];
            msg.WriteTo(buffer);
            _client.Send(buffer);
            int rec = _client.Receive(receiveBuffer);
            byte[] data = new byte[rec];
            Array.Copy(receiveBuffer, data, rec);
            return ConnectionMessage.Parser.ParseFrom(data);
        }
        /*
        [Test]
        public static void TestProtoSize()
        {
            ConnectionMessage test = new ConnectionMessage()
            {
                ResourceOwnershipResponse = new ConnectionMessage.Types.ResourceOwnershipResponse
                {
                    Confirm = true,
                    Error = "none",
                    Generation = 0,
                    Guid = ByteString.CopyFromUtf8(Guid.NewGuid().ToString()),
                    ResourceName = "test"
                }
            };
            MemoryStream ms = new MemoryStream();
            test.WriteDelimitedTo(ms);
            System.Diagnostics.Debug.WriteLine(ms.Length);
        }

        [Test]
        public static void TestStreamBuffers()
        {
            ConnectionMessage test = new ConnectionMessage()
            {
                ResourceOwnershipResponse = new ConnectionMessage.Types.ResourceOwnershipResponse
                {
                    Confirm = true,
                    Error = "none",
                    Generation = 0,
                    Guid = ByteString.CopyFromUtf8(Guid.NewGuid().ToString()),
                    ResourceName = "test"
                }
            };
            MemoryStream ms = new MemoryStream();
            MemoryStream tmp = new MemoryStream();
            System.Diagnostics.Debug.Write("Initial buffer size: ");
            System.Diagnostics.Debug.WriteLine(ms.GetBuffer().Length);
            test.WriteDelimitedTo(tmp);
            tmp.ToArray().CopyTo(ms.GetBuffer(), 0);
            System.Diagnostics.Debug.WriteLine(ms.GetBuffer().Length);

        }

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
        */

    }
    
}
