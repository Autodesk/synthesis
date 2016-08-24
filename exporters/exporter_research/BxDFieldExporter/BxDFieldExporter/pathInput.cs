using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using Inventor;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.IO;

namespace BxDFieldExporter {
    public partial class pathInput : Form { 
        private System.Windows.Forms.ProgressBar progressBar1;
        private Button button1;
            public pathInput(ArrayList ArrayListFieldTypes, Inventor.Application CurrentApplicationPassed) {
            InitializeComponent();
            fieldTypes = new List<FieldDataType>();
            CurrentApplication = CurrentApplicationPassed;
            CurrentDocument = (AssemblyDocument)CurrentApplicationPassed.ActiveDocument;
            foreach (FieldDataType fieldType in ArrayListFieldTypes) {
                fieldTypes.Add(fieldType);
            }
        }
        private void button1_Click(object sender, EventArgs e) {
            button1.Enabled = false;
            BackgroundWorker messageHandler = new BackgroundWorker();
            messageHandler.DoWork += backgroundWorker_DoWork;
            messageHandler.RunWorkerCompleted += backgroundWorker_RunWorkerCompleted;
            if (!messageHandler.IsBusy) messageHandler.RunWorkerAsync();

        }
        private void backgroundWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e) {
            displaySuccess((bool)e.Result);
        }

        private void backgroundWorker_DoWork(object sender, DoWorkEventArgs e) {
            e.Result = SetUpFileReader();
        }
        private void raiseProgress() {
            progressBar1.PerformStep();
            //TODO: Make the progress bar load more fluidly
        }
        private void displaySuccess(bool result) {
            string[] paths = Directory.GetFiles(@"C:\\Users\\" + System.Environment.UserName + "\\AppData\\Roaming\\Autodesk\\Synthesis\\");
            foreach (string path in paths) {
                System.IO.File.Delete(path);
            }
            if (result) {
                MessageBox.Show("Conversion successful");
                Close();
            }
            else {
                MessageBox.Show("Conversion failed");
                button1.Enabled = true;
                progressBar1.Value = 0;
            }
        }

        private void InitializeComponent() {
            button1 = new System.Windows.Forms.Button();
            progressBar1 = new System.Windows.Forms.ProgressBar();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Location = new System.Drawing.Point(283, 12);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(75, 23);
            button1.TabIndex = 0;
            button1.Text = "Convert";
            button1.UseVisualStyleBackColor = true;
            button1.Click += new System.EventHandler(button1_Click);
            // 
            // progressBar1
            // 
            progressBar1.Location = new System.Drawing.Point(12, 12);
            progressBar1.Maximum = 60;
            progressBar1.Name = "progressBar1";
            progressBar1.Size = new System.Drawing.Size(257, 23);
            progressBar1.TabIndex = 1;
            // 
            // pathInput
            // 
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(367, 48);
            Controls.Add(progressBar1);
            Controls.Add(button1);
            Name = "pathInput";
            Text = "pathInput";
            ResumeLayout(false);

        }

    }
}
