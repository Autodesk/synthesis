using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Synthesis.Network
{
    public class TcpFileSender
    {
        private readonly Queue<FileInfo> fileQueue;
        private readonly IPAddress address;
        private readonly int port;

        private Socket socket;

        private Action<string> onReadyToSend;
        private Action onSendingComplete;

        public TcpFileSender(string networkAddress, int port)
        {
            fileQueue = new Queue<FileInfo>();

            if (!IPAddress.TryParse(networkAddress, out address))
                address = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork);

            this.port = port;
        }

        public static void SendFiles(string networkAddress, int port, string sourceDirectory)
        {
            IPAddress address;

            if (!IPAddress.TryParse(networkAddress, out address))
                address = Dns.GetHostEntry(Dns.GetHostName()).AddressList.First(a => a.AddressFamily == AddressFamily.InterNetwork);

            IPEndPoint endPoint = new IPEndPoint(address, port);

            Socket socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(endPoint);

            Task.Run(() =>
            {
                List<byte> preBuffer = new List<byte>();

                foreach (string fileName in Directory.GetFiles(sourceDirectory))
                {
                    preBuffer.Clear();

                    FileInfo fileInfo = new FileInfo(fileName);

                    MemoryStream stream = new MemoryStream();
                    BinaryWriter writer = new BinaryWriter(stream);

                    writer.Write(fileInfo.Name);
                    writer.Write(fileInfo.Length);

                    socket.SendFile(fileName, stream.ToArray(), null,
                        TransmitFileOptions.UseDefaultWorkerThread);
                }
            }).ContinueWith(t =>
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }

        public void StartTcpSender(string sourceDirectory, Action<string> readyToSend, Action sendingComplete)
        {
            onReadyToSend = readyToSend;
            onSendingComplete = sendingComplete;

            fileQueue.Clear();

            IPEndPoint ipEndPoint = new IPEndPoint(address, port);

            socket = new Socket(address.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
            socket.Connect(ipEndPoint);

            foreach (string fileName in Directory.GetFiles(sourceDirectory))
                fileQueue.Enqueue(new FileInfo(fileName));

            onReadyToSend(fileQueue.Peek().Name);
        }

        public void QuitTransfer()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        public void SendNextFile()
        {
            FileInfo info = fileQueue.Dequeue();

            List<byte> preBuffer = new List<byte>();
            preBuffer.AddRange(Encoding.ASCII.GetBytes(info.Name));
            preBuffer.AddRange(BitConverter.GetBytes(info.Length));

            Task sendTask = new Task(() => socket.SendFile(info.FullName, preBuffer.ToArray(), null,
                TransmitFileOptions.UseDefaultWorkerThread));
            sendTask.ContinueWith(t =>
            {
                socket.Close();
                //if (fileQueue.Any())
                //    onReadyToSend(new FileInfo(fileQueue.Peek()).Name);
                //else
                //    onSendingComplete();
            }, TaskScheduler.FromCurrentSynchronizationContext());
            sendTask.Start();
        }
    }
}
