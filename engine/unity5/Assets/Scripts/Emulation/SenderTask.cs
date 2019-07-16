using EmulationService;
using System;
using UnityEngine;

namespace Synthesis
{

    class SenderTask : GrpcTask
    {
        private EmulationWriter.EmulationWriterClient client = null;

        public SenderTask(
            Channel<IMessage> sender,
            Channel<IMessage> receiver,
            string ip,
            string port,
            double? timeout = null,
            uint? retries = null) : base(sender, receiver, ip, port, timeout, retries){}

        protected override void Connect()
        {
            base.Connect();
            if (SSHClient.IsVMConnected() && !IsConnected() && client == null)
            {
                client = new EmulationWriter.EmulationWriterClient(conn);
            }
        }

        public override void OnCycle()
        {
            base.OnCycle();
            Connect();
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
                if (e is Grpc.Core.RpcException)
                {
                    Debug.Log(e.ToString());
                }
                else
                {
                    Debug.Log(e.ToString());
                }
            }
        }
    }
}