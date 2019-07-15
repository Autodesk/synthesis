using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis
{
    class ReceiverTask : ManagedTask
    {
        private Grpc.Core.Channel conn = null;
        private EmulationService.EmulationReader.EmulationReaderClient client = null;
        private Grpc.Core.AsyncServerStreamingCall<EmulationService.RobotOutputsResponse> call;

        string ip;
        string port;
        double? timeout = null;
        uint? retries = null;
        int connectionAttempts = 0;

        public bool IsConnected()
        {
            return conn != null && conn.State != Grpc.Core.ChannelState.TransientFailure && conn.State != Grpc.Core.ChannelState.Shutdown;
        }

        public ReceiverTask(
            Channel<IMessage> sender,
            Channel<IMessage> receiver,
            string ip,
            string port,
            double? timeout = null,
            uint? retries = null): base(sender, receiver)
        {
            this.ip = ip;
            this.port = port;
            this.timeout = timeout;
            this.retries = retries;
        }

        private void Connect()
        {
            conn = new Grpc.Core.Channel(ip + ":" + port, Grpc.Core.ChannelCredentials.Insecure);
            if (timeout != null)
            {
                try
                {
                    conn.ConnectAsync(DateTime.UtcNow.AddSeconds(timeout.Value)).Wait();
                    connectionAttempts = 0;
                }
                catch (Exception e)
                {
                    connectionAttempts++;
                    if (retries != null && connectionAttempts > retries.Value)
                    {
                        stateChannel.Send(new GrpcMessage.ConnectionError());
                        stateChannel.Send(new StandardMessage.ThreadStoppedMessage());
                        connectionAttempts = 0;
                        Debug.Log(e.Message);
                        Pause();
                        return;
                    }
                }
            }
            client = new EmulationService.EmulationReader.EmulationReaderClient(conn);
            call = client.RobotOutputs(new EmulationService.RobotOutputsRequest { });
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
                call.ResponseStream.MoveNext();

                OutputManager.Instance = call.ResponseStream.Current.OutputData;
            } catch (Exception e)
            {
                conn = null;
                // Debug.Log(e.StackTrace);
            }
        }

    }
}