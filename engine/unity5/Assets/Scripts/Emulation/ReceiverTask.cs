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
            if (EmulatorManager.IsVMConnected() && client == null)
            {
                client = new EmulationReader.EmulationReaderClient(conn);
            }
        }

        public override async void OnCycle()
        {
            base.OnCycle();
            Connect();
            if (IsConnected())
            {
                if (call == null)
                {
                    call = client.RobotOutputs(new RobotOutputsRequest { });
                }
                try
                {
                    await call.ResponseStream.MoveNext();

                    OutputManager.Instance = call.ResponseStream.Current.OutputData;
                }
                catch (Exception e)
                {
                    if (e is Grpc.Core.RpcException)
                    {
                        Debug.Log(e.ToString());
                    }
                    else
                    {
                        Debug.Log(e.ToString());
                    }
                    System.Threading.Thread.Sleep(100);
                }
            }
            if (conn.State == Grpc.Core.ChannelState.TransientFailure)
            {
                client = null;
            }
        }
    }
}