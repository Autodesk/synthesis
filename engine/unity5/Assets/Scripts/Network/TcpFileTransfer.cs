using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Synthesis.Network
{
    public class TcpFileTransfer
    {
        const int BufferSize = 1024;

        private readonly Queue<string> fileQueue;
        private readonly IPAddress address;
        private readonly int port;

        private Socket socket;
        private TcpListener listener;
        private TcpClient tcpClient;
        private Stream stream;

        private Action<string> onReadyToSend;
        private Action onSendingComplete;

        public TcpFileTransfer(string ipAddress, int port)
        {
            fileQueue = new Queue<string>();

            IPHostEntry ipHost = Dns.GetHostEntry(Dns.GetHostName()/*ipAddress)*/);
            address = ipHost.AddressList[0];
            this.port = port;
            //ipEndPoint = new IPEndPoint(ipAddr, port);
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
                fileQueue.Enqueue(fileName);

            onReadyToSend(fileQueue.Dequeue());
        }

        public void QuitTransfer()
        {
            socket.Shutdown(SocketShutdown.Both);
            socket.Close();
        }

        public void StartTcpListener()
        {
            listener = new TcpListener(IPAddress.Loopback, port);//new TcpListener(IPAddress.Any, ipEndPoint.Port);//new TcpListener(ipEndPoint);
            listener.Start();
            tcpClient = listener.AcceptTcpClient();
            stream = tcpClient.GetStream();

            //if (tcpClient == null)
            //{
                UnityEngine.Debug.Log("Listener initialized on port " + port);
                return;
            //}
        }

        public void SendNextFile()
        {
            Task sendTask = new Task(() => socket.SendFile(fileQueue.Dequeue()));
            sendTask.ContinueWith(t =>
            {
                if (fileQueue.Any())
                    onReadyToSend(fileQueue.Peek());
                else
                    onSendingComplete();
            }, TaskScheduler.FromCurrentSynchronizationContext());
            //socket.BeginSendFile(fileName, OnSendComplete, socket);
        }

        public void ReadNextFile(string savePath)
        {
            // TODO: for some reason tcpClient is null after being assigned to, or so it seems.
            // There is a NullReferenceException being thrown here.
            // Look at this with fresh eyes and see if you see a problem and could find a solution to it.
            // Update: I think this is happening because the second client is rejecting connection because
            // the port is shared? Let's figure that out.
            // Update 2: Ok there's no way that's the issue. Either things are happening out of order or
            // The wrong TcpFileTransfer instance is being initialized as a listener.
            if (listener/*stream*//*tcpClient*/ == null)
            {
                UnityEngine.Debug.Log("Read returned null on port " + port);
                return;
            }
            
            //using (var stream = tcpClient.GetStream())
            using (var output = File.Create(savePath))
            {
                byte[] buffer = new byte[BufferSize];
                int bytesRead;

                while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
                    output.Write(buffer, 0, bytesRead);
            }
        }

        private void OnSendComplete(IAsyncResult ar)
        {
            Socket client = ar.AsyncState as Socket;
            client.EndSendFile(ar);
            
            if (fileQueue.Any())
            {
                onReadyToSend(fileQueue.Dequeue());
            }
            else
            {
                onSendingComplete();
                client.Shutdown(SocketShutdown.Both);
                client.Close();
            }
        }
    }
}
