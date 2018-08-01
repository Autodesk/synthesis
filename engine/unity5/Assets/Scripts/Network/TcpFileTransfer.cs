using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Synthesis.Network
{
    public class TcpFileTransfer
    {
        const int BufferSize = 1024;

        /// <summary>
        /// Returns the localhost IP address.
        /// </summary>
        private static IPAddress LocalhostAddress
        {
            get
            {
                return Dns.GetHostEntry(Dns.GetHostName()).AddressList.
                    First(a => a.AddressFamily == AddressFamily.InterNetwork);
            }
        }

        /// <summary>
        /// Sends all files from the given source directory to the network address and port provided.
        /// </summary>
        /// <param name="networkAddress"></param>
        /// <param name="port"></param>
        /// <param name="sourceDirectory"></param>
        public static void SendFiles(string networkAddress, int port, string sourceDirectory)
        {
            IPAddress address;

            if (!IPAddress.TryParse(networkAddress, out address))
                address = LocalhostAddress;

            IPEndPoint endPoint = new IPEndPoint(address, port);

            Socket socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endPoint);

            Task.Run(() => SendAllFiles(socket, sourceDirectory)).ContinueWith(t =>
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        /// <summary>
        /// Sends all files in the given directory through the provided socket.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="sourceDirectory"></param>
        private static void SendAllFiles(Socket socket, string sourceDirectory)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            foreach (string fileName in Directory.GetFiles(sourceDirectory))
            {
                FileInfo fileInfo = new FileInfo(fileName);

                writer.Write(fileInfo.Length);
                writer.Write(fileInfo.Name);

                socket.SendFile(fileName, stream.ToArray(), null,
                    TransmitFileOptions.UseDefaultWorkerThread);

                stream.SetLength(0);
            }

            writer.Write(-1L);
            socket.Send(stream.ToArray());
        }

        /// <summary>
        /// Receives all files received from the given port and saves them to the provided
        /// directory.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="saveDirectory"></param>
        public static void ReceiveFiles(int port, string saveDirectory)
        {
            TcpListener listener = new TcpListener(LocalhostAddress, port);
            listener.Start();

            Directory.CreateDirectory(saveDirectory);

            Task.Run(() => ReceiveAllFiles(listener, saveDirectory));
        }

        /// <summary>
        /// Receives all files through the given <see cref="TcpListener"/> and saves them to
        /// the provided directory.
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="saveDirectory"></param>
        private static void ReceiveAllFiles(TcpListener listener, string saveDirectory)
        {
            using (TcpClient client = listener.AcceptTcpClient())
            using (NetworkStream stream = client.GetStream())
            {
                BinaryReader binaryReader = new BinaryReader(stream);
                long bytesLeft;

                while ((bytesLeft = binaryReader.ReadInt64()) != -1)
                {
                    string fileName = binaryReader.ReadString();

                    using (FileStream file = File.Create(saveDirectory + "\\" + fileName))
                    {
                        while (true)
                        {
                            if (bytesLeft < BufferSize)
                            {
                                int remaining = Convert.ToInt32(bytesLeft);
                                file.Write(binaryReader.ReadBytes(remaining), 0, remaining);
                                break;
                            }
                            else
                            {
                                file.Write(binaryReader.ReadBytes(BufferSize), 0, BufferSize);
                                bytesLeft -= BufferSize;
                            }
                        }
                    }
                }
            }
        }
    }
}
