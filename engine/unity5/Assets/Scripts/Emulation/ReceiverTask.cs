using EmulationService;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Synthesis
{
    class ReceiverTask : GrpcTask
    {
        protected EmulationReader.EmulationReaderClient client = null;
        private Grpc.Core.AsyncServerStreamingCall<RobotOutputsResponse> call;
        public ReceiverTask(
            Channel<IMessage> sender,
            Channel<IMessage> receiver,
            string ip,
            string port,
            double? timeout = null,
            uint? retries = null): base(sender, receiver, ip, port, timeout, retries){}

        protected override void Connect()
        {
            base.Connect();
            if (SSHClient.IsVMConnected() && !IsConnected() && client == null)
            {
                client = new EmulationReader.EmulationReaderClient(conn);
                call = client.RobotOutputs(new RobotOutputsRequest { });
            }
        }

        public override void OnCycle()
        {
            base.OnCycle();
            Connect();
            try
            {
                call.ResponseStream.MoveNext().Wait();

                OutputManager.Instance = call.ResponseStream.Current.OutputData;
            }
            catch (Exception e)
            {
                if(e is Grpc.Core.RpcException)
                {
                    Debug.Log(e.ToString());
                } else
                {
                    Debug.Log(e.ToString());
                }
            }
        }
    }
}