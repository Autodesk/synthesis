using FieldExporter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

static class Program
{
    /// <summary>
    /// The global Inventor application instance.
    /// </summary>
    public static Inventor.Application INVENTOR_APPLICATION;

    /// <summary>
    /// The global MainWindow instance.
    /// </summary>
    public static MainWindow mainWindow;

    /// <summary>
    /// The global ProgressWindow instance.
    /// </summary>
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
        Application.Run(mainWindow = new MainWindow());
    }
}
