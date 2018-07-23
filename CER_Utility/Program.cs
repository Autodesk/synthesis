using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CER_Utility
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.ThreadException += (sender, args) => SendReport(args.Exception);
            AppDomain.CurrentDomain.UnhandledException += (sender, args) =>
            {
                SendReport((Exception)args.ExceptionObject);
                Environment.Exit(0);
                //ReportMe((Exception)args.ExceptionObject);
            };

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new CrashMe());
        }

        public static void SendReport(Exception xception)
        {
            //Application.Run(new CrashReport());
            CrashReport reporterPro = new CrashReport();
            //reporterPro.Show();
            reporterPro.ShowDialog();
            reporterPro.ReportMe(xception);
        }
    }
}
