using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SynthesisAPI.Utilities;
using Mirabuf;
using Mirabuf.Signal;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.IO;
using Google.Protobuf;
using Google.Protobuf.WellKnownTypes;

namespace RobotProofOfConcept
{
    class RobotConcept
    {
        private Signals RobotLayout { get; set; }
        public ControllableState Robot { get; set; }
        private ConcurrentQueue<UpdateSignals> UpdateQueue { get; set; }
        private bool _isRunning = false;

        public RobotConcept()
        {
            UpdateQueue = new ConcurrentQueue<UpdateSignals>();
        }

        public void Run()
        {
            _isRunning = true;
            SetupLayout();
            Robot = new ControllableState()
            {
                CurrentSignalLayout = RobotLayout
            };

            Thread thr1 = new Thread(() => 
            {
                while (_isRunning)
                {
                    if (UpdateQueue.TryDequeue(out UpdateSignals tmp) && Robot.CurrentInfo.Name.Equals(tmp.Name))
                        Robot.Update(tmp);
                }
            });
            thr1.Start();

            TcpServerManager.SetTargetQueue(UpdateQueue);
            TcpServerManager.Start();

            SendData("127.0.0.1", GetRandomUpdateSignals(Robot.CurrentInfo.Name));

            Thread.Sleep(500);
            _isRunning = false;
            
        }

        public void RunUpdate() 
        {
            _isRunning = true;
            Thread thr1 = new Thread(() =>
            {
                while (_isRunning)
                {
                    if (UpdateQueue.TryDequeue(out UpdateSignals tmp) && Robot.CurrentInfo.Name.Equals(tmp.Name))
                        Robot.Update(tmp);
                }
            });
            thr1.Start();


            SendData("127.0.0.1", GetRandomUpdateSignals(Robot.CurrentInfo.Name));

            Thread.Sleep(500);

            TcpServerManager.Stop();
            _isRunning = false;
            thr1.Join();
        }


        private UpdateSignals GetRandomUpdateSignals(string name)
        {
            Random random = new Random();
            var tmp = new UpdateSignals()
            {
                Name = name
            };
            tmp.SignalMap.Add("DI1", new UpdateSignal()
            {
                DeviceType = "Digital",
                Io = UpdateIOType.Input,
                Value = random.NextDouble() * 5
            });
            tmp.SignalMap.Add("DI2", new UpdateSignal()
            {
                DeviceType = "Digital",
                Io = UpdateIOType.Input,
                Value = random.NextDouble() * 5
            });
            tmp.SignalMap.Add("DO1", new UpdateSignal()
            {
                DeviceType = "Digital",
                Io = UpdateIOType.Output,
                Value = random.NextDouble() * 5
            });
            tmp.SignalMap.Add("AO1", new UpdateSignal()
            {
                DeviceType = "Analog",
                Io = UpdateIOType.Output,
                Value = random.NextDouble() * 5
            });

            return tmp;
        }
        private void SendData(string server, UpdateSignals message)
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
            if (!BitConverter.IsLittleEndian)
                Array.Reverse(metadata);

            using (NetworkStream stream = client.GetStream())
            {
                stream.Write(metadata, 0, metadata.Length);
                stream.Write(content, 0, content.Length);
            }


            client.Close();
        }

        private void SetupLayout()
        {
            RobotLayout = new Signals()
            {
                Info = new Info()
                {
                    GUID = Guid.NewGuid().ToString(),
                    Name = "Robot",
                    Version = 1
                }
            };
            RobotLayout.SignalMap.Add("DI1", new Signal()
            {
                Info = new Info()
                {
                    GUID = Guid.NewGuid().ToString(),
                    Name = "DigitalInput1",
                    Version = 1
                },
                DeviceType = "Digital",
                Io = IOType.Input
            });
            RobotLayout.SignalMap.Add("DI2", new Signal()
            {
                Info = new Info()
                {
                    GUID = Guid.NewGuid().ToString(),
                    Name = "DigitalInput2",
                    Version = 1
                },
                DeviceType = "Digital",
                Io = IOType.Input
            });
            RobotLayout.SignalMap.Add("DO1", new Signal()
            {
                Info = new Info()
                {
                    GUID = Guid.NewGuid().ToString(),
                    Name = "DigitalOutput1",
                    Version = 1
                },
                DeviceType = "Digital",
                Io = IOType.Output
            });
            RobotLayout.SignalMap.Add("AO1", new Signal()
            {
                Info = new Info()
                {
                    GUID = Guid.NewGuid().ToString(),
                    Name = "AnalogOutput1",
                    Version = 1
                },
                DeviceType = "Analog",
                Io = IOType.Output
            });
        }
    }
}

