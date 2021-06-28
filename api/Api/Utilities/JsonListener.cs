using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace SynthesisAPI.Utilities
{
    public static class JsonListener
    {
        private static TcpListener server = null;
        public static bool Open { get; set; } = true;
        public static Dictionary<string, List<DigitalOutput>> Packets = new Dictionary<string, List<DigitalOutput>>();

        public static void Listen()
        {
            try
            {
                int port = 13000;
                IPAddress localAddr = IPAddress.Parse("127.0.0.1");
                server = new TcpListener(localAddr, port);

                Byte[] bytes = new Byte[256];

                server.Start();

                while (Open)
                {
                    TcpClient client = server.AcceptTcpClient();
                    NetworkStream stream = client.GetStream();

                    string data;
                    int i;
                    while ((i = stream.Read(bytes, 0, bytes.Length)) != 0)
                    {
                        data = System.Text.Encoding.ASCII.GetString(bytes, 0, i);

                        AddPacket(DeserializeData(data));

                        data = data.ToUpper();
                        byte[] msg = System.Text.Encoding.ASCII.GetBytes(data);
                        stream.Write(msg, 0, msg.Length);
                    }
                    client.Close();
                }
            }
            finally
            {
                server.Stop();
            }
            
        }

        private static APIPacket DeserializeData(string data) 
        {
            JSchemaGenerator schemaGenerator = new JSchemaGenerator();
            JSchema schema = schemaGenerator.Generate(typeof(APIPacket));

            JObject apiPacket = JObject.Parse(data);

            if (apiPacket.IsValid(schema))
            {
                return JsonConvert.DeserializeObject<APIPacket>(data);
            }
            throw new Exception("Invalid Json Object");
        }

        private static void AddPacket(APIPacket packet)
        {
            Packets.Add(packet.Id, packet.Data);
        }

    }
}