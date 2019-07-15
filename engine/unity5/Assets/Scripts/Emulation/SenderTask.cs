using EmulationService;
using System;
using UnityEngine;

namespace Synthesis
{

    class SenderTask : ManagedTask
    {
        private Grpc.Core.Channel conn = null;
        private EmulationWriter.EmulationWriterClient client = null;
        private Grpc.Core.AsyncClientStreamingCall<UpdateRobotInputsRequest, UpdateRobotInputsResponse> call;

        string ip;
        string port;
        double? timeout = null;
        uint? retries = null;
        int connectionAttempts = 0;

        public bool IsConnected()
        {
            return conn != null && conn.State != Grpc.Core.ChannelState.TransientFailure && conn.State != Grpc.Core.ChannelState.Shutdown;
        }

        public SenderTask(
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

        private void Connect()
        {
            if (conn == null)
                conn = new Grpc.Core.Channel(ip + ":" + port, Grpc.Core.ChannelCredentials.Insecure);
            if (timeout != null)
            {
                try
                {
                    conn.ConnectAsync(DateTime.UtcNow.AddSeconds(timeout.Value)).Wait();
                    connectionAttempts = 0;
                }
                catch (Exception)
                {
                    connectionAttempts++;
                    if (retries != null && connectionAttempts > retries.Value)
                    {
                        stateChannel.Send(new GrpcMessage.ConnectionError());
                        stateChannel.Send(new StandardMessage.ThreadStoppedMessage());
                        connectionAttempts = 0;
                        Pause();
                        return;
                    }
                }
            }
            client = new EmulationWriter.EmulationWriterClient(conn);
            call = client.RobotInputs();
        }

        public override void OnStart()
        {
            Connect();
            base.OnStart();
        }

        public override void OnCycle()
        {
            base.OnCycle();
            if (!IsConnected())
            {
                Debug.Log("Connection Closed");
                stateChannel.Send(new GrpcMessage.ConnectionError());
                commandChannel.Send(new StandardMessage.StopMessage());
                Connect();
            }
            try
            {
                var a = InputManager.Instance;
                //Debug.Log(a.Joysticks[0].ToString());

                client.RobotInputs().RequestStream.WriteAsync(new UpdateRobotInputsRequest
                {
                    Api = "v1",
                    InputData = InputManager.Instance,
                }).Wait();
                System.Threading.Thread.Sleep(30);
            }
            catch (Exception e)
            {
                conn = null;
                // Debug.Log(e.StackTrace);
            }
        }

    }
}