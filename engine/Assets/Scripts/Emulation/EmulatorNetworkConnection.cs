using System;
using EmulationService;
using UnityEngine;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Synthesis
{
    /// <summary>
    /// Manager class for the gRPC connection to HEL running in the user program
    /// 
    /// It's lifetime is connected to a game object
    /// </summary>
    public class EmulatorNetworkConnection : MonoBehaviour
    {
        public static EmulatorNetworkConnection Instance { get; private set; }

        public const string DEFAULT_PORT = "50051";
        public const string DEFAULT_NATIVE_PORT = "50052";
        public const string DEFAULT_JAVA_PORT = "50053";
   
        private const int LOOP_DELAY = 50; // ms
        private const int ERROR_DELAY = 100; // ms

        private bool senderConnected = false, receiverConnected = false;

        private const string API_VERSION = "v1";

        private bool isConnectionOpen = false;

        public void Awake()
        {
            Instance = this;
        }

        /// <summary>
        /// Attempt to open the connection to HEL
        /// </summary>
        public void OpenConnection()
        {
            if(senderConnected != receiverConnected)
            {
                isConnectionOpen = false; // Kill both threads so we can restart them
            }
            if (!senderConnected && !receiverConnected)
            {
                isConnectionOpen = true;
                Task.Factory.StartNew(SendData, TaskCreationOptions.LongRunning);
                Task.Factory.StartNew(ReceiveData, TaskCreationOptions.LongRunning);
            }
        }

        /// <summary>
        /// Send robot input data to HEL
        /// </summary>
        private async void SendData()
        {
            var conn = new Grpc.Core.Channel(EmulatorManager.DEFAULT_HOST + ":" + ((EmulatorManager.programType == UserProgram.Type.JAVA) ? DEFAULT_JAVA_PORT : DEFAULT_NATIVE_PORT), Grpc.Core.ChannelCredentials.Insecure);
            var client = new EmulationWriter.EmulationWriterClient(conn);
            while (EmulatorManager.IsTryingToRunRobotCode() && Instance && isConnectionOpen) // Run while robot code is running or until the object stops existing
            {
                try
                {
                    using (var call = client.RobotInputs())
                    {
                        while(EmulatorManager.IsTryingToRunRobotCode() && Instance && isConnectionOpen)
                        {
                            await call.RequestStream.WriteAsync(new UpdateRobotInputsRequest
                            {
                                Api = API_VERSION,
                                TargetPlatform = EmulatorManager.programType == UserProgram.Type.JAVA ? TargetPlatform.Java : TargetPlatform.Native,
                                InputData = EmulatedRoboRIO.RobotInputs,
                            });
                            senderConnected = true;
                            // Debug.Log("Sending " + EmulatedRoboRIO.RobotInputs);
                            await Task.Delay(LOOP_DELAY); // ms
                        }
                    }
                }
                catch (Exception)
                {
                    senderConnected = false;
                    await Task.Delay(ERROR_DELAY); // ms
                }
            }
            using (var call = client.RobotInputs())
            {
                await call.RequestStream.CompleteAsync();
            }
            senderConnected = false;
            isConnectionOpen = false;
        }

        /// <summary>
        /// Receive robot output data from HEL
        /// </summary>
        private async void ReceiveData()
        {
            var conn = new Grpc.Core.Channel(EmulatorManager.DEFAULT_HOST + ":" + ((EmulatorManager.programType == UserProgram.Type.JAVA) ? DEFAULT_JAVA_PORT : DEFAULT_NATIVE_PORT), Grpc.Core.ChannelCredentials.Insecure);
            var client = new EmulationReader.EmulationReaderClient(conn);
            while (EmulatorManager.IsTryingToRunRobotCode() && Instance && isConnectionOpen) // Run while robot code is running or until the object stops existing
            {
                try
                {
                    using (var call = client.RobotOutputs(new RobotOutputsRequest {
                        Api = API_VERSION,
                        TargetPlatform = EmulatorManager.programType == UserProgram.Type.JAVA ? TargetPlatform.Java : TargetPlatform.Native,
                    }))
                    {
                        while (await call.ResponseStream.MoveNext())
                        {
                            receiverConnected = true;
                            EmulatedRoboRIO.RobotOutputs = call.ResponseStream.Current.OutputData;
                            // Debug.Log("Received " + EmulatedRoboRIO.RobotOutputs);
                        }
                    }
                }
                catch (Exception)
                {
                    receiverConnected = false;
                    await Task.Delay(ERROR_DELAY); // ms
                }
            }
            receiverConnected = false;
            isConnectionOpen = false;
        }

        /// <summary>
        /// Check if the current connection attempt is still running
        /// </summary>
        /// <returns>True if they're running</returns>
        public bool IsConnectionOpen()
        {
            return isConnectionOpen;
        }

        public void OnApplicationQuit()
        {
            EmulatorManager.KillEmulator();
        }

        /// <summary>
        /// Check if both the sender and reciever connections are good
        /// </summary>
        /// <returns>True if they're both connected</returns>
        public bool IsConnected()
        {
            return senderConnected && receiverConnected;
        }
    }
}
