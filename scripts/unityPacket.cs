using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
/*
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
		
		public void read(byte[] pack)
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
	public volatile bool stillStupid = true;
	UdpClient udp;
	Thread threadRecieve, threadSend;
	OutputStatePacket packet = new OutputStatePacket();
	
	public void Start()
	{
	
	
		active = true;
		threadRecieve = threadSend = new Thread(ServerInternal);
		
     
		threadRecieve.Start(delegate(UdpClient udp,byte[] buffer)
		{
		
		});
		threadSend.Start(delegate(UdpClient udp, byte[] buffer){
		
		});
		
		//this udp thread was really really stupid || this is because of how dumb Unity is
		if (stillStupid)
		{
			Stop();
			Start();	
		}
	}

	public void Stop()
	{
		try
		{
			Debug.Log("Stop...");
			active = false;
			threadSend.Join();
			threadRecieve.Join();
			
			try
			{
				udp.Close();
				Debug.Log("UDP Server Shutdown Cleanly... ");    
			} catch (Exception ex)
			{
				Debug.Log(ex.Source + ": " + ex.Message + ": " + ex.StackTrace.ToString());
			}
			
		} catch (Exception ex)
		{
			Debug.Log(ex + ": " + ex.Message + ": " + ex.StackTrace.ToString());
		}
	}

	private void ServerInternal(Action<UdpClient, byte[]> networkingBehaviour)
	{
		
		try
		{	
			udp = new UdpClient();
			
			Debug.Log("Server...");	
			
			IPEndPoint ipEnd = new IPEndPoint(IPAddress.Loopback, 2550);
			udp.ExclusiveAddressUse = false;
			udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			if (stillStupid)
			{
				stillStupid = false;
			} else
			{
				udp.Client.Bind(ipEnd);
			}
			byte[] buffer = new byte[1024];
			while (active)
			{	
				if (udp.Available <= 0)
				{
					Thread.Sleep(20);
					continue;
				}
                
				buffer = udp.Receive(ref ipEnd);
										
				packet.read(buffer);	
				
				//int portFromInvAPI = 18;
				//packet.dio[(portFromInvAPI >> 4) & 0xF].pwmValues[portFromInvAPI & 0xF]
			}
            
		} catch (Exception ex)
		{
			Debug.Log(ex + ": " + ex.Message + ": " + ex.StackTrace.ToString());
		}
	}

	public OutputStatePacket GetPacket()
	{
		return packet;
	}

}
	/*
public class UnityClient
{	
	private void ClientInternal()
	{
		try
		{
			Debug.Log("Client...");
			IPEndPoint ipEnd = new IPEndPoint(IPAddress.Loopback, 2551);
			udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			
			udp = new UdpClient();
			udp.Client.Bind(ipEnd);
		
		
			byte[] buffer = new byte[1024];
			while (active)
			{
				//buffer = packet.Write(buffer);
				
				udp.Send(buffer, buffer.Length);
			
			}
		} catch (Exception ex)
		{
			Debug.Log(ex + ": " + ex.Message + ": " + ex.StackTrace.ToString());
		}
		
			
	}
		*/


	

		







