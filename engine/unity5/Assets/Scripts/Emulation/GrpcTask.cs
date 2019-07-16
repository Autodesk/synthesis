using EmulationService;
using System;
using UnityEngine;

namespace Synthesis
{
    abstract class GrpcTask : ManagedTask
    {
        protected Grpc.Core.Channel conn = null;

        protected string ip;
        protected string port;
        protected double? timeout = null;
        protected uint? retries = null;
        protected int connectionAttempts = 0;
        protected bool clientConnected = false;

        public bool IsConnected()
        {
            Debug.Log("Connection status(" + System.Threading.Thread.CurrentThread.ManagedThreadId + "): " + (conn == null ? "null" : conn.State.ToString()));
            return conn != null && (conn.State == Grpc.Core.ChannelState.Ready || conn.State == Grpc.Core.ChannelState.Connecting || conn.State == Grpc.Core.ChannelState.Idle);
        }

        public GrpcTask(
            Channel<IMessage> sender,
            Channel<IMessage> receiver,
            string ip,
            string port,
            double? timeout = null,
            uint? retries = null) : base(sender, receiver)
        {
            this.ip = ip;
            this.port = port;
            this.timeout = timeout;
            this.retries = retries;
        }

        public void OnConnect()
        {
            Connect();
        }

        protected virtual void Connect()
        {
            if (conn == null)
                conn = new Grpc.Core.Channel(ip + ":" + port, Grpc.Core.ChannelCredentials.Insecure);
        }

        /*
        protected void Connect()
        {
            Debug.Log("IsConnected:" + IsConnected());
            if (!IsConnected())
            {
                conn = new Grpc.Core.Channel(ip + ":" + port, Grpc.Core.ChannelCredentials.Insecure);
                Debug.Log("IsConnected2:" + IsConnected());
                if (timeout != null && !IsConnected())
                {
                    Debug.Log("connection attempts:" + connectionAttempts + " Connected:" + IsConnected());
                    conn.ConnectAsync(deadline: DateTime.UtcNow.AddSeconds(timeout.Value)).RunSynchronously();
                    System.Threading.Thread.Sleep((int)timeout.Value * 1000);
                    connectionAttempts++;
                    if (retries != null && connectionAttempts > retries.Value)
                    {
                        stateChannel.Send(new GrpcMessage.ConnectionErrorMessage());
                        stateChannel.Send(new StandardMessage.ThreadStoppedMessage());
                        connectionAttempts = 0;
                        Pause();
                        return;
                    }
                }
            }
        }
        */
        public override void OnMessage()
        {
            var currentCommand = commandChannel.TryPeek();
            if (currentCommand.IsValid())
            {
                switch (currentCommand.Get().GetName())
                {
                    case GrpcMessage.Connect:
                        OnConnect();
                        commandChannel.Pop();
                        break;
                    default:
                        base.OnMessage();
                        break;
                }
            }
        }

        public override void OnStart()
        {
            base.OnStart();
        }

        public override void OnCycle()
        {
            base.OnCycle();
            /*
            if (SSHClient.IsVMConnected() && !IsConnected()) // Ensure it connected once already
            {
                Debug.Log("Connection Closed");
                stateChannel.Send(new GrpcMessage.ConnectionErrorMessage());
                commandChannel.Send(new StandardMessage.StopMessage());
                Connect();
            }
            */
        }
    }
}