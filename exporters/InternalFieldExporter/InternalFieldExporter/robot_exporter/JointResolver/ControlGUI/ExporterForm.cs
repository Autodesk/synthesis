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
using EditorsLibrary;

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

    PluginSettingsForm.PluginSettingsValues ExporterSettings;

    public ExporterForm(PluginSettingsForm.PluginSettingsValues settings)
    {
        InitializeComponent();

        Components = new List<ComponentOccurrence>();

        oldConsole = Console.Out;

        newConsole = new TextboxWriter(logText);
        Console.SetOut(newConsole);

        label1.Text = "";
        labelOverall.Text = "";

        buttonSaveLog.Enabled = false;
        buttonSaveLog.Visible = false;

        ExporterSettings = settings;

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

    public void UpdateComponents(RigidNode_Base skeletonBase)
    {
        ExportedNode = skeletonBase;
        List<RigidNode_Base> nodes = new List<RigidNode_Base>();
        skeletonBase.ListAllNodes(nodes);
        nodeEditorPane1.AddNodes(nodes);
    }

    public void ResetProgress()
    {
        if (InvokeRequired)
        {
            Invoke((Action)ResetProgress);
            return;
        }

        progressBar1.Value = 0;
        label1.Text = "";
    }

    public void ResetOverall()
    {
        if (InvokeRequired)
        {
            Invoke((Action)ResetOverall);
            return;
        }

        progressBarOverall.Value = 0;
        labelOverall.Text = "";
    }

    public int GetProgress()
    {
        if (InvokeRequired)
        {
            return (int)Invoke((Func<int>)GetProgress);
        }

        return progressBar1.Value;
    }

    public void AddProgress(int percentLength)
    {
        if (InvokeRequired)
        {
            Invoke((Action<int>)AddProgress, percentLength);
            return;
        }

        progressBar1.Step = percentLength;
        progressBar1.PerformStep();
    }

    public void SetProgressText(string text)
    {
        if (InvokeRequired)
        {
            Invoke((Action<string>)SetProgressText, text);
            return;
        }

        label1.Text = "Progress: " + text;
    }

    public void SetNumMeshes(int meshes)
    {
        if (InvokeRequired)
        {
            Invoke((Action<int>)SetNumMeshes, meshes);
            return;
        }

        progressBarOverall.Maximum = meshes;
        progressBarOverall.Value = 0;
    }

    public void AddOverallStep()
    {
        if (InvokeRequired)
        {
            Invoke((Action)AddOverallStep);
            return;
        }

        progressBarOverall.Step = 1;
        progressBarOverall.PerformStep();
    }

    public void SetOverallText(string text)
    {
        if (InvokeRequired)
        {
            Invoke((Action<string>)SetOverallText, text);
            return;
        }

        labelOverall.Text = "Current step: " + text;
    }

    public void Finish(string logFile = null)
    {
        if (InvokeRequired)
        {
            Invoke((Action<string>)Finish, logFile);
            return;
        }

        if (!finished)
        {
            buttonStart.Enabled = true;
            return;
        }

        label1.Text = "";
        labelOverall.Text = "Finished";
        progressBar1.Value = 100;
        progressBarOverall.Value = progressBarOverall.Maximum;

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
                    MessageBox.Show("Saved!");
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
        if (InventorManager.Instance == null)
        {
            MessageBox.Show("Couldn't detect a running instance of Inventor.\nIs it open?");
            return;
        }

        if (InventorManager.Instance.ActiveDocument == null || !(InventorManager.Instance.ActiveDocument is AssemblyDocument))
        {
            MessageBox.Show("Couldn't detect an open assembly");
            return;
        }

        InventorManager.Instance.UserInterfaceManager.UserInteractionDisabled = true;

        try
        {
            ExportedMeshes = Exporter.ExportMeshes(ExportedNode);
            ExportedNode = new OGLViewer.OGL_RigidNode(ExportedNode);
        }
        catch (COMException)
        {
        }
        catch (Exception e)
        {
            MessageBox.Show(e.Message);
            InventorManager.Instance.UserInterfaceManager.UserInteractionDisabled = false;
            return;
        }

        InventorManager.Instance.UserInterfaceManager.UserInteractionDisabled = false;
        finished = true;
    }

    private void CheckExporter(object exporter)
    {
        Thread exporterThread = (Thread)exporter;

        Invoke((Action) delegate
        {
            nodeEditorPane1.Enabled = false;
            inventorChooserPane1.Enabled = false;
        });

        while (!exporterThread.Join(0))
        {
            if (!Visible) exporterThread.Abort();
        }

        Invoke((Action)delegate
        {
            nodeEditorPane1.Enabled = true;
            inventorChooserPane1.Enabled = true;
        });
        Finish();
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

