using System.ComponentModel;

namespace InventorRobotExporter.Messages
{
    partial class FirstLaunchInfo
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private IContainer components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FirstLaunchInfo));
            this.submitButton = new System.Windows.Forms.Button();
            this.analyticsCheckBox = new System.Windows.Forms.CheckBox();
            this.reasonTextList = new System.Windows.Forms.CheckedListBox();
            this.reasonLabel = new System.Windows.Forms.Label();
            this.teamTextBox = new System.Windows.Forms.TextBox();
            this.teamLabel = new System.Windows.Forms.Label();
            this.instructionsLabel = new System.Windows.Forms.Label();
            this.otherTextBox = new System.Windows.Forms.TextBox();
            this.skipButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // submitButton
            // 
            this.submitButton.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.submitButton.Location = new System.Drawing.Point(205, 289);
            this.submitButton.Margin = new System.Windows.Forms.Padding(2);
            this.submitButton.Name = "submitButton";
            this.submitButton.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.submitButton.Size = new System.Drawing.Size(93, 30);
            this.submitButton.TabIndex = 3;
            this.submitButton.Text = "Submit";
            this.submitButton.UseVisualStyleBackColor = true;
            this.submitButton.Click += new System.EventHandler(this.SubmitButton_Click);
            // 
            // analyticsCheckBox
            // 
            this.analyticsCheckBox.AutoSize = true;
            this.analyticsCheckBox.Checked = true;
            this.analyticsCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.analyticsCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.analyticsCheckBox.Location = new System.Drawing.Point(18, 263);
            this.analyticsCheckBox.Name = "analyticsCheckBox";
            this.analyticsCheckBox.Size = new System.Drawing.Size(322, 21);
            this.analyticsCheckBox.TabIndex = 4;
            this.analyticsCheckBox.Text = "Track additional analytics to improve Synthesis";
            this.analyticsCheckBox.UseVisualStyleBackColor = true;
            // 
            // reasonTextList
            // 
            this.reasonTextList.BackColor = System.Drawing.SystemColors.Control;
            this.reasonTextList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.reasonTextList.CheckOnClick = true;
            this.reasonTextList.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.reasonTextList.FormattingEnabled = true;
            this.reasonTextList.Items.AddRange(new object[] {
            "To test designs before creating our robot",
            "To test our code",
            "To give our drivers practice before competition",
            "To formulate a strategy or tour the field",
            "Other"});
            this.reasonTextList.Location = new System.Drawing.Point(19, 147);
            this.reasonTextList.Name = "reasonTextList";
            this.reasonTextList.Size = new System.Drawing.Size(288, 90);
            this.reasonTextList.TabIndex = 5;
            this.reasonTextList.UseCompatibleTextRendering = true;
            this.reasonTextList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ReasonTextList_ItemCheck);
            // 
            // reasonLabel
            // 
            this.reasonLabel.AutoSize = true;
            this.reasonLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.reasonLabel.Location = new System.Drawing.Point(16, 127);
            this.reasonLabel.Name = "reasonLabel";
            this.reasonLabel.Size = new System.Drawing.Size(282, 17);
            this.reasonLabel.TabIndex = 6;
            this.reasonLabel.Text = "Why does your team mainly use Synthesis?";
            // 
            // teamTextBox
            // 
            this.teamTextBox.Location = new System.Drawing.Point(246, 88);
            this.teamTextBox.Name = "teamTextBox";
            this.teamTextBox.Size = new System.Drawing.Size(67, 21);
            this.teamTextBox.TabIndex = 7;
            // 
            // teamLabel
            // 
            this.teamLabel.AutoSize = true;
            this.teamLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.teamLabel.Location = new System.Drawing.Point(16, 88);
            this.teamLabel.Name = "teamLabel";
            this.teamLabel.Size = new System.Drawing.Size(224, 17);
            this.teamLabel.TabIndex = 8;
            this.teamLabel.Text = "What FRC team are you a part of?";
            // 
            // instructionsLabel
            // 
            this.instructionsLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.instructionsLabel.Location = new System.Drawing.Point(16, 9);
            this.instructionsLabel.MaximumSize = new System.Drawing.Size(500, 500);
            this.instructionsLabel.Name = "instructionsLabel";
            this.instructionsLabel.Size = new System.Drawing.Size(334, 58);
            this.instructionsLabel.TabIndex = 9;
            this.instructionsLabel.Text = "The Synthesis robot exporter plugin has been added. To access the exporter, selec" +
    "t the \"Robot Export\" under the \"Environments\" tab.";
            // 
            // otherTextBox
            // 
            this.otherTextBox.Location = new System.Drawing.Point(84, 218);
            this.otherTextBox.Name = "otherTextBox";
            this.otherTextBox.Size = new System.Drawing.Size(223, 21);
            this.otherTextBox.TabIndex = 10;
            this.otherTextBox.Visible = false;
            // 
            // skipButton
            // 
            this.skipButton.Location = new System.Drawing.Point(84, 291);
            this.skipButton.Name = "skipButton";
            this.skipButton.Size = new System.Drawing.Size(88, 28);
            this.skipButton.TabIndex = 11;
            this.skipButton.Text = "Skip";
            this.skipButton.UseVisualStyleBackColor = true;
            this.skipButton.Click += new System.EventHandler(this.SkipButton_Click);
            // 
            // FirstLaunchInfo
            // 
            this.AcceptButton = this.submitButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(367, 330);
            this.Controls.Add(this.skipButton);
            this.Controls.Add(this.otherTextBox);
            this.Controls.Add(this.instructionsLabel);
            this.Controls.Add(this.teamLabel);
            this.Controls.Add(this.teamTextBox);
            this.Controls.Add(this.reasonLabel);
            this.Controls.Add(this.reasonTextList);
            this.Controls.Add(this.analyticsCheckBox);
            this.Controls.Add(this.submitButton);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FirstLaunchInfo";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Successfully loaded Synthesis plugin";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button submitButton;
        private System.Windows.Forms.CheckBox analyticsCheckBox;
        private System.Windows.Forms.CheckedListBox reasonTextList;
        private System.Windows.Forms.Label reasonLabel;
        private System.Windows.Forms.TextBox teamTextBox;
        private System.Windows.Forms.Label teamLabel;
        private System.Windows.Forms.Label instructionsLabel;
        private System.Windows.Forms.TextBox otherTextBox;
        private System.Windows.Forms.Button skipButton;
    }
}