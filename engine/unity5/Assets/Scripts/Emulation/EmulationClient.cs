using System;
using Newtonsoft.Json;
using System.Text;
using System.Net.Sockets;
using System.Threading;
using System.Diagnostics;
using Synthesis.GUI;

public class EmulationClient
{

    private static Thread sender;
    private static Thread receiver;
    private static Process proc;

    private const string ESCAPE_CHARACTER = "\x1B";

    public static bool needsToReconnect { get; set; }

    public EmulationClient(string ip = "127.0.0.1", int sendPort = 11000, int receivedPort = 11001)
    {
        sender = new Thread(new ThreadStart(() => Serialize(ip, sendPort)));
        receiver = new Thread(new ThreadStart(() => Deserialize(ip, receivedPort)));
        ProcessStartInfo startinfo = new ProcessStartInfo();
        startinfo.CreateNoWindow = true;
        startinfo.UseShellExecute = false;
        startinfo.FileName = @"C:\Program Files\qemu\qemu-system-arm.exe";
        startinfo.WindowStyle = ProcessWindowStyle.Hidden;
        startinfo.Arguments = " -machine xilinx-zynq-a9 -cpu cortex-a9 -m 2048 -kernel " + EmulationDriverStation.emulationDir + "zImage" + " -dtb " + EmulationDriverStation.emulationDir + "zynq-zed.dtb" + " -display none -serial null -serial mon:stdio -append \"console=ttyPS0,115200 earlyprintk root=/dev/mmcblk0 rw\" -net user,hostfwd=tcp::10022-:22,hostfwd=tcp::11000-:11000,hostfwd=tcp::11001-:11001,hostfwd=tcp::2354-:2354 -net nic -sd " + EmulationDriverStation.emulationDir + "rootfs.ext4";
        startinfo.Verb = "runas";
        proc = Process.Start(startinfo);
        sender.Start();
        receiver.Start();
    }

    ~EmulationClient()
    {
        UnityEngine.Debug.Log("Killing Threads");
        proc.Kill();
        sender.Abort();
        receiver.Abort();
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
        string rest = "";
        string strJSON = "";
        int retries = 0x10000;
        TcpClient client = null;

        do
        {
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

        retries = 0x10000;
        int count = 0;
        while (true)
        {
            while (true) // Handle packet receival, buffering, and parsing
            {
                int bytesRead = 0;
                byte[] buffer = new byte[client.ReceiveBufferSize];

                string jsonBuffer = "";

                try  // Get new TCP packet
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
                strJSON = rest + jsonBuffer;
                rest = "";

                const string PREAMBLE = "{\"roborio";

                if (strJSON.Length >= PREAMBLE.Length && strJSON.Substring(0, PREAMBLE.Length) != PREAMBLE) // Front of buffer isn't start of JSON packet
                {
                    if (strJSON.IndexOf(ESCAPE_CHARACTER) == -1) // Wait until end of packet is found to clip the front off the buffer
                    {
                        rest = strJSON;
                    }
                    else
                    {
                        rest = strJSON.Substring(strJSON.IndexOf(ESCAPE_CHARACTER) + 1);  // If the start of the buffer isn't the start of a packet, clip the front off
                    }
                }
                else if (strJSON.IndexOf(ESCAPE_CHARACTER) != -1) // Front of packet is preamble and end of packet is present--the whole packet has been received
                {
                    while (strJSON.IndexOf(ESCAPE_CHARACTER) != -1) // Parse all received packets before receiving more--prevents the buffer from getting behind (and we can't discard packets)
                    {
                        rest = strJSON.Substring(strJSON.IndexOf(ESCAPE_CHARACTER) + 1);
                        strJSON = strJSON.Substring(0, strJSON.IndexOf(ESCAPE_CHARACTER));

                        //UnityEngine.Debug.Log(strJSON + "\n\n" + rest);
                        OutputManager.Instance = JsonConvert.DeserializeObject<EmuData>(strJSON);
                        strJSON = rest;
                        System.Threading.Thread.Sleep(5);
                    }
                    break;
                }
                else // Front of packet has been received but not end
                {
                    rest = strJSON;
                }
            }
            System.Threading.Thread.Sleep(30);
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
                    //UnityEngine.Debug.Log(jData.Length);
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