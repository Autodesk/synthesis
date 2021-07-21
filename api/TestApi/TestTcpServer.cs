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
using Google.Protobuf.WellKnownTypes;
using Google.Protobuf;

namespace TestApi {
    
    
    [TestFixture]
    public static class TestTcpServer
    {

        [Test]
        public static void TestReceivingData()
        {

            // Assert.Pass() or Assert.Fail() pretty self explanatory
            // Assert.IsTrue([condition]) or Assert.IsFalse([condition])
            // Loads more, check with intellisense


            UpdateSignal testDigitalOutput1 = new UpdateSignal()
            {
                Io = IOType.Output,
                Class = "PWM",
                Value = Value.ForNumber(4.2)
            };

            List<UpdateSignal> outputList1 = new List<UpdateSignal>();
            outputList1.Add(testDigitalOutput1);

            Dictionary<string, UpdateSignal> dos = new Dictionary<string, DigitalOutput>();
            dos.Add("DigitalOutput1", testDigitalOutput1);

            UpdateMessage testPacket1 = new UpdateMessage()
            {
                Id = "test1",
                Fields = new UpdateMessage.Types.ModifiedFields()
            };
            testPacket1.Fields.DOs.Add(dos);

            List<UpdateMessage> msgs = new List<UpdateMessage>()
            {
                testPacket1
            };

           
            
            TcpServerManager.Start();

            //Thread.Sleep(500);
            

            SendData("127.0.0.1", testPacket1);

            Thread.Sleep(500);

            TcpServerManager.Stop();

            foreach (UpdateSignals s in TcpServerManager.Packets.ToArray())
            {
                System.Diagnostics.Debug.WriteLine(s.Name);
            }


            Assert.IsTrue(TcpServerManager.Packets.ContainsKey("test1"));
            Assert.IsFalse(TcpServerManager.Packets.ContainsKey("test420"));
 
        }

        public static void SendData(string server, UpdateSignal message)
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
