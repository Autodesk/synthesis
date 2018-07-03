using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using UnityEngine;

namespace Synthesis.StatePacket
{
    public class UnityPacket
    {
        public class OutputStatePacket
        {
            public class DIOModule
            {
                public const int LENGTH = sizeof(uint) * 3 + (10 * sizeof(float)) + (32 * sizeof(float));
                public uint relayForward;
                public uint relayReverse;
                public uint digitalOutput;
                public float[] pwmValues = new float[10];
                public float[] canValues = new float[10];
            }

            public class SolenoidModule
            {
                public const int LENGTH = 1;
                public byte state;
            }

            public DIOModule[] dio = new DIOModule[2];
            public SolenoidModule solenoid = new SolenoidModule();

            public void Read(byte[] pack)
            {
                for (int i = 0; i < dio.Length; i++)
                {
                    int offset = i * DIOModule.LENGTH;
                    dio[i] = new DIOModule();
                    dio[i].relayForward = BitConverter.ToUInt32(pack, offset);
                    dio[i].relayReverse = BitConverter.ToUInt32(pack, offset + 4);
                    dio[i].digitalOutput = BitConverter.ToUInt32(pack, offset + 8);

                    for (int j = 0; j < dio[i].pwmValues.Length; j++)
                        dio[i].pwmValues[j] = BitConverter.ToSingle(pack, offset + 12 + (4 * j));
                }

                int totalOffset = DIOModule.LENGTH * dio.Length + SolenoidModule.LENGTH;
                solenoid = new SolenoidModule();
                solenoid.state = pack[totalOffset];
            }
        }

        public volatile bool active;
        public volatile bool stillSend = true;
        public volatile bool stillRecieve = true;
        Thread threadRecieve, threadSend;
        UdpClient client, server;
        private byte[] receiveBuffer = new byte[1024];
        private byte[] sendBuffer = new byte[1024];
        private int sendBufferLen = 0;
        private Mutex clientMutex, serverMutex;

        public void Start()
        {
            clientMutex = new Mutex();
            serverMutex = new Mutex();

            active = true;

            threadRecieve = new Thread(ServerInternal);
            threadSend = new Thread(ClientInternal);

            threadRecieve.Start();
            threadSend.Start();

            Debug.Log("UnityPacket Initialized...");
            //this udp thread was really really stupid || this is because of how dumb Unity is
            if (stillRecieve && (threadSend.IsAlive && threadRecieve.IsAlive))
            {
                Stop();
                Start();
            }
        }

        public void Stop()
        {
            try
            {
                active = false;
                threadSend.Join();
                threadRecieve.Join();
                Debug.Log("Stopping UnityPacket...");
            }
            catch (Exception ex)
            {
                Debug.Log(ex + ": " + ex.Message + ": " + ex.StackTrace.ToString());
            }
            finally
            {
                serverMutex.Close();
                clientMutex.Close();
            }
        }

        private void ServerInternal()
        {
            try
            {
                server = new UdpClient();

                IPEndPoint ipEnd = new IPEndPoint(IPAddress.Loopback, 2550);
                server.ExclusiveAddressUse = false;
                server.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                Debug.Log("Still recieve is " + (stillRecieve + ".").ToLower());
                if (stillRecieve)
                {
                    stillRecieve = false;
                }
                else
                {
                    Debug.Log("Binding...");
                    server.Client.Bind(ipEnd);
                }
                while (active)
                {
                    //Debug.Log(server.Available);
                    if (server.Available <= 0)
                    {
                        Thread.Sleep(20);
                        continue;
                    }

                    byte[] temp = server.Receive(ref ipEnd);
                    //Debug.LogError("Packet len: " + temp.Length);
                    serverMutex.WaitOne();
                    receiveBuffer = temp;
                    serverMutex.ReleaseMutex();
                }

                //int portFromInvAPI = 18;
                //packet.dio[(portFromInvAPI >> 4) & 0xF].pwmValues[portFromInvAPI & 0xF]
            }
            catch (Exception ex)
            {
                Debug.Log(ex + ": " + ex.Message + ": " + ex.StackTrace.ToString());
            }
            finally
            {
                try
                {
                    server.Close();
                }
                catch (Exception ex)
                {
                    Debug.Log(ex + ": " + ex.Message + ": " + ex.StackTrace.ToString());
                }
            }
        }

        private void ClientInternal()
        {
            try
            {
                client = new UdpClient();
                IPEndPoint ipEnd = new IPEndPoint(IPAddress.Loopback, 2551);

                while (active)
                {
                    if (sendBufferLen > 0)
                    {
                        clientMutex.WaitOne();
                        client.Client.SendTo(sendBuffer, sendBufferLen, SocketFlags.None, ipEnd);
                        clientMutex.ReleaseMutex();
                    }
                    Thread.Sleep(20);
                }

            }
            catch (Exception ex)
            {
                Debug.Log(ex + ": " + ex.Message + ": " + ex.StackTrace.ToString());
            }
            finally
            {
                try
                {
                    client.Close();
                }
                catch (Exception ex)
                {
                    Debug.Log(ex + ": " + ex.Message + ": " + ex.StackTrace.ToString());
                }
            }
        }

        public OutputStatePacket GetLastPacket()
        {
            OutputStatePacket pack = new OutputStatePacket();
            serverMutex.WaitOne();
            pack.Read(receiveBuffer);
            serverMutex.ReleaseMutex();
            return pack;
        }

        public void WritePacket(InputStatePacket input)
        {
            clientMutex.WaitOne();
            sendBufferLen = input.Write(sendBuffer);
            clientMutex.ReleaseMutex();
        }
    }
}