using EmulationService;
using System;
using UnityEngine;

namespace Synthesis
{

    class SenderTask : GrpcTask
    {
        private EmulationWriter.EmulationWriterClient client = null;
        private Grpc.Core.AsyncClientStreamingCall<UpdateRobotInputsRequest, UpdateRobotInputsResponse> call = null;

        public SenderTask(
            Channel<IMessage> sender,
            Channel<IMessage> receiver,
            string ip,
            string port,
            double? timeout = null,
            uint? retries = null) : base(sender, receiver, ip, port, timeout, retries) { }

        protected override void Connect()
        {
            base.Connect();
            if (EmulatorManager.IsVMConnected() && client == null)
            {
                client = new EmulationWriter.EmulationWriterClient(conn);
            }
        }

        public override async void OnCycle()
        {
            base.OnCycle();
            Connect();
            if (IsConnected())
            { 
                if(call == null)
                {
                    call = client.RobotInputs();
                }
                try
                {
                    var a = EmulatedRoboRIO.RobotInputs;
                    //Debug.Log(a.Joysticks[0].ToString());

                    await call.RequestStream.WriteAsync(new UpdateRobotInputsRequest
                    {
                        Api = "v1",
                        InputData = EmulatedRoboRIO.RobotInputs,
                    });
                    System.Threading.Thread.Sleep(30);
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