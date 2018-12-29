using System;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.IO;
using System.Configuration;
using System.IO.Compression;
using System.Threading;

public class SSHClient
{
    //@"C:\Program Files (x86)\Autodesk\Synthesis"


    public static void SCPFileSender()
    {
        try
        {
            //choofdlog.Multiselect = true;
            string sFileName = SFB.StandaloneFileBrowser.OpenFilePanel("Robot Code", "C:\\", "", false)[0];
            UnityEngine.Debug.Log(sFileName);
            using (ScpClient client = new ScpClient("127.0.0.1", 10022, "lvuser", ""))
            {
                client.Connect();
                using (Stream localFile = File.OpenRead(sFileName))
                {
                    client.Upload(localFile, @"/home/lvuser/" + sFileName.Substring(sFileName.LastIndexOf('\\') + 1));
                }
            }
        }

        catch (Exception ex)
        {
        }
    }

    private static bool VMConnected = false; // Last connection status
    private static Thread TestVMConnectionThread = new Thread(() =>
    {
        while (true)
        {
            try
            {
                using (SshClient client = new SshClient("127.0.0.1", 10022, "lvuser", ""))
                {
                    client.Connect();
                    VMConnected = client.IsConnected;
                    UnityEngine.Debug.Log("Connection status:  " + VMConnected);
                    client.Disconnect();
                }
            }
            catch
            {
                VMConnected = false;
            }
            if (VMConnected) // Sleep longer if connected since it's less vital to check for disconnects
            {
                Thread.Sleep(15000); // ms
            }
            else
            {
                Thread.Sleep(3000); // ms
            }
        }
    });

    public static bool IsVMConnected()
    {
        if (!TestVMConnectionThread.IsAlive)
        {
            TestVMConnectionThread.Start();
        }
        return VMConnected;
    }

    public static void StopRobotCode()
    {
        new Thread(() =>
        {
            using (SshClient client = new SshClient("127.0.0.1", 10022, "lvuser", ""))
            {
                client.Connect();
                client.RunCommand("sudo killall frc_program_chooser.sh >/dev/null 2>&1; sudo killall java >/dev/null 2>&1; sudo killall FRCUserProgram >/dev/null 2>&1;");
                client.Disconnect();
            }
        }).Start();
    }

    public static void StartRobotCode()
    {
        new Thread(() =>
        {
            using (SshClient client = new SshClient("127.0.0.1", 10022, "lvuser", ""))
            {
                client.Connect();
                client.RunCommand("sudo killall frc_program_chooser.sh >/dev/null 2>&1; sudo killall java >/dev/null 2>&1; sudo killall FRCUserProgram >/dev/null 2>&1; nohup /home/lvuser/frc_program_chooser.sh </dev/null >/dev/null 2>&1 &");
                client.Disconnect();
            }
        }).Start();
    }
}
