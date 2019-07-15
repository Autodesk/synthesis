//using System;
//using System.IO;
//using System.Linq;
//using System.Net;
//using System.Net.Sockets;
//using System.Threading.Tasks;

//namespace Synthesis.Network
//{
//    public class TcpFileTransfer
//    {
//        /// <summary>
//        /// The size of the buffer used to receive file data.
//        /// </summary>
//        private const int BufferSize = 1024;

//        /// <summary>
//        /// Returns the local IP address.
//        /// </summary>
//        private static IPAddress LocalAddress => Dns.GetHostEntry(Dns.GetHostName()).
//            AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork);

//        /// <summary>
//        /// Sends all given files to the network address and port provided.
//        /// </summary>
//        /// <param name="networkAddress">The network address to send the files to.</param>
//        /// <param name="port">The network port to connect to.</param>
//        /// <param name="files">A list of file paths pointing to the files to send.</param>
//        public static void SendFiles(string networkAddress, int port, string[] files)
//        {
//            SendFiles(new string[] { networkAddress }, port, files);
//        }

//        /// <summary>
//        /// Sends all given files to the network addresses and port provided.
//        /// </summary>
//        /// <param name="networkAddresses">A list of network addresses to send the files to.</param>
//        /// <param name="port">The network port to connect to.</param>
//        /// <param name="files">A list of file paths pointing to the files to send.</param>
//        public static void SendFiles(string[] networkAddresses, int port, string[] files)
//        {
//            Socket[] sockets = new Socket[networkAddresses.Length];
//            Task[] tasks = new Task[networkAddresses.Length];

//            for (int i = 0; i < networkAddresses.Length; i++)
//            {
//                IPAddress address;

//                // If the address is not parseable (e.g. "localhost"), then use the local IP address as a fallback.
//                if (!IPAddress.TryParse(networkAddresses[i], out address))
//                    address = LocalAddress;

//                IPEndPoint endPoint = new IPEndPoint(address, port);

//                sockets[i] = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
//                tasks[i] = sockets[i].ConnectAsync(endPoint);
//            }

//            // Wait for all sockets to connect before sending files.
//            Task.WhenAll(tasks).ContinueWith(t => SendAllFiles(sockets, files)).ContinueWith(t =>
//            {
//                foreach (Socket socket in sockets)
//                {
//                    socket.Shutdown(SocketShutdown.Both);
//                    socket.Close();
//                }
//            });
//        }

//        /// <summary>
//        /// Sends all given files through the provided sockets.
//        /// </summary>
//        /// <param name="socket">The sockets used to send the files.</param>
//        /// <param name="files">The paths of the files to send.</param>
//        private static void SendAllFiles(Socket[] sockets, string[] files)
//        {
//            MemoryStream stream = new MemoryStream();
//            BinaryWriter writer = new BinaryWriter(stream);
            
//            foreach (string file in files)
//            {
//                FileInfo fileInfo = new FileInfo(file);

//                writer.Write(fileInfo.Length);
//                writer.Write(fileInfo.Name);
//                writer.Write(File.ReadAllBytes(file));

//                SendStream(sockets, stream);

//                // Reset the stream for the next iteration.
//                stream.SetLength(0);
//            }

//            // Write -1L as the next file size to indicate the end of the stream.
//            writer.Write(-1L);

//            SendStream(sockets, stream);
//        }

//        /// <summary>
//        /// Sends all bytes in the given stream to the provided sockets.
//        /// </summary>
//        /// <param name="sockets"></param>
//        /// <param name="stream"></param>
//        private static void SendStream(Socket[] sockets, MemoryStream stream)
//        {
//            foreach (Socket socket in sockets)
//                socket.Send(stream.ToArray());
//        }

//        /// <summary>
//        /// Receives all files received from the given port and saves them to the provided
//        /// directory.
//        /// </summary>
//        /// <param name="port">The port to receive the files from.</param>
//        /// <param name="saveDirectory">The directory where the new files will be saved.</param>
//        /// <param name="fileReceived">An action called on the main thread when a file is done being received.</param>
//        public static void ReceiveFiles(int port, string saveDirectory, Action<string> fileReceived = null)
//        {
//            TcpListener listener = new TcpListener(LocalAddress, port);
//            listener.Start();

//            Task.Run(() => listener.AcceptTcpClient()).ContinueWith(t =>
//            {
//                Directory.CreateDirectory(saveDirectory);
//                NetworkStream stream = t.Result.GetStream();

//                // Read all files from the stream, closing the stream and TcpClient when done.
//                ReadFiles(saveDirectory, new BinaryReader(stream), fileReceived, () =>
//                {
//                    stream.Close();
//                    t.Result.Close();
//                });
//            }, TaskScheduler.FromCurrentSynchronizationContext()).ContinueWith(t => listener.Stop());
//        }

//        /// <summary>
//        /// Recursively receives all files read from the given <see cref="BinaryReader"/> and saves them to the
//        /// provided directory. Provided actions are called on the main thread.
//        /// </summary>
//        /// <param name="saveDirectory">The directory where the new files will be saved.</param>
//        /// <param name="reader">The reader used to read each file.</param>
//        /// <param name="fileReceived">An action called on the main thread when a file is done being received.</param>
//        /// <param name="whenDone">An action called on the main thread when all files have been read.</param>
//        private static void ReadFiles(string saveDirectory, BinaryReader reader, Action<string> fileReceived, Action whenDone)
//        {
//            Task.Run(() => ReadFile(saveDirectory, reader)).ContinueWith(t =>
//            {
//                if (t.Result == null)
//                {
//                    whenDone();
//                }
//                else
//                {
//                    fileReceived?.Invoke(t.Result);
//                    ReadFiles(saveDirectory, reader, fileReceived, whenDone);
//                }
//            }, TaskScheduler.FromCurrentSynchronizationContext());
//        }

//        /// <summary>
//        /// Reads a single file from the given <see cref="BinaryReader"/> and saves it to the
//        /// provided directory.
//        /// </summary>
//        /// <param name="saveDirectory">The directory where the new file will be saved.</param>
//        /// <param name="reader">The reader used to read the file.</param>
//        /// <returns>The name of the file read.</returns>
//        private static string ReadFile(string saveDirectory, BinaryReader reader)
//        {
//            long bytesLeft = reader.ReadInt64();

//            // If the size of the file is -1L, the end of the stream has been reached.
//            if (bytesLeft == -1L)
//                return null;

//            string fileName = reader.ReadString();

//            using (FileStream file = File.Create(saveDirectory + Path.DirectorySeparatorChar + fileName))
//            {
//                while (true)
//                {
//                    if (bytesLeft < BufferSize)
//                    {
//                        int remaining = Convert.ToInt32(bytesLeft);
//                        file.Write(reader.ReadBytes(remaining), 0, remaining);
//                        break;
//                    }
//                    else
//                    {
//                        file.Write(reader.ReadBytes(BufferSize), 0, BufferSize);
//                        bytesLeft -= BufferSize;
//                    }
//                }
//            }

//            return fileName;
//        }
//    }
//}
