using System.ComponentModel;

namespace InventorRobotExporter.Messages
{
    partial class AnalyticsSurvey
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnalyticsSurvey));
            this.submitButton = new System.Windows.Forms.Button();
            this.reasonTextList = new System.Windows.Forms.CheckedListBox();
            this.reasonLabel = new System.Windows.Forms.Label();
            this.teamTextBox = new System.Windows.Forms.TextBox();
            this.teamLabel = new System.Windows.Forms.Label();
            this.otherTextBox = new System.Windows.Forms.TextBox();
            this.skipButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // submitButton
            // 
            this.submitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.submitButton.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.submitButton.Location = new System.Drawing.Point(297, 242);
            this.submitButton.Margin = new System.Windows.Forms.Padding(2);
            this.submitButton.Name = "submitButton";
            this.submitButton.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.submitButton.Size = new System.Drawing.Size(116, 27);
            this.submitButton.TabIndex = 3;
            this.submitButton.Text = "Submit";
            this.submitButton.UseVisualStyleBackColor = true;
            this.submitButton.Click += new System.EventHandler(this.SubmitButton_Click);
            // 
            // reasonTextList
            // 
            this.reasonTextList.BackColor = System.Drawing.SystemColors.Control;
            this.reasonTextList.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.reasonTextList.CheckOnClick = true;
            this.reasonTextList.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.reasonTextList.FormattingEnabled = true;
            this.reasonTextList.Items.AddRange(new object[] {
            "Robot design testing",
            "Robot code testing",
            "Drive practice",
            "Competition strategy analysis",
            "Other: (Please Specify Below)"});
            this.reasonTextList.Location = new System.Drawing.Point(17, 79);
            this.reasonTextList.Margin = new System.Windows.Forms.Padding(4);
            this.reasonTextList.Name = "reasonTextList";
            this.reasonTextList.Size = new System.Drawing.Size(367, 105);
            this.reasonTextList.TabIndex = 5;
            this.reasonTextList.UseCompatibleTextRendering = true;
            this.reasonTextList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.ReasonTextList_ItemCheck);
            // 
            // reasonLabel
            // 
            this.reasonLabel.AutoSize = true;
            this.reasonLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.reasonLabel.Location = new System.Drawing.Point(13, 54);
            this.reasonLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.reasonLabel.Name = "reasonLabel";
            this.reasonLabel.Size = new System.Drawing.Size(281, 20);
            this.reasonLabel.TabIndex = 6;
            this.reasonLabel.Text = "Why does your team use Synthesis?";
            // 
            // teamTextBox
            // 
            this.teamTextBox.Location = new System.Drawing.Point(280, 19);
            this.teamTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.teamTextBox.Name = "teamTextBox";
            this.teamTextBox.Size = new System.Drawing.Size(83, 24);
            this.teamTextBox.TabIndex = 7;
            this.teamTextBox.TextChanged += new System.EventHandler(this.TeamTextBox_TextChanged);
            // 
            // teamLabel
            // 
            this.teamLabel.AutoSize = true;
            this.teamLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.teamLabel.Location = new System.Drawing.Point(13, 19);
            this.teamLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.teamLabel.Name = "teamLabel";
            this.teamLabel.Size = new System.Drawing.Size(254, 20);
            this.teamLabel.TabIndex = 8;
            this.teamLabel.Text = "What is your FRC team number?";
            this.teamLabel.Click += new System.EventHandler(this.TeamLabel_Click);
            // 
            // otherTextBox
            // 
            this.otherTextBox.Location = new System.Drawing.Point(29, 192);
            this.otherTextBox.Margin = new System.Windows.Forms.Padding(4);
            this.otherTextBox.Name = "otherTextBox";
            this.otherTextBox.Size = new System.Drawing.Size(355, 24);
            this.otherTextBox.TabIndex = 10;
            this.otherTextBox.Visible = false;
            // 
            // skipButton
            // 
            this.skipButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.skipButton.Location = new System.Drawing.Point(13, 240);
            this.skipButton.Margin = new System.Windows.Forms.Padding(4);
            this.skipButton.Name = "skipButton";
            this.skipButton.Size = new System.Drawing.Size(110, 27);
            this.skipButton.TabIndex = 11;
            this.skipButton.Text = "Skip";
            this.skipButton.UseVisualStyleBackColor = true;
            this.skipButton.Click += new System.EventHandler(this.SkipButton_Click);
            // 
            // AnalyticsSurvey
            // 
            this.AcceptButton = this.submitButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(120F, 120F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(424, 280);
            this.Controls.Add(this.skipButton);
            this.Controls.Add(this.otherTextBox);
            this.Controls.Add(this.teamLabel);
            this.Controls.Add(this.teamTextBox);
            this.Controls.Add(this.reasonLabel);
            this.Controls.Add(this.reasonTextList);
            this.Controls.Add(this.submitButton);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 9F);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(2);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AnalyticsSurvey";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Post-Export Survey";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button submitButton;
        private System.Windows.Forms.CheckedListBox reasonTextList;
        private System.Windows.Forms.Label reasonLabel;
        private System.Windows.Forms.TextBox teamTextBox;
        private System.Windows.Forms.Label teamLabel;
        private System.Windows.Forms.TextBox otherTextBox;
        private System.Windows.Forms.Button skipButton;
    }
}