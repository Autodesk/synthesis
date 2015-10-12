using FieldExporter;
using FieldExporter.Components;
using FieldExporter.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

static class Program
{
    /*
     * TODO:
     * 1. Invest in using a non-binary file type (something like XML).
     *    This will be better for backwards compatibility and stability, etc. (Done)
     * 2. Consider using an Inventor.ApprenticeServer instead of directly referencing the
     *    Inventor application. Right now, there is a lot of code for simply making sure
     *    the application is constantly connected to Inventor and preventing the garbage
     *    collector from abusing us. However, an ApprenticeServer allows us to reference
     *    an AssemblyDocument without needing Inventor to be open. Plus, the Inventor API
     *    has a way to implement the Inventor viewer as a .NET component into the application
     *    very easily (we should have discovered this sooner). There are several advantages to this:
     *      A. We don't have to worry about accidentally disconnecting from the assembly/application (minimal runtime errors).
     *      B. We don't need the user to have Inventor open AT ALL.
     *      C. Communication with the assembly is much faster (meaning better export times).
     *      D. We don't need to implment a custom OpenGL viewer like the robot exporter does.
     */

    /// <summary>
    /// The global Inventor application instance.
    /// </summary>
    public static Inventor.Application INVENTOR_APPLICATION;
    
    /// <summary>
    /// The global assembly document.
    /// </summary>
    public static Inventor.AssemblyDocument ASSEMBLY_DOCUMENT;

    /// <summary>
    /// The global MainWindow instance.
    /// </summary>
    public static MainWindow MAINWINDOW;

    /// <summary>
    /// Used for distinguishing between the first assembly document open and another one opened later.
    /// </summary>
    private static string fullDocumentName = "undef";

    /// <summary>
    /// Used for determining if a reconnection has been requested.
    /// </summary>
    private static bool connectionRequested = false;

    /// <summary>
    /// Locks the Inventor UI loop to help a process run faster.
    /// </summary>
    /// <param name="process"></param>
    public static void LockInventor()
    {
        INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = true;
    }

    /// <summary>
    /// Unlocks the Inventor UI loop to resume interaction with Inventor.
    /// </summary>
    public static void UnlockInventor()
    {
        INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = false;
        Connect();
    }

    /// <summary>
    /// Attempts to connect to an open Inventor Application instance and the active assembly document.
    /// </summary>
    /// <returns>true if successful, false if failed</returns>
    private static bool Connect()
    {
        try
        {
            INVENTOR_APPLICATION = (Inventor.Application)System.Runtime.InteropServices.Marshal.GetActiveObject("Inventor.Application");
            INVENTOR_APPLICATION.ApplicationEvents.OnQuit += ApplicationEvents_OnQuit;
            INVENTOR_APPLICATION.ApplicationEvents.OnCloseDocument += ApplicationEvents_OnCloseDocument;
        }
        catch
        {
            return false;
        }

        ASSEMBLY_DOCUMENT = null;

        if (fullDocumentName.Equals("undef"))
        {
            if (INVENTOR_APPLICATION.Documents.VisibleDocuments.Count > 0)
            {
                FieldSelectForm fieldSelector = new FieldSelectForm();
                if (fieldSelector.ShowDialog() == DialogResult.OK)
                {
                    foreach (Inventor.Document doc in INVENTOR_APPLICATION.Documents.VisibleDocuments)
                    {
                        if (doc.DisplayName == fieldSelector.SelectedField)
                            ASSEMBLY_DOCUMENT = (Inventor.AssemblyDocument)doc;
                    }

                    if (ASSEMBLY_DOCUMENT == null)
                        return false;

                    fullDocumentName = ASSEMBLY_DOCUMENT.FullDocumentName;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        else
        {
            foreach (Inventor.Document doc in INVENTOR_APPLICATION.Documents.VisibleDocuments)
            {
                if (doc.FullDocumentName == fullDocumentName)
                    ASSEMBLY_DOCUMENT = (Inventor.AssemblyDocument)doc;
            }

            if (ASSEMBLY_DOCUMENT == null)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Shows a MessageBox prompting the user to either retry the Assembly connection or cancel.
    /// </summary>
    /// <returns></returns>
    private static void RequestConnection()
    {
        if (connectionRequested)
            return;

        connectionRequested = true;

        do
        {
            if (MessageBox.Show(MAINWINDOW, "Unable to connect to the Assembly.\nRetry Assembly connection?", "Connection lost.", MessageBoxButtons.RetryCancel) == DialogResult.Cancel)
            {
                MAINWINDOW.Activate();
                MAINWINDOW.Close();
                break;
            }
        }
        while (!Connect());

        connectionRequested = false;
    }

    /// <summary>
    /// Handles the OnQuit event by attempting to reconnect to Inventor and then attempting to reconnect to the Assembly.
    /// </summary>
    /// <param name="BeforeOrAfter"></param>
    /// <param name="Context"></param>
    /// <param name="HandlingCode"></param>
    private static void ApplicationEvents_OnQuit(Inventor.EventTimingEnum BeforeOrAfter, Inventor.NameValueMap Context, out Inventor.HandlingCodeEnum HandlingCode)
    {
        HandlingCode = Inventor.HandlingCodeEnum.kEventHandled;

        MAINWINDOW.BeginInvoke(new Action(RequestConnection));
    }

    /// <summary>
    /// Handles the OnCloseDocument event by attempting to reconnect to the Assembly Document.
    /// </summary>
    /// <param name="DocumentObject"></param>
    /// <param name="FullDocumentName"></param>
    /// <param name="BeforeOrAfter"></param>
    /// <param name="Context"></param>
    /// <param name="HandlingCode"></param>
    private static void ApplicationEvents_OnCloseDocument(Inventor._Document DocumentObject, string FullDocumentName, Inventor.EventTimingEnum BeforeOrAfter, Inventor.NameValueMap Context, out Inventor.HandlingCodeEnum HandlingCode)
    {
        HandlingCode = Inventor.HandlingCodeEnum.kEventHandled;

        if (ASSEMBLY_DOCUMENT.RevisionId == DocumentObject.RevisionId)
            MAINWINDOW.BeginInvoke(new Action(RequestConnection));
    }

    /// <summary>
    /// Used for catching any unexpected thread exceptions by displaying a message and closing the application.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void Application_ThreadException(object sender, System.Threading.ThreadExceptionEventArgs e)
    {
        new CrashForm(e.Exception.ToString()).ShowDialog();

        if (INVENTOR_APPLICATION != null)
            INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = false;

        Application.Exit();
    }

    /// <summary>
    /// Used for catching any unexpected exceptions by displaying a message and closing the application.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        new CrashForm(e.ExceptionObject.ToString()).ShowDialog();

        if (INVENTOR_APPLICATION != null)
            INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = false;

        Application.Exit();
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);
        
        Application.ThreadException += Application_ThreadException;

        Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

        AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

        if (!Connect())
        {
            MessageBox.Show("Please open the field Assembly Document and try again.", "Could not connect to the Assembly Document.");
            return;
        }

        Application.Run(MAINWINDOW = new MainWindow());

        try
        {
            INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = false;
        }
        catch { }
    }
}
