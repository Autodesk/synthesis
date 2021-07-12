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

            
            DigitalOutput testDigitalOutput1 = new DigitalOutput()
            {
                Name = "Out1",
                Type = "PWM",
                Value = Value.ForNumber(4.2)
            };

            List<DigitalOutput> outputList1 = new List<DigitalOutput>();
            outputList1.Add(testDigitalOutput1);

            Dictionary<string, DigitalOutput> dos = new Dictionary<string, DigitalOutput>();
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

            foreach (KeyValuePair<string, UpdateMessage.Types.ModifiedFields> kvp in TcpServerManager.Packets)
            {
                System.Diagnostics.Debug.WriteLine(kvp.Key);
            }


            Assert.IsTrue(TcpServerManager.Packets.ContainsKey("test1"));
            Assert.IsFalse(TcpServerManager.Packets.ContainsKey("test420"));
 
        }

        public static void SendData(string server, UpdateMessage message)
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
