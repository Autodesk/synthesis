using FieldExporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

static class Program
{
    public static Inventor.Application INVENTOR_APPLICATION;

    public static ProgressWindow progressWindow;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        try
        {
            INVENTOR_APPLICATION = (Inventor.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application");
        }
        catch
        {
            MessageBox.Show("Please launch Autodesk Inventor and try again.", "Could not connect to Autodesk Inventor.");
            return;
        }

        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        Application.Run(new MainWindow());
    }
}
