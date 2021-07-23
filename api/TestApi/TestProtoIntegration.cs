using System;
using NUnit.Framework;
using SynthesisAPI.AssetManager;
using SynthesisAPI.Utilities;
using System.Threading;
using System.Collections.Generic;
using Mirabuf.Signal;
using Mirabuf;
using System.Net.Sockets;
using System.IO;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace TestApi {
    
    
    [TestFixture]
    public static class TestProtoCompleteIntegration
    {
        /*
        [Test]
        public static void TestIngtegration()
        {
            Signals testSignalLayout = new Signals()
            {
                Info = new Info()
                {
                    GUID = "1",
                    Name = "testSignalLayout",
                    Version = 1
                }
            };
            testSignalLayout.SignalMap.Add("DO1", new Signal()
            {
                Class = "Digital",
                Info = new Info()
                {
                    GUID = "2",
                    Name = "testDigitalOutput",
                    Version = 1
                },
                Io = IOType.Output
            });
            testSignalLayout.SignalMap.Add("AI1", new Signal()
            {
                Class = "Analog",
                Info = new Info()
                {
                    GUID = "3",
                    Name = "testAnalogInput",
                    Version = 1
                },
                Io = IOType.Input
            });

            UpdateSignals updateMessage = new UpdateSignals()
            {
                Name = "testSignalLayout"
            };
            updateMessage.SignalMap.Add("DO1", new UpdateSignal()
            {
                Class = "Digital",
                Io = UpdateIOType.Output,
                Value = Value.ForNumber(4.2)
            });
            updateMessage.SignalMap.Add("AI1", new UpdateSignal()
            {
                Class = "Digital",
                Io = UpdateIOType.Output,
                Value = Value.ForNumber(3.9)
            });

            System.Diagnostics.Debug.WriteLine("Adding signal layout");
            RobotManager.Instance.AddSignalLayout(testSignalLayout);

            System.Diagnostics.Debug.WriteLine("Starting Server");
            TcpServerManager.SetTargetQueue(RobotManager.Instance.UpdateQueue);
            TcpServerManager.Start();

            System.Diagnostics.Debug.WriteLine("Sending data");
            SendData("127.0.0.1", updateMessage);

            Thread.Sleep(500);

            System.Diagnostics.Debug.WriteLine("Stopping Server");
            TcpServerManager.Stop();

            System.Diagnostics.Debug.WriteLine("Starting RobotManager");
            RobotManager.Instance.Start();

            Thread.Sleep(500);
            RobotManager.Instance.Stop();
            System.Diagnostics.Debug.WriteLine("Stopped RobotManager");

            Assert.IsTrue(RobotManager.Instance.Robots["testSignalLayout"].CurrentSignals["DO1"].Equals(new UpdateSignal()
            {
                Class = "Digital",
                Io = UpdateIOType.Output,
                Value = Value.ForNumber(4.2)
            }));
        }
        */

        [Test]
        public static void TestRobotManager()
        {
            Signals testSignalLayout = new Signals()
            {
                Info = new Info()
                {
                    GUID = "1",
                    Name = "testSignalLayout",
                    Version = 1
                }
            };
            testSignalLayout.SignalMap.Add("DO1", new Signal()
            {
                Class = "Digital",
                Info = new Info()
                {
                    GUID = "2",
                    Name = "testDigitalOutput",
                    Version = 1
                },
                Io = IOType.Output
            });
            testSignalLayout.SignalMap.Add("AI1", new Signal()
            {
                Class = "Analog",
                Info = new Info()
                {
                    GUID = "3",
                    Name = "testAnalogInput",
                    Version = 1
                },
                Io = IOType.Input
            });

            UpdateSignals updateMessage = new UpdateSignals()
            {
                Name = "testSignalLayout"
            };
            updateMessage.SignalMap.Add("DO1", new UpdateSignal()
            {
                Class = "Digital",
                Io = UpdateIOType.Output,
                Value = Value.ForNumber(4.2)
            });
            updateMessage.SignalMap.Add("AI1", new UpdateSignal()
            {
                Class = "Digital",
                Io = UpdateIOType.Output,
                Value = Value.ForNumber(3.9)
            });

            System.Diagnostics.Debug.WriteLine("Adding signal layout");
            RobotManager.Instance.AddSignalLayout(testSignalLayout);

            System.Diagnostics.Debug.WriteLine("Starting RobotManager");
            RobotManager.Instance.Start();

            RobotManager.Instance.UpdateQueue.Enqueue(updateMessage);

            Thread.Sleep(500);
            RobotManager.Instance.Stop();
            System.Diagnostics.Debug.WriteLine("Stopped RobotManager");

            Assert.IsTrue(RobotManager.Instance.Robots["testSignalLayout"].CurrentSignals["DO1"].Equals(new UpdateSignal()
            {
                Class = "Digital",
                Io = UpdateIOType.Output,
                Value = Value.ForNumber(4.2)
            }));
        }

        public static void SendData(string server, UpdateSignals message)
        {
            int port = 13000;
            TcpClient client = new TcpClient(server, port);

            var ms = new MemoryStream();

            message.WriteTo(ms);

            int size = message.CalculateSize();
            ms.Seek(0, SeekOrigin.Begin);
            byte[] content = new byte[size];
            ms.Read(content, 0, size);


            byte[] metadata = new byte[sizeof(int)];
            metadata = BitConverter.GetBytes(size);
            if (BitConverter.IsLittleEndian)
                Array.Reverse(metadata);

            using (NetworkStream stream = client.GetStream())
            {
                stream.Write(metadata, 0, metadata.Length);
                stream.Write(content, 0, content.Length);
            }


            client.Close();
        }

    }
}
