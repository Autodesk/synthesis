using System;
using Renci.SshNet;
using Renci.SshNet.Sftp;
using System.IO;
using System.Configuration;
using System.IO.Compression;

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
            using (ScpClient client = new ScpClient("127.0.0.1", 10022, "synthesis", "adskbxd"))
            {
                client.Connect();
                using (Stream localFile = File.OpenRead(sFileName))
                {
                    client.Upload(localFile, @"/home/synthesis/" + sFileName);
                }
            }
        }

        catch (Exception ex)
        {
        }
    }

    public static void StartRobotCode()
    {
        using (SshClient client = new SshClient("127.0.0.1", 10022, "synthesis", "adskbxd"))
        {
            client.Connect();
            client.RunCommand("/home/lvuser/frc_program_chooser.sh");
        }
    }
    public static void RestartRobotCode()
    {
        StartRobotCode();
    }

}