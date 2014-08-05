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
	UdpClient udp;
	Thread threadRecieve, threadSend;
	OutputStatePacket packetRecieve = new OutputStatePacket();
	InputStatePacket packetSend = new InputStatePacket();
	
	public void Start()
	{
	
	
		active = true;
		
		threadRecieve = new Thread(RunServerWrapper);
		threadSend = new Thread(RunClientWrapper);	
		
		threadRecieve.Start(this);
		
		threadSend.Start(this);
		
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
		}
	}

	private void ServerInternal()
	{
		try
		{	
			udp = new UdpClient();
			
			IPEndPoint ipEnd = new IPEndPoint(IPAddress.Loopback, 2550);
			udp.ExclusiveAddressUse = false;
			udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			if (stillRecieve)
			{
				stillRecieve = false;
			} else
			{
				udp.Client.Bind(ipEnd);
			}
			byte[] buffer = new byte[1024];
			while (active)
			{	
			/*
				if (udp.Available <= 0)
				{
					//Debug.Log("Server...");
					Thread.Sleep(20);
					continue;
				}
				*/
				buffer = udp.Receive(ref ipEnd);
				packetRecieve.Read(buffer);
			}
				
			//int portFromInvAPI = 18;
			//packet.dio[(portFromInvAPI >> 4) & 0xF].pwmValues[portFromInvAPI & 0xF]
		} catch (Exception ex)
		{
			Debug.Log(ex + ": " + ex.Message + ": " + ex.StackTrace.ToString());
		} finally
		{
			udp.Close();
		}
	}

	private void ClientInternal()
	{
		try
		{	
			udp = new UdpClient();
			IPEndPoint ipEnd = new IPEndPoint(IPAddress.Loopback, 2551);
	
			byte[] buffer = new byte[1024];
			while (active)
			{	
				buffer = packetSend.Write(buffer);
				udp.Client.SendTo(buffer, ipEnd);
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
				udp.Close();
			} catch (Exception ex)
			{
				Debug.Log(ex + ": " + ex.Message + ": " + ex.StackTrace.ToString());
			}
		}
	}
	
	private static void RunServerWrapper(object obj)
	{
		Debug.Log("Server...");
		((unityPacket)obj).ServerInternal();
		
	}

	private static void RunClientWrapper(object obj)
	{
		Debug.Log("Client...");
		((unityPacket)obj).ClientInternal();
		
	}
}
	

	

		







