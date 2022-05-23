using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using NUnit.Framework;
using SynthesisAPI.Utilities;
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
        private static Thread heartbeatThread = new Thread(() =>
            {
            while (_isRunning)
            {
                Thread.Sleep(500);
                SendData(heartbeat);
            }
        });
        private static ConnectionMessage heartbeat = new ConnectionMessage()
        {
            Heartbeat = new ConnectionMessage.Types.Heartbeat() { }
        };
        private static ConnectionMessage connectionRequest = new ConnectionMessage()
        {
            ConnectionRequest = new ConnectionMessage.Types.ConnectionRequest() { }
        };
        private static ConnectionMessage resourceOwnershipRequest = new ConnectionMessage()
        {
            ResourceOwnershipRequest = new ConnectionMessage.Types.ResourceOwnershipRequest()
            {
                ResourceName = "Robot"
            }
        };
        private static ConnectionMessage releaseResourceRequest = new ConnectionMessage()
        {
            ReleaseResourceRequest = new ConnectionMessage.Types.ReleaseResourceRequest()
            {
                ResourceName = "Robot"
            }
        };
        private static ConnectionMessage terminateConnectionRequest = new ConnectionMessage()
        {
            TerminateConnectionRequest = new ConnectionMessage.Types.TerminateConnectionRequest()
            {
            }
        };
        private static Socket _client;
        private static bool _isRunning;

        [Test]
        public static void TestUpdating()
        {
            IPEndPoint remoteIpEndPoint = new IPEndPoint(IPAddress.Any, 13001);
            UdpClient udpClient = new UdpClient(13001);
            Socket updateSendSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            IPEndPoint updateEndpoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 13000);

            Thread udpReceiveThread = new Thread(() =>
            {
                udpClient.JoinMulticastGroup(IPAddress.Parse("224.100.0.1"));
                System.Diagnostics.Debug.WriteLine("Start Udp stuff...");
                while (_isRunning)
                {
                    try
                    {
                        var data = UpdateSignals.Parser.ParseDelimitedFrom(new MemoryStream(udpClient.Receive(ref remoteIpEndPoint)));
                    }
                    catch (SocketException e)
                    {
                        System.Diagnostics.Debug.WriteLine("Udp Connection Terminated");
                    }
                }
            });
            
            RegisterRobot();
            string guid = string.Empty;
            int generation = 999;
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

            // established connection
            Assert.IsTrue(true);
            _isRunning = true;
            heartbeatThread.Start();

            ConnectionMessage response = SendReceiveData(connectionRequest);

            Assert.IsTrue(response.ConnectionResonse.Confirm);
            if (response.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.ConnectionResonse && response.ConnectionResonse.Confirm)
            {
                response = SendReceiveData(resourceOwnershipRequest);
            }

            Assert.IsTrue(response.ResourceOwnershipResponse.Confirm);
            if (response.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.ResourceOwnershipResponse && response.ResourceOwnershipResponse.Confirm)
            {
                guid = response.ResourceOwnershipResponse.Guid;
                generation = response.ResourceOwnershipResponse.Generation;
            }

            UpdateSignals update = new UpdateSignals()
            {
                Generation = generation,
                Guid = ByteString.CopyFromUtf8(guid),
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

            var ms = new MemoryStream();
            update.WriteDelimitedTo(ms);
            System.Diagnostics.Debug.WriteLine("Sending update test");
            System.Diagnostics.Debug.WriteLine(update);
            updateSendSocket.SendTo(ms.ToArray(), updateEndpoint);

            Thread.Sleep(1000); // need sleep to give it time to update

            Assert.IsTrue(!SimulationManager.SimulationObjects["Robot"].State.IsFree);

            System.Diagnostics.Debug.WriteLine(SimulationManager.SimulationObjects["Robot"].State.CurrentSignals["DigitalOutput"].Value);
            Assert.IsTrue(SimulationManager.SimulationObjects["Robot"].State.CurrentSignals["DigitalOutput"].Value.NumberValue == 4.2);

            _isRunning = false;
            heartbeatThread.Join();
            response = SendReceiveData(terminateConnectionRequest);
            System.Diagnostics.Debug.WriteLine("THIS HAPPENED");
            Assert.IsTrue(response.TerminateConnectionResponse.Confirm);
            
            udpClient.Close();
            UdpServerManager.Stop();
            udpReceiveThread.Join();
            System.Diagnostics.Debug.WriteLine("Almost Finished");
            TcpServerManager.Stop();
            System.Diagnostics.Debug.WriteLine("Finished");
            Assert.IsTrue(true);
        }

        [Test]
        public static void TestConnecting()
        {

            RegisterRobot();
            
            string guid = string.Empty;
            int generation;
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

            // established connection
            Assert.IsTrue(true);

            ConnectionMessage response;
            
            response = SendReceiveData(connectionRequest);
            Assert.IsTrue(response.ConnectionResonse.Confirm);
            if (response.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.ConnectionResonse && response.ConnectionResonse.Confirm)
            {
                response = SendReceiveData(resourceOwnershipRequest);
            }

            Assert.IsTrue(response.ResourceOwnershipResponse.Confirm);
            if (response.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.ResourceOwnershipResponse && response.ResourceOwnershipResponse.Confirm)
            {
                guid = response.ResourceOwnershipResponse.Guid;
                generation = response.ResourceOwnershipResponse.Generation;
            }

            response = SendReceiveData(releaseResourceRequest);
            Assert.IsTrue(response.ReleaseResourceResponse.Confirm);

            response = SendReceiveData(terminateConnectionRequest);
            if (response.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.TerminateConnectionResponse && response.TerminateConnectionResponse.Confirm)
            {
                System.Diagnostics.Debug.WriteLine("RECEIVED TERMINATE CONNECTION RESPONSE");
                Assert.IsTrue(true);
            }
        }

        // for sending message and recieving a response
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
        // for just sending a message
        public static void SendData(ConnectionMessage msg)
        {
            byte[] buffer = new byte[msg.CalculateSize()];
            msg.WriteTo(buffer);
            _client.Send(buffer);
        }

        public static void RegisterRobot()
        {
            try
            {
                Signals signals = new Signals()
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
                SimulationManager.RegisterSimObject(new SimObject("Robot", new ControllableState()
                {
                    CurrentSignalLayout = signals,
                    IsFree = true
                }));
            } catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
            
        }
    }
}
