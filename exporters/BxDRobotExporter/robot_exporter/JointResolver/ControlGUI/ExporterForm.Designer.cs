partial class ExporterForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ExporterForm));
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabSelect = new System.Windows.Forms.TabPage();
            this.nodeEditorPane1 = new NodeEditorPane();
            this.inventorChooserPane1 = new InventorChooserPane();
            this.logPage = new System.Windows.Forms.TabPage();
            this.logText = new System.Windows.Forms.RichTextBox();
            this.progressBar1 = new System.Windows.Forms.ProgressBar();
            this.label1 = new System.Windows.Forms.Label();
            this.buttonStart = new System.Windows.Forms.Button();
            this.buttonSaveLog = new System.Windows.Forms.Button();
            this.progressBarOverall = new System.Windows.Forms.ProgressBar();
            this.labelOverall = new System.Windows.Forms.Label();
            this.tabs.SuspendLayout();
            this.tabSelect.SuspendLayout();
            this.logPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // tabs
            // 
            this.tabs.Controls.Add(this.tabSelect);
            this.tabs.Controls.Add(this.logPage);
            this.tabs.Dock = System.Windows.Forms.DockStyle.Top;
            this.tabs.Location = new System.Drawing.Point(0, 0);
            this.tabs.Margin = new System.Windows.Forms.Padding(2);
            this.tabs.Name = "tabs";
            this.tabs.SelectedIndex = 0;
            this.tabs.Size = new System.Drawing.Size(588, 366);
            this.tabs.TabIndex = 0;
            // 
            // tabSelect
            // 
            this.tabSelect.Controls.Add(this.nodeEditorPane1);
            this.tabSelect.Controls.Add(this.inventorChooserPane1);
            this.tabSelect.Location = new System.Drawing.Point(4, 22);
            this.tabSelect.Margin = new System.Windows.Forms.Padding(2);
            this.tabSelect.Name = "tabSelect";
            this.tabSelect.Padding = new System.Windows.Forms.Padding(2);
            this.tabSelect.Size = new System.Drawing.Size(580, 340);
            this.tabSelect.TabIndex = 2;
            this.tabSelect.Text = "Select Parts";
            this.tabSelect.UseVisualStyleBackColor = true;
            // 
            // nodeEditorPane1
            // 
            this.nodeEditorPane1.Location = new System.Drawing.Point(2, 35);
            this.nodeEditorPane1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.nodeEditorPane1.Name = "nodeEditorPane1";
            this.nodeEditorPane1.Size = new System.Drawing.Size(570, 303);
            this.nodeEditorPane1.TabIndex = 1;
            // 
            // inventorChooserPane1
            // 
            this.inventorChooserPane1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.inventorChooserPane1.Location = new System.Drawing.Point(2, 2);
            this.inventorChooserPane1.Margin = new System.Windows.Forms.Padding(2);
            this.inventorChooserPane1.Name = "inventorChooserPane1";
            this.inventorChooserPane1.Size = new System.Drawing.Size(576, 336);
            this.inventorChooserPane1.TabIndex = 0;
            // 
            // logPage
            // 
            this.logPage.Controls.Add(this.logText);
            this.logPage.Location = new System.Drawing.Point(4, 22);
            this.logPage.Margin = new System.Windows.Forms.Padding(2);
            this.logPage.Name = "logPage";
            this.logPage.Padding = new System.Windows.Forms.Padding(2);
            this.logPage.Size = new System.Drawing.Size(580, 340);
            this.logPage.TabIndex = 1;
            this.logPage.Text = "Exporter Log";
            this.logPage.UseVisualStyleBackColor = true;
            // 
            // logText
            // 
            this.logText.BackColor = System.Drawing.Color.Black;
            this.logText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.logText.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.logText.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logText.Font = new System.Drawing.Font("Consolas", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.logText.ForeColor = System.Drawing.Color.Lime;
            this.logText.Location = new System.Drawing.Point(2, 2);
            this.logText.Margin = new System.Windows.Forms.Padding(2);
            this.logText.Name = "logText";
            this.logText.ReadOnly = true;
            this.logText.Size = new System.Drawing.Size(576, 336);
            this.logText.TabIndex = 0;
            this.logText.Text = "";
            // 
            // progressBar1
            // 
            this.progressBar1.Location = new System.Drawing.Point(3, 402);
            this.progressBar1.Margin = new System.Windows.Forms.Padding(2);
            this.progressBar1.Name = "progressBar1";
            this.progressBar1.Size = new System.Drawing.Size(580, 20);
            this.progressBar1.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBar1.TabIndex = 1;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1, 379);
            this.label1.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(48, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Progress";
            // 
            // buttonStart
            // 
            this.buttonStart.Location = new System.Drawing.Point(487, 371);
            this.buttonStart.Margin = new System.Windows.Forms.Padding(2);
            this.buttonStart.Name = "buttonStart";
            this.buttonStart.Size = new System.Drawing.Size(94, 28);
            this.buttonStart.TabIndex = 3;
            this.buttonStart.Text = "Start exporter";
            this.buttonStart.UseVisualStyleBackColor = true;
            // 
            // buttonSaveLog
            // 
            this.buttonSaveLog.Location = new System.Drawing.Point(388, 371);
            this.buttonSaveLog.Margin = new System.Windows.Forms.Padding(2);
            this.buttonSaveLog.Name = "buttonSaveLog";
            this.buttonSaveLog.Size = new System.Drawing.Size(94, 28);
            this.buttonSaveLog.TabIndex = 4;
            this.buttonSaveLog.Text = "Save log";
            this.buttonSaveLog.UseVisualStyleBackColor = true;
            // 
            // progressBarOverall
            // 
            this.progressBarOverall.Location = new System.Drawing.Point(3, 427);
            this.progressBarOverall.Margin = new System.Windows.Forms.Padding(2);
            this.progressBarOverall.Name = "progressBarOverall";
            this.progressBarOverall.Size = new System.Drawing.Size(580, 20);
            this.progressBarOverall.Style = System.Windows.Forms.ProgressBarStyle.Continuous;
            this.progressBarOverall.TabIndex = 5;
            // 
            // labelOverall
            // 
            this.labelOverall.AutoSize = true;
            this.labelOverall.Location = new System.Drawing.Point(182, 379);
            this.labelOverall.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.labelOverall.Name = "labelOverall";
            this.labelOverall.Size = new System.Drawing.Size(64, 13);
            this.labelOverall.TabIndex = 6;
            this.labelOverall.Text = "Current step";
            // 
            // ExporterForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(588, 456);
            this.Controls.Add(this.buttonSaveLog);
            this.Controls.Add(this.buttonStart);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.progressBar1);
            this.Controls.Add(this.labelOverall);
            this.Controls.Add(this.progressBarOverall);
            this.Controls.Add(this.tabs);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MaximumSize = new System.Drawing.Size(604, 495);
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(604, 495);
            this.Name = "ExporterForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Inventor Export";
            this.tabs.ResumeLayout(false);
            this.tabSelect.ResumeLayout(false);
            this.logPage.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

    }

    #endregion

    private System.Windows.Forms.TabControl tabs;
    private System.Windows.Forms.ProgressBar progressBar1;
    private System.Windows.Forms.Label label1;
    private System.Windows.Forms.TabPage logPage;
    private System.Windows.Forms.RichTextBox logText;
    private System.Windows.Forms.Button buttonStart;
    private System.Windows.Forms.Button buttonSaveLog;
	private System.Windows.Forms.ProgressBar progressBarOverall;
    private System.Windows.Forms.Label labelOverall;
    private System.Windows.Forms.TabPage tabSelect;
    private InventorChooserPane inventorChooserPane1;
    private NodeEditorPane nodeEditorPane1;
}
