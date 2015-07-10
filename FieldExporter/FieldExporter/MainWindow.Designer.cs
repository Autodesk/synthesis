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
            this.DocumentScanner = new System.ComponentModel.BackgroundWorker();
            this.ScanButton = new System.Windows.Forms.Button();
            this.DocumentView = new System.Windows.Forms.TreeView();
            this.CollisionObjectsLabel = new System.Windows.Forms.Label();
            this.ComponentsViewLabel = new System.Windows.Forms.Label();
            this.CollisionObjectsView = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // DocumentScanner
            // 
            this.DocumentScanner.DoWork += new System.ComponentModel.DoWorkEventHandler(this.DocumentScanner_DoWork);
            this.DocumentScanner.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.DocumentScanner_RunWorkerCompleted);
            // 
            // ScanButton
            // 
            this.ScanButton.Location = new System.Drawing.Point(12, 12);
            this.ScanButton.Name = "ScanButton";
            this.ScanButton.Size = new System.Drawing.Size(598, 64);
            this.ScanButton.TabIndex = 4;
            this.ScanButton.Text = "Scan Document";
            this.ScanButton.UseVisualStyleBackColor = true;
            this.ScanButton.Click += new System.EventHandler(this.ScanButton_Click);
            // 
            // DocumentView
            // 
            this.DocumentView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.DocumentView.Location = new System.Drawing.Point(12, 99);
            this.DocumentView.Name = "DocumentView";
            this.DocumentView.Size = new System.Drawing.Size(296, 321);
            this.DocumentView.TabIndex = 2;
            this.DocumentView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.DocumentView_ItemDrag);
            // 
            // CollisionObjectsLabel
            // 
            this.CollisionObjectsLabel.Location = new System.Drawing.Point(314, 79);
            this.CollisionObjectsLabel.Name = "CollisionObjectsLabel";
            this.CollisionObjectsLabel.Size = new System.Drawing.Size(296, 17);
            this.CollisionObjectsLabel.TabIndex = 6;
            this.CollisionObjectsLabel.Text = "Objects With Collision";
            this.CollisionObjectsLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // ComponentsViewLabel
            // 
            this.ComponentsViewLabel.Location = new System.Drawing.Point(12, 79);
            this.ComponentsViewLabel.Name = "ComponentsViewLabel";
            this.ComponentsViewLabel.Size = new System.Drawing.Size(296, 17);
            this.ComponentsViewLabel.TabIndex = 7;
            this.ComponentsViewLabel.Text = "Components In Assembly";
            this.ComponentsViewLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // CollisionObjectsView
            // 
            this.CollisionObjectsView.AllowDrop = true;
            this.CollisionObjectsView.Location = new System.Drawing.Point(314, 99);
            this.CollisionObjectsView.Name = "CollisionObjectsView";
            this.CollisionObjectsView.Size = new System.Drawing.Size(293, 105);
            this.CollisionObjectsView.TabIndex = 8;
            this.CollisionObjectsView.DragDrop += new System.Windows.Forms.DragEventHandler(this.CollisionObjectsView_DragDrop);
            this.CollisionObjectsView.DragEnter += new System.Windows.Forms.DragEventHandler(this.CollisionObjectsView_DragEnter);
            this.CollisionObjectsView.KeyDown += new System.Windows.Forms.KeyEventHandler(this.CollisionObjectsView_KeyDown);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(622, 435);
            this.Controls.Add(this.CollisionObjectsView);
            this.Controls.Add(this.ComponentsViewLabel);
            this.Controls.Add(this.CollisionObjectsLabel);
            this.Controls.Add(this.ScanButton);
            this.Controls.Add(this.DocumentView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainWindow";
            this.Text = "Field Exporter";
            this.Activated += new System.EventHandler(this.MainWindow_Activated);
            this.ResumeLayout(false);

        }

        #endregion

        private System.ComponentModel.BackgroundWorker DocumentScanner;
        private System.Windows.Forms.Button ScanButton;
        private System.Windows.Forms.TreeView DocumentView;
        private System.Windows.Forms.Label CollisionObjectsLabel;
        private System.Windows.Forms.Label ComponentsViewLabel;
        private System.Windows.Forms.TreeView CollisionObjectsView;
    }
}

