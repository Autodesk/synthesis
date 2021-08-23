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


namespace TestApi 
{
    [TestFixture]
    public static class TestServers
    {
        static ConnectionMessage connectionRequest;
        static ConnectionMessage resourceOwnershipRequest;
        static ConnectionMessage terminateConnectionRequest;
        static ConnectionMessage heartbeat;

        static ConnectionMessage response;
        static ByteString guid;
        static int generation;

        static TcpClient client;
        static int port = 13000;
        static NetworkStream clientStream;

        static Thread heartbeatThread;

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
                SendData(heartbeat);
            });

            RobotManager.Instance.AddSignalLayout(new Signals()
            {
                Info = new Info()
                {
                    Name = "Robot",
                    GUID = Guid.NewGuid().ToString()
                }
            });

            heartbeatThread.Start();
            TcpServerManager.Start();
            StartClient("127.0.0.1");

            System.Diagnostics.Debug.WriteLine("Sending Connection Request");
            SendData(connectionRequest);

            response = ReadData();
            if (response.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.ConnectionResonse && response.ConnectionResonse.Confirm)
            {
                System.Diagnostics.Debug.WriteLine("Sending Resource Ownership Request");
                SendData(resourceOwnershipRequest);
            }

            response = ReadData();
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
                    Guid = guid,
                    Generation = generation
                }
            };
            System.Diagnostics.Debug.WriteLine("Sending Terminate Connection Request");
            SendData(terminateConnectionRequest);
            if (response.MessageTypeCase == ConnectionMessage.MessageTypeOneofCase.TerminateConnectionResponse && response.TerminateConnectionResponse.Confirm)
            {
                StopClient();
            }

        }

        public static void StartClient(string server)
        {
            client = new TcpClient(server, port);
            clientStream = client.GetStream();
        }

        public static void StopClient()
        {
            //may need error handling
            clientStream.Close();
            client.Close();
        }

        public static void SendData(ConnectionMessage message)
        {
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
                

            clientStream.Write(metadata, 0, metadata.Length);
            clientStream.Write(content, 0, content.Length);
        }

        public static ConnectionMessage ReadData()
        {
            ConnectionMessage tmp = new ConnectionMessage();

            byte[] sizeBuffer = new byte[sizeof(Int32)];
            clientStream.Read(sizeBuffer, 0, sizeof(Int32));
            byte[] messageBuffer = new byte[BitConverter.ToInt32(sizeBuffer, 0)];
            clientStream.Read(messageBuffer, 0, messageBuffer.Length);
            tmp.MergeFrom(messageBuffer);

            return tmp;
        }
    }
    
}
