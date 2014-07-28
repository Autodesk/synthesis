using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class StatePacket
{
	public float[] pwmValues = new float[8];
	public float[] canMotorValues = new float[16];
	public byte solenoidValues;
	public byte relayValues;
};

public class unityPacket
{
	public Thread thread;
	public volatile bool active = true;
	UdpClient udp;
	StatePacket packet = new StatePacket();
	
	public void Start()
	{
		thread = new Thread(unityPacket.RunServerWrapper);
		udp = new UdpClient();
        active = true;
		/*
		udp.Client.DontFragment = true;
		udp.Client.Ttl = 100;
		udp.Client.SendTimeout = 1;
		*/
		thread.Start(this);
		
	}

	public void Stop()
	{
		try
		{
			Debug.Log("Stop...");
			active = false;
			thread.Join();
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

	private void ServerInternal()
	{
		try
		{
			Debug.Log("Server...");	
			
			IPEndPoint ipEnd = new IPEndPoint(IPAddress.Loopback, 2550);
			udp.ExclusiveAddressUse = false;
            udp.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udp.Client.Bind(ipEnd);
			byte[] buffer = new byte[1024];
			while (active)
			{	
				
				if (udp.Available <= 0)
				{
					Thread.Sleep(20);
					//Debug.Log("Hey baby...");
					continue;
				}
				
								
				buffer = udp.Receive(ref ipEnd);
								
								
			
				//Debug.Log("I hate the world");
				for (int i = 0; i < 8; i++)
				{
					packet.pwmValues [i] = System.BitConverter.ToSingle(buffer, i * 4);
					Debug.Log(packet.pwmValues [i]);
				}
			}
		} catch (Exception ex)
		{
			Debug.Log(ex + ": " + ex.Message + ": " + ex.StackTrace.ToString());
		}
	}

	public StatePacket GetPacket()
	{
		return packet;
	}

	private static void RunServerWrapper(object obj)
	{
		((unityPacket)obj).ServerInternal();
	}

		


}




