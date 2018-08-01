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
        /// Returns the local IP address.
        /// </summary>
        private static IPAddress LocalAddress => Dns.GetHostEntry(Dns.GetHostName()).
            AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork);

        /// <summary>
        /// Sends all given files to the network address and port provided.
        /// </summary>
        /// <param name="networkAddress"></param>
        /// <param name="port"></param>
        /// <param name="sourceDirectory"></param>
        public static void SendFiles(string networkAddress, int port, string[] files)
        {
            SendFiles(new string[] { networkAddress }, port, files);
        }

        /// <summary>
        /// Sends all given files to the network addresses and port provided.
        /// </summary>
        /// <param name="networkAddresses"></param>
        /// <param name="port"></param>
        /// <param name="files"></param>
        public static void SendFiles(string[] networkAddresses, int port, string[] files)
        {
            Socket[] sockets = new Socket[networkAddresses.Length];
            Task[] tasks = new Task[networkAddresses.Length];

            for (int i = 0; i < networkAddresses.Length; i++)
            {
                IPAddress address;

                if (!IPAddress.TryParse(networkAddresses[i], out address))
                    address = LocalAddress;

                IPEndPoint endPoint = new IPEndPoint(address, port);

                sockets[i] = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
                tasks[i] = sockets[i].ConnectAsync(endPoint);
            }

            Task.WhenAll(tasks).ContinueWith(t => SendAllFiles(sockets, files)).ContinueWith(t =>
            {
                foreach (Socket socket in sockets)
                {
                    socket.Shutdown(SocketShutdown.Both);
                    socket.Close();
                }
            });
        }

        /// <summary>
        /// Sends all given files through the provided sockets.
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="sourceDirectory"></param>
        private static void SendAllFiles(Socket[] sockets, string[] files)
        {
            MemoryStream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);
            
            foreach (string file in files)
            {
                FileInfo fileInfo = new FileInfo(file);

                writer.Write(fileInfo.Length);
                writer.Write(fileInfo.Name);
                writer.Write(File.ReadAllBytes(file));

                SendStream(sockets, stream);

                stream.SetLength(0);
            }

            writer.Write(-1L);

            SendStream(sockets, stream);
        }

        /// <summary>
        /// Sends all bytes in the given stream to the provided sockets.
        /// </summary>
        /// <param name="sockets"></param>
        /// <param name="stream"></param>
        private static void SendStream(Socket[] sockets, MemoryStream stream)
        {
            foreach (Socket socket in sockets)
                socket.Send(stream.ToArray());
        }

        /// <summary>
        /// Receives all files received from the given port and saves them to the provided
        /// directory.
        /// </summary>
        /// <param name="port"></param>
        /// <param name="saveDirectory"></param>
        public static void ReceiveFiles(int port, string saveDirectory, Action<string> fileReceived = null)
        {
            TcpListener listener = new TcpListener(LocalAddress, port);
            listener.Start();

            Directory.CreateDirectory(saveDirectory);

            Task.Run(() => ReceiveAllFiles(listener, saveDirectory, fileReceived)).ContinueWith(t => listener.Stop());
        }

        /// <summary>
        /// Receives all files through the given <see cref="TcpListener"/> and saves them to
        /// the provided directory.
        /// </summary>
        /// <param name="listener"></param>
        /// <param name="saveDirectory"></param>
        private static void ReceiveAllFiles(TcpListener listener, string saveDirectory, Action<string> fileReceived)
        {
            using (TcpClient client = listener.AcceptTcpClient())
            using (NetworkStream stream = client.GetStream())
            {
                BinaryReader reader = new BinaryReader(stream);
                long bytesLeft;

                while ((bytesLeft = reader.ReadInt64()) != -1)
                {
                    string fileName = reader.ReadString();

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

                    fileReceived?.Invoke(fileName);
                }
            }
        }
    }
}
