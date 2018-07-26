using System;
using System.IO;
using System.Windows.Forms;
using System.Diagnostics;
using System.Management;
using Microsoft.Win32;

namespace CER_Utility
{
    public partial class CrashReport : Form
    {
        public string errorDetails;
        public string osDetails;
        public string userDetails;
        public bool DontSend = true;

        public CrashReport()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            //XceptionData xception = (sender as XceptionData);
            //ReportMe((Exception)sender.ExceptionObject);
            DontSend = false;
            Application.Exit();
        }

        public void ReportMe(Exception xception)
        {
            string test1 = default(string);
            string test2 = default(string);
            string test3 = default(string);
            string gpu = default(string);
            string cpu = "CPU Model: ";
            string clr = "CLR Version: " + System.Runtime.InteropServices.RuntimeEnvironment.GetSystemVersion();
            string space = Environment.NewLine;

            ManagementObjectSearcher mos = new ManagementObjectSearcher("select * from Win32_OperatingSystem");
            foreach (ManagementObject managementObject in mos.Get())
            {
                if (managementObject["Caption"] != null)
                {
                    test1 = "Operating System Name  :  " + managementObject["Caption"].ToString();
                }
                if (managementObject["OSArchitecture"] != null)
                {
                    test2 = "Operating System Architecture  :  " + managementObject["OSArchitecture"].ToString();
                }
                if (managementObject["CSDVersion"] != null)
                {
                    test3 = "Operating System Service Pack   :  " + managementObject["CSDVersion"].ToString() + space;
                }
            }

            ManagementObjectSearcher myVideoObject = new ManagementObjectSearcher("select * from Win32_VideoController");

            foreach (ManagementObject obj in myVideoObject.Get())
            {
                gpu = "GPU Model: " + obj["Name"] + space;
            }

            RegistryKey processor_name = Registry.LocalMachine.OpenSubKey(@"Hardware\Description\System\CentralProcessor\0", RegistryKeyPermissionCheck.ReadSubTree);   //This registry entry contains entry for processor info.
            string test4 = (string)processor_name.GetValue("ProcessorNameString");

            string divider = "----------SOMEBODY SOMEWHERE BROKE SYNTHESIS----------" + space;
            osDetails = space + space + test1 + space + test2 + space + test3 + space + clr + space + space + cpu + test4 + space + gpu + space;
            errorDetails = "Exception Message: " + xception.Message + space + "Exception Data: " + xception.Data + space + "Exception Source: " + xception.Source + space + space + "Stack Trace:" + "```" + xception.StackTrace + "```";
            userDetails = space + "User Email: " + textBox1.Text + space + "User Error Details: " + textBox2.Text + space;
            File.WriteAllText(@"Synthesis Crash Report.txt", errorDetails + space + osDetails + space + userDetails);

            if (DontSend == false)
            {
                try
                {
                    //ulong toalRam = cinfo.TotalPhysicalMemory;
                    //double toal = Convert.ToDouble(toalRam / (1024 * 1024));
                    //int t = Convert.ToInt32(Math.Ceiling(toal / 1024).ToString());
                    //label6.Text = t.ToString() + " GB";// ram detail

                    string urlWithAccessToken = "INSERT WEBHOOK LINK HERE";

                    SlackClient client = new SlackClient(urlWithAccessToken);

                    client.PostMessage(username: "Synthesis Crash Reporter",
                               text: divider + errorDetails + osDetails + userDetails,
                               channel: "#failures");
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                Environment.Exit(0);
            }
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://errorreport.autodesk.com/whatHappens.jsp?language=1033");
        }

        private void linkLabel2_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://www.autodesk.com/company/legal-notices-trademarks/privacy-statement");
        }

        private void button2_Click(object sender, EventArgs e)
        {
            string dir = Directory.GetCurrentDirectory().ToString();
            System.Timers.Timer timer = new System.Timers.Timer(10);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
            try
            {
                Process.Start(@"Synthesis Crash Report.txt");
            }
            catch
            {
                MessageBox.Show("Failed To Open: Synthesis Crash Report.txt" + Environment.NewLine + "Check " + dir);
            }
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            Application.Exit();
        }

        private void linkLabel3_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/Autodesk/synthesis/wiki");
        }
    }
}