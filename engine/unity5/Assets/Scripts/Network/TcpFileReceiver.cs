using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace Synthesis.Network
{
    public class TcpFileReceiver
    {
        const int BufferSize = 1024;

        private readonly int port;

        private Action onReceivingComplete;

        private NetworkStream networkStream;

        public TcpFileReceiver(int port)
        {
            this.port = port;
        }

        public static void ReceiveFiles(int port, string saveDirectory)
        {
            TcpListener listener = new TcpListener(Dns.GetHostEntry(Dns.GetHostName())
                .AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork), port);
            listener.Start();

            Directory.CreateDirectory(saveDirectory);

            Task.Run(() =>
            {
                using (TcpClient client = listener.AcceptTcpClient())
                using (NetworkStream stream = client.GetStream())
                {
                    bool receiving = true;

                    while (receiving)
                    {
                        BinaryReader reader = new BinaryReader(stream);
                        string fileName = reader.ReadString();
                        long bytesLeft = reader.ReadInt64();

                        using (FileStream file = File.Create(saveDirectory + "\\" + fileName))
                        {
                            while (true)
                            {
                                if (bytesLeft < BufferSize)
                                {
                                    int remaining = Convert.ToInt32(bytesLeft);
                                    file.Write(reader.ReadBytes(remaining), 0, remaining);
                                    break;
                                }
                                else
                                {
                                    file.Write(reader.ReadBytes(BufferSize), 0, BufferSize);
                                    bytesLeft -= BufferSize;
                                }
                            }
                        }

                        // TODO: Make it so it knows when all the files are transferred, if it doesn't already.
                    }

                }
            });
        }

        public void StartTcpReceiver(Action receivingComplete)
        {
            onReceivingComplete = receivingComplete;

            TcpListener listener = new TcpListener(
                Dns.GetHostEntry(Dns.GetHostName()).AddressList
                .First(a => a.AddressFamily == AddressFamily.InterNetwork),
                port);

            listener.Start();
            TcpClient tcpClient = listener.AcceptTcpClient();
            networkStream = tcpClient.GetStream();
        }

        public void ReceiveNextFile(string savePath)
        {
            // TODO: Check if a current task is running and end it if it is.
            // Also maybe just check to make sure it doesn't already work lol.
            using (var output = File.Create(savePath))
            {
                Task.Run(() =>
                {
                    byte[] buffer = new byte[BufferSize];
                    int bytesRead;

                    while ((bytesRead = networkStream.Read(buffer, 0, buffer.Length)) > 0)
                        output.Write(buffer, 0, bytesRead);

                    onReceivingComplete();
                });
            }
        }
    }
}
