using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class unityPacket
{
	
	public class OutputStatePacket
	{
		public DIOModule[] dio = new DIOModule[2];
		public SolenoidModule[] solenoid = new SolenoidModule[1];
		public class DIOModule
		{
			public const int LENGTH = 12 + (10 * 4);
			public UInt32 relayForward;
			public UInt32 relayReverse;
			public UInt32 digitalOutput;
			public float[] pwmValues = new float[10];
		}

		public class SolenoidModule
		{
			public const int LENGTH = 1;
			public byte state;
		} 
		
		public void Read(byte[] pack)
		{
			
			for (int i = 0; i < dio.Length; i++)
			{
				int offset = i * DIOModule.LENGTH;
				dio [i] = new DIOModule();
				dio [i].relayForward = BitConverter.ToUInt32(pack, offset);
				dio [i].relayReverse = BitConverter.ToUInt32(pack, offset + 4);
				dio [i].digitalOutput = BitConverter.ToUInt32(pack, offset + 8);
				for (int j = 0; j < dio[i].pwmValues.Length; j++)
				{
					dio [i].pwmValues [j] = BitConverter.ToSingle(pack, offset + 12 + (4 * j));
				}
			}
			for (int i = 0; i < solenoid.Length; i++)
			{
				int offset = (DIOModule.LENGTH * dio.Length) + (i * SolenoidModule.LENGTH);
				solenoid [i] = new SolenoidModule();
				solenoid [i].state = pack [offset];
			}
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
		//threadSend = new Thread(ClientInternal);
			
		Debug.Log("Server...");
		threadRecieve.Start();
		Debug.Log("Client...");
		threadSend.Start();
		
		//this udp thread was really really stupid || this is because of how dumb Unity is
		if (stillSend && stillRecieve && (threadSend.IsAlive && threadRecieve.IsAlive))
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
			Debug.Log("Stop...");
			
		} catch (Exception ex)
		{
			Debug.Log(ex + ": " + ex.Message + ": " + ex.StackTrace.ToString());
		} finally
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
			if (stillRecieve)
			{
				stillRecieve = false;
			} else
			{
				server.Client.Bind(ipEnd);
			}
			//byte[] buffer = new byte[1024];
			while (active)
			{	
				serverMutex.WaitOne();
				receiveBuffer = server.Receive(ref ipEnd);
				//packetRecieve = new OutputStatePacket();
				//packetRecieve.Read(buffer);
				serverMutex.ReleaseMutex();
			}
				
			//int portFromInvAPI = 18;
			//packet.dio[(portFromInvAPI >> 4) & 0xF].pwmValues[portFromInvAPI & 0xF]
		} catch (Exception ex)
		{
			Debug.Log(ex + ": " + ex.Message + ": " + ex.StackTrace.ToString());
		} finally
		{
			try
			{				
				server.Close();
			} catch (Exception ex)
			{
				Debug.Log(ex + ": " + ex.Message + ": " + ex.StackTrace.ToString());
			}
		}
	}
/*
	private void ClientInternal()
	{
		try
		{	
			client = new UdpClient();
			IPEndPoint ipEnd = new IPEndPoint(IPAddress.Loopback, 2551);
	
			while (active)
			{	
				if (sendBufferLen > sendBuffer.Length)
				{
				clientMutex.WaitOne();
				client.Client.SendTo(sendBuffer, sendBufferLen, ipEnd);
				clientMutex.ReleaseMutex();
				}
			}
			
		} catch (Exception ex)
		{
			Debug.Log(ex + ": " + ex.Message + ": " + ex.StackTrace.ToString());
		} finally
		{
			try
			{				
				client.Close();
			} catch (Exception ex)
			{
				Debug.Log(ex + ": " + ex.Message + ": " + ex.StackTrace.ToString());
			}
		}
	}
	*/
	public OutputStatePacket getLastPacket()
	{
		OutputStatePacket pack = new OutputStatePacket();
		serverMutex.WaitOne();
		pack.Read(receiveBuffer);
		serverMutex.ReleaseMutex();
		return pack;
	}
	public void sendLastPacket(InputStatePacket pack)
	{
		clientMutex.WaitOne();
		sendBufferLen = pack.Write(sendBuffer);
		clientMutex.ReleaseMutex();
	}
	
}