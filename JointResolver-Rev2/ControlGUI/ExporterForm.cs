using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inventor;

public partial class ExporterForm : Form
{

    public static ExporterForm Instance;

    public RigidNode_Base ExportedNode;
    public List<BXDAMesh> ExportedMeshes;
    public bool finished;

    public List<ComponentOccurrence> Components;

    private Thread exporterThread;
    private Thread exporterProgressThread;

    private TextWriter oldConsole;
    private TextboxWriter newConsole;

    public ExporterForm()
    {
        InitializeComponent();

        Components = new List<ComponentOccurrence>();

        oldConsole = Console.Out;

        newConsole = new TextboxWriter(logText);
        Console.SetOut(newConsole);

        logText.ForeColor = System.Drawing.Color.FromArgb((int) SynthesisGUI.ExporterSettings.generalTextColor);
        logText.BackColor = System.Drawing.Color.FromArgb((int) SynthesisGUI.ExporterSettings.generalBackgroundColor);

        label1.Text = "";

        buttonSaveLog.Enabled = false;
        buttonSaveLog.Visible = false;

        FormClosed += delegate(object sender, FormClosedEventArgs e)
        {
            Console.SetOut(oldConsole);
            if (exporterProgressThread != null && exporterProgressThread.IsAlive) exporterProgressThread.Abort();
            if (exporterThread != null && exporterThread.IsAlive) exporterThread.Abort();
            Cleanup();
        };

        buttonStart.Click += delegate(object sender, EventArgs e)
        {
            if (!finished)
            {
                StartExporter();

                buttonStart.Enabled = false;
            }
            else Close();
        };

        System.Windows.Forms.Application.Idle += delegate(object sender, EventArgs e)
        {
            newConsole.printQueue();
        };

        Instance = this;
    }

    public void UpdateComponents(List<ComponentOccurrence> components)
    {
        Components.AddRange(components);
        jointGroupPane1.UpdateComponents(Components);
    }

    public void ResetProgress()
    {
        if (InvokeRequired)
        {
            Invoke((Action)(() => ResetProgress()));
            return;
        }

        progressBar1.Value = 0;
        label1.Text = "";
    }

    public int GetProgress()
    {
        if (InvokeRequired)
        {
            return (int) Invoke((Func<int>)(() => GetProgress()));
        }

        return progressBar1.Value;
    }

    public void AddProgress(int percentLength)
    {
        if (InvokeRequired)
        {
            Invoke((Action<int>)((int toAdd) => AddProgress(toAdd)), percentLength);
            return;
        }

        progressBar1.Step = percentLength;
        progressBar1.PerformStep();
    }

    public void SetProgressText(string text)
    {
        if (InvokeRequired)
        {
            Invoke((Action<string>)((string newText) => SetProgressText(newText)), text);
            return;
        }

        label1.Text = "Progress: " + text;
    }

    public void Finish(string logFile = null)
    {
        if (InvokeRequired)
        {
            Invoke((Action<string>)((string file) => Finish(file)), logFile);
            return;
        }

        if (!finished)
        {
            buttonStart.Enabled = true;
            return;
        }

        label1.Text = "Finished";

        buttonSaveLog.Enabled = (logFile != null);
        buttonSaveLog.Visible = buttonSaveLog.Enabled;

        buttonStart.Text = "Close";
        buttonStart.Enabled = true;

        buttonSaveLog.Click += delegate(object sender, EventArgs e)
        {
            try
            {
                using (StreamWriter logFileStream = new StreamWriter(logFile))
                {
                    logFileStream.Write(logText.Text);
#if DEBUG
                    Console.WriteLine("Wrote " + logFile);
#endif
                }
            }
            catch (IOException ie)
            {
                Console.WriteLine(ie);
                Console.WriteLine("Couldn't write log file " + logFile);
            }
        };

        SynthesisGUI.Instance.SkeletonBase = ExportedNode;
        SynthesisGUI.Instance.Meshes = ExportedMeshes;
    }

    public void Cleanup()
    {
        jointGroupPane1.Cleanup();
        inventorChooserPane1.Cleanup();

        InventorManager.ReleaseInventor();
    }

    private string GetLogText()
    {
        return logText.Text;
    }

    private void StartExporter()
    {
        exporterThread = new Thread(RunExporter);

        exporterThread.SetApartmentState(ApartmentState.MTA);
        exporterThread.Start();

        exporterProgressThread = new Thread(CheckExporter);

        exporterProgressThread.SetApartmentState(ApartmentState.STA);
        exporterProgressThread.Start(exporterThread);
    }

    private void RunExporter()
    {
        finished = false;

        try
        {
            ExportedNode = Exporter.ExportSkeleton(Components);
            ExportedMeshes = Exporter.ExportMeshes(ExportedNode,
                                                 SynthesisGUI.ExporterSettings.meshResolutionValue == 1, SynthesisGUI.ExporterSettings.meshFancyColors);

            ExportedNode = new OGLViewer.OGL_RigidNode(ExportedNode);
        }
        catch (COMException ce)
        {

        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
            return;
        }

        finished = true;
    }

    private void CheckExporter(object exporter)
    {
        Thread exporterThread = (Thread) exporter;

        while (!exporterThread.Join(0))
        {
            if (!Visible) exporterThread.Abort();
        }

        string logPath = SynthesisGUI.ExporterSettings.generalSaveLogLocation + "\\log_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");

        Finish(logPath);
    }

    #region Nested classes
    private class TextboxWriter : StringWriter
    {

        private delegate void WriteDelegate(string value);

        private RichTextBox _box;
        private Queue<string> lineQueue;

        public TextboxWriter(RichTextBox box)
        {
            _box = box;
            lineQueue = new Queue<string>();
        }

        public void printQueue()
        {
            if (lineQueue.Count == 0) return;

            string toPrint = "";

            while (lineQueue.Count > 0)
            {
                toPrint += lineQueue.Dequeue() + NewLine;
            }

            base.WriteLine(toPrint);
            _box.AppendText(toPrint); // When character data is written, append it to the text box.
            _box.ScrollToCaret();
        }

        public override void WriteLine(string value)
        {
            if (_box.InvokeRequired)
            {
                _box.Invoke(new WriteDelegate(WriteLine), value);
                return;
            }

            if (lineQueue.Count < 20)
                lineQueue.Enqueue(value);
        }

        public override void Write(string value)
        {
            WriteLine(value);
        }

        public override Encoding Encoding
        {
            get { return System.Text.Encoding.UTF8; }
        }

    }
    #endregion

}

