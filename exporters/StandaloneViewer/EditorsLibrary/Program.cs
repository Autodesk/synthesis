using System;
using System.Windows.Forms;


namespace StandaloneViewer
{
    public static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            if (args.Length != 0)
            {
                LaunchParams.Load(args);
                Application.Run(new InventorViewerForm());
            }
            else
            {
                LaunchParams.Defaults();
                Application.Run(new StandaloneViewerForm());
            }
        }
    }
}