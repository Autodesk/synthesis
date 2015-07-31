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
        /// The delegate method for resetting the progress bar
        /// </summary>
        private delegate void resetProgressDelegate();

        /// <summary>
        /// The delegate for adding to the progress bar
        /// </summary>
        /// <param name="value">The amount to add in percentage points</param>
        private delegate void addProgressDelegate(int value);

        /// <summary>
        /// The delegate for getting the value of the progress bar
        /// </summary>
        /// <returns></returns>
        private delegate int getProgressDelegate();

        /// <summary>
        /// The delegate for setting the progress bar label text
        /// </summary>
        /// <param name="text">The text to set after "Progress:"</param>
        private delegate void setProgressTextDelegate(string text);

        /// <summary>
        /// The delegate for calling the Finish() method
        /// </summary>
        /// <param name="logFile">The location to save the log file</param>
        private delegate void finishDelegate(string logFile);

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

            label1.Text = "";

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
            if (progressBar1.InvokeRequired)
            {
                progressBar1.Invoke(new resetProgressDelegate(ResetProgress));
                return;
            }

            progressBar1.Value = 0;
            label1.Text = "";
        }

        /// <summary>
        /// Get the value of the progress bar
        /// </summary>
        /// <returns>The progress bar value</returns>
        public int GetProgress()
        {
            if (progressBar1.InvokeRequired)
            {
                return (int) progressBar1.Invoke(new getProgressDelegate(GetProgress));
            }

            return progressBar1.Value;
        }

        /// <summary>
        /// Add progress to the progress bar
        /// </summary>
        /// <param name="percentLength">The amount to add in percentage points</param>
        public void AddProgress(int percentLength)
        {
            if (progressBar1.InvokeRequired)
            {
                progressBar1.Invoke(new addProgressDelegate(AddProgress), percentLength);
                return;
            }

            progressBar1.Step = percentLength;
            progressBar1.PerformStep();
        }

        /// <summary>
        /// Set the progress bar label text
        /// </summary>
        /// <param name="text">The text to set</param>
        public void SetProgressText(string text)
        {
            if (label1.InvokeRequired)
            {
                label1.Invoke(new setProgressTextDelegate(SetProgressText), text);
                return;
            }

            label1.Text = "Progress: " + text;
        }

        /// <summary>
        /// Finish the exporter process and allow the user to save the log
        /// </summary>
        /// <param name="logFile">The location to save the log</param>
        public void Finish(string logFile = null)
        {
            if (InvokeRequired)
            {
                Invoke(new finishDelegate(Finish), logFile);
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
            /// The delegate for writing to the log
            /// </summary>
            /// <param name="value">The text to write</param>
            private delegate void WriteDelegate(string value);

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
                    _box.Invoke(new WriteDelegate(WriteLine), value);
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
