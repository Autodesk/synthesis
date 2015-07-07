namespace FieldExporter
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainWindow));
            this.DocumentView = new System.Windows.Forms.TreeView();
            this.RefreshButton = new System.Windows.Forms.Button();
            this.ScanButton = new System.Windows.Forms.Button();
            this.DocumentList = new System.Windows.Forms.ListBox();
            this.DocumentScanner = new System.ComponentModel.BackgroundWorker();
            this.SuspendLayout();
            // 
            // DocumentView
            // 
            this.DocumentView.Location = new System.Drawing.Point(12, 162);
            this.DocumentView.Name = "DocumentView";
            this.DocumentView.Size = new System.Drawing.Size(256, 261);
            this.DocumentView.TabIndex = 2;
            // 
            // RefreshButton
            // 
            this.RefreshButton.Location = new System.Drawing.Point(12, 12);
            this.RefreshButton.Name = "RefreshButton";
            this.RefreshButton.Size = new System.Drawing.Size(256, 32);
            this.RefreshButton.TabIndex = 3;
            this.RefreshButton.Text = "Refresh Document List";
            this.RefreshButton.UseVisualStyleBackColor = true;
            this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
            // 
            // ScanButton
            // 
            this.ScanButton.Location = new System.Drawing.Point(12, 124);
            this.ScanButton.Name = "ScanButton";
            this.ScanButton.Size = new System.Drawing.Size(256, 32);
            this.ScanButton.TabIndex = 4;
            this.ScanButton.Text = "Scan Selected Document";
            this.ScanButton.UseVisualStyleBackColor = true;
            this.ScanButton.Click += new System.EventHandler(this.ScanButton_Click);
            // 
            // DocumentList
            // 
            this.DocumentList.FormattingEnabled = true;
            this.DocumentList.ItemHeight = 16;
            this.DocumentList.Location = new System.Drawing.Point(12, 50);
            this.DocumentList.Name = "DocumentList";
            this.DocumentList.Size = new System.Drawing.Size(256, 68);
            this.DocumentList.TabIndex = 5;
            // 
            // DocumentScanner
            // 
            this.DocumentScanner.DoWork += new System.ComponentModel.DoWorkEventHandler(this.DocumentScanner_DoWork);
            this.DocumentScanner.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.DocumentScanner_RunWorkerCompleted);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 435);
            this.Controls.Add(this.DocumentList);
            this.Controls.Add(this.ScanButton);
            this.Controls.Add(this.RefreshButton);
            this.Controls.Add(this.DocumentView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "Field Exporter";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView DocumentView;
        private System.Windows.Forms.Button RefreshButton;
        private System.Windows.Forms.Button ScanButton;
        private System.Windows.Forms.ListBox DocumentList;
        private System.ComponentModel.BackgroundWorker DocumentScanner;
    }
}

