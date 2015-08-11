using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditorsLibrary
{

    /// <summary>
    /// The form used to show exporter progress and display the log
    /// </summary>
    public partial class ExporterProgressForm : Form
    {

        /// <summary>
        /// Set to true when the exporter has finished
        /// </summary>
        public bool finished;

        /// <summary>
        /// The old Console.Out
        /// </summary>
        private TextWriter oldConsole;

        /// <summary>
        /// The exporter log displayed to the user in the form
        /// </summary>
        private TextboxWriter newConsole;

        /// <summary>
        /// Create a new progress form
        /// </summary>
        /// <param name="startEvent">The event to communicate the exporter starting to the <see cref="ExporterGUI"/></param>
        /// <param name="textColor">The color of the log text</param>
        /// <param name="backgroundColor">The color of the log background</param>
        public ExporterProgressForm(AutoResetEvent startEvent, Color textColor, Color backgroundColor)
        {
            InitializeComponent();

            oldConsole = Console.Out;

            newConsole = new TextboxWriter(logText);
            Console.SetOut(newConsole);

            logText.ForeColor = textColor;
            logText.BackColor = backgroundColor;

            labelProgress.Text = "";
            labelOverall.Text = "";
            progressBarOverall.Maximum = 4;
            progressBarOverall.Value = 0;

            buttonSaveLog.Enabled = false;
            buttonSaveLog.Visible = false;

            FormClosing += delegate(object sender, FormClosingEventArgs e)
            {
                Console.SetOut(oldConsole);
                finished = true;
                startEvent.Set();
            };

            buttonStart.Click += delegate(object sender, EventArgs e)
            {
                if (!finished)
                {
                    startEvent.Set();
                    buttonStart.Enabled = false;
                }
                else Close();
            };

            Application.Idle += delegate(object sender, EventArgs e)
            {
                newConsole.printQueue();
            };
        }

        /// <summary>
        /// Reset the progress bar
        /// </summary>
        public void ResetProgress()
        {
            if (InvokeRequired)
            {
                Invoke((Action)ResetProgress);
                return;
            }

            progressBarCurrent.Value = 0;
            labelProgress.Text = "";
        }

        /// <summary>
        /// Get the value of the progress bar
        /// </summary>
        /// <returns>The progress bar value</returns>
        public int GetProgress()
        {
            if (InvokeRequired)
            {
                return (int) Invoke((Func<int>)GetProgress);
            }

            return progressBarCurrent.Value;
        }

        /// <summary>
        /// Add progress to the progress bar
        /// </summary>
        /// <param name="percentLength">The amount to add in percentage points</param>
        public void AddProgress(int percentLength)
        {
            if (InvokeRequired)
            {
                Invoke((Action<int>)AddProgress, percentLength);
                return;
            }

            progressBarCurrent.Step = percentLength;
            progressBarCurrent.PerformStep();
        }

        /// <summary>
        /// Set the progress bar label text
        /// </summary>
        /// <param name="text">The text to set</param>
        public void SetProgressText(string text)
        {
            if (InvokeRequired)
            {
                Invoke((Action<string>)SetProgressText, text);
                return;
            }

            labelProgress.Text = "Progress: " + text;
        }

        public void SetNumMeshes(int meshes)
        {
            if (InvokeRequired)
            {
                Invoke((Action<int>)SetNumMeshes, meshes);
                return;
            }

            progressBarOverall.Maximum = meshes * 2;
            progressBarOverall.Value = meshes;
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

        /// <summary>
        /// Finish the exporter process and allow the user to save the log
        /// </summary>
        /// <param name="logFile">The location to save the log</param>
        public void Finish(string logFile = null)
        {
            if (InvokeRequired)
            {
                Invoke((Action<string>)Finish, logFile);
                return;
            }

            labelProgress.Text = "Finished";
            labelOverall.Text = "";

            buttonSaveLog.Enabled = (logFile != null);
            buttonSaveLog.Visible = buttonSaveLog.Enabled;

            buttonStart.Text = "Close";
            buttonStart.Enabled = true;
            buttonStart.TabIndex = 0;

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

            finished = true;
        }

        /// <summary>
        /// Get the text of the exporter log
        /// </summary>
        /// <returns>The log</returns>
        public string GetLogText()
        {
            return logText.Text;
        }

        /// <summary>
        /// The class that displays the exporter log to the user
        /// </summary>
        private class TextboxWriter : StringWriter
        {

            /// <summary>
            /// The textbox to display the log in
            /// </summary>
            private RichTextBox _box;

            /// <summary>
            /// A queue of text lines to be displayed when the application is idling
            /// </summary>
            /// <remarks>
            /// This prevents significant slowdown while the exporter is running
            /// </remarks>
            private Queue<string> lineQueue;

            /// <summary>
            /// Create a new TextboxWriter
            /// </summary>
            /// <param name="box">The text box to display the log in</param>
            public TextboxWriter(RichTextBox box)
            {
                _box = box;
                lineQueue = new Queue<string>();
            }

            /// <summary>
            /// Print all lines currently in the queue
            /// </summary>
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

            /// <summary>
            /// Write a line of text to the log
            /// </summary>
            /// <param name="value">The text to write</param>
            public override void WriteLine(string value)
            {
                if (_box.InvokeRequired)
                {
                    _box.Invoke((Action<string>)((string val) => WriteLine(val)), value);
                    return;
                }

                lineQueue.Enqueue(value);
            }

            /// <summary>
            /// Write text to the log
            /// </summary>
            /// <param name="value">The text to write</param>
            public override void Write(string value)
            {
                WriteLine(value);
            }

            /// <summary>
            /// The text encoding to use for the output
            /// </summary>
            public override Encoding Encoding
            {
                get { return System.Text.Encoding.UTF8; }
            }

        }

    }
}
