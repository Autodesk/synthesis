using System;
using Newtonsoft.Json;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.Threading;
using System.Diagnostics;

public class Serialization
{

    private static Thread sender;
    private static Thread receiver;

    public static bool needsToReconnect { get; set; }

    public Serialization(string ip = "127.0.0.1", int sendPort = 11000, int receivedPort = 11001)
    {
        sender = new Thread(new ThreadStart(() => Serialize(ip, sendPort)));
        receiver = new Thread(new ThreadStart(() => Deserialize(ip, receivedPort)));
        sender.Start();
        receiver.Start();
    }

    public static void RestartThreads(string ip = "127.0.0.1", int sendPort = 11000, int receivePort = 11001)
    {
        sender.Abort();
        receiver.Abort();
        sender = new Thread(new ThreadStart(() => Serialize(ip, sendPort)));
        receiver = new Thread(new ThreadStart(() => Deserialize(ip, receivePort)));
        sender.Start();
        receiver.Start();
    }

    public static void Deserialize(string ip = "127.0.0.1", int port = 11001)
    {
        EmuData emu;
        string rest = "";
        string strJSON = "";
        int retries = 65536;
        TcpClient client = null;
        bool kill = false;

        do
        {
            if (kill)
                return;
            try
            {
                UnityEngine.Debug.Log("Attempting to connect to remote host " + ip + " over port " + port);
                client = new TcpClient(ip, port);
            }
            catch (SocketException e)
            {
                OutputManager.Instance = new EmuData();
                if (retries != 0)
                {
                    UnityEngine.Debug.Log("Connection failed to establish to remote host "+ ip +". " + retries + " retries remaining. Retrying in 5 seconds");
                    System.Threading.Thread.Sleep(5000);

                }
                else
                {
                    UnityEngine.Debug.Log("Failed to connect to remote host " + ip );
                    //return;
                    throw new SocketException();
                }
                retries--;
            }
        } while (client == null);

        UnityEngine.Debug.Log("Connection successfully established to " + ip);
        NetworkStream nwStream = client.GetStream();

        retries = 65536;
        int count = 0;
        while (true)
        {
            while (true)
            {
                int bytesRead = 0;
                byte[] buffer = new byte[client.ReceiveBufferSize];

                string jsonBuffer = "";

                try
                {
                    bytesRead = nwStream.Read(buffer, 0, client.ReceiveBufferSize);
                    jsonBuffer = Encoding.ASCII.GetString(buffer, 0, bytesRead);
                    if (jsonBuffer.Length == 0)
                    {
                        Console.WriteLine("Failed to receive data. Sleeping for 5 seconds.\n");
                        count++;
                        throw new SocketException();
                    }
                }
                catch (SocketException se)
                {
                    TcpClient newClient = null;
                    OutputManager.Instance = new EmuData();
                    do
                    {
                        try
                        {
                            UnityEngine.Debug.Log("Attempting to connect to remote host " + ip + " over port " + port);
                            newClient = new TcpClient(ip, port);
                        }
                        catch (SocketException e)
                        {
                            if (retries != 0)
                            {
                                UnityEngine.Debug.Log("Connection failed to establish to remote host " + ip + ". " + retries + " retries remaining. Retrying in 5 seconds");
                                System.Threading.Thread.Sleep(5000);

                            }
                            else
                            {
                                UnityEngine.Debug.Log("Failed to connect to remote host" + ip);
                                //return;
                                throw new SocketException();
                            }
                            retries--;
                        }
                    } while (newClient == null);
                    UnityEngine.Debug.Log("Connection successfully re-established. Waiting for socket to initialize");
                    nwStream = newClient.GetStream();
                    continue;
                }
                strJSON = rest;
                rest = "";
                if (strJSON.IndexOf("\x1B") != -1)
                {
                    rest = strJSON.Substring(strJSON.IndexOf("\x1B") + 1);
                    strJSON = strJSON.Substring(0, strJSON.IndexOf("\x1B"));
                    break;
                }
                strJSON += jsonBuffer;

                if (strJSON.Length >= 9 && strJSON.Substring(0, 9) != "{\"roborio")
                {
                    if (strJSON.IndexOf("\x1B") == -1)
                        rest = strJSON;
                    else
                        rest = strJSON.Substring(strJSON.IndexOf("\x1B") + 1);
                    continue;
                }
                else if (strJSON.Length >= 9)
                {
                    rest = strJSON;
                    continue;
                }

                if (strJSON.IndexOf("\x1B") == -1)
                {
                    rest = strJSON;
                    continue;
                }
                rest = strJSON.Substring(strJSON.IndexOf("\x1B") + 1);
                strJSON = strJSON.Substring(0, strJSON.IndexOf("\x1B"));
                break;

            }
            if (strJSON != "")
            {
                OutputManager.Instance = JsonConvert.DeserializeObject<EmuData>(strJSON);
                strJSON = "";
                System.Threading.Thread.Sleep(30);
            }
        }
    }

    public static void Serialize(string ip = "127.0.0.1", int port = 11000)
    {
        int retries = 5;
        bool kill = false;
        TcpClient client = null;
        string jData;
        //TryReconnectSend = false;

        do
        {
            if (kill)
                return;
            try
            {
                UnityEngine.Debug.Log("Attempting to connect to remote host " + ip + " over port " + port);
                client = new TcpClient(ip, port);
            }
            catch (SocketException e)
            {
                
                if (retries != 0)
                {
                    UnityEngine.Debug.Log("Connection failed to establish to remote host " + ip + retries + " retries remaining. Retrying in 5 seconds");
                    System.Threading.Thread.Sleep(5000);

                }
                else
                {
                    UnityEngine.Debug.Log("Failed to connect to remote host " + ip);
                    //return;
                    throw new SocketException();
                }
                retries--;
            }
        } while (client == null);
        UnityEngine.Debug.Log("Connection successfully established to " + ip);
        NetworkStream nwStream = client.GetStream();

        while (true)
        {
            try
            {
                EngineData instance = InputManager.Instance;
                instance.Roborio.RobotMode.updateRobotMode();
                jData = JsonConvert.SerializeObject(instance);
                if (jData != "")
                {
                    if (needsToReconnect)
                        throw new SocketException();
                    byte[] bytesToSend = ASCIIEncoding.ASCII.GetBytes(jData + '\x1B');

                    Console.WriteLine("Sending : " + jData);
                    UnityEngine.Debug.Log(jData.Length);
                    nwStream.Write(bytesToSend, 0, bytesToSend.Length);
                    System.Threading.Thread.Sleep(30);
                }
            }
            catch
            {
                //while (!TryReconnectSend)
                {
                    Thread.Sleep(1000);
                }
                TcpClient newClient = null;
                do
                {
                    try
                    {
                        UnityEngine.Debug.Log("Attempting to connect to remote host " + ip + " over port " + port);
                        newClient = new TcpClient(ip, port);
                    }
                    catch (SocketException e)
                    {
                        InputManager.Instance = new EngineData();
                        if (retries != 0)
                        {
                            UnityEngine.Debug.Log("Connection failed to establish to remote host " + ip + ". " + retries + " retries remaining. Retrying in 5 seconds");
                            System.Threading.Thread.Sleep(5000);

                        }
                        else
                        {
                            UnityEngine.Debug.Log("Failed to connect to remote host" + ip);
                            return;
                            //throw new SocketException();
                        }
                        retries--;
                    }
                } while (newClient == null);
                UnityEngine.Debug.Log("Connection successfully re-established. Waiting for socket to initialize");
                nwStream = newClient.GetStream();
                needsToReconnect = false;
                continue;
            }
        }
    }

    public static double getPWM(int index)
    {
        var instance = OutputManager.Instance;

        if (index > instance.Roborio.PwmHdrs.Length)
            throw new IndexOutOfRangeException();
        return instance.Roborio.PwmHdrs[index];

    }

    public static double getCAN(int index)
    {
        var instance = OutputManager.Instance;

        if (index > 63)
            throw new IndexOutOfRangeException();
        foreach (var CAN in instance.Roborio.CANDevices)
            if (CAN.id == index)
                return CAN.speed;
        return 0.0f;
    }

    public static void updateJoystick(int index, double[] axes, bool[] buttons, double[] povs)
    {
        var instance = InputManager.Instance;

        if (index > instance.Roborio.Joysticks.Length)
            throw new IndexOutOfRangeException();
        instance.Roborio.Joysticks[index].updateJoystick(axes, buttons, povs);
    }
}