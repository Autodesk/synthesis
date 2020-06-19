using System.ComponentModel;

namespace SynthesisExporterInventor.GUI.Messages
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
            this.reasonLabel = new System.Windows.Forms.Label();
            this.teamInput = new System.Windows.Forms.TextBox();
            this.teamLabel = new System.Windows.Forms.Label();
            this.otherInput = new System.Windows.Forms.TextBox();
            this.skipButton = new System.Windows.Forms.Button();
            this.choiceDesignButton = new System.Windows.Forms.RadioButton();
            this.choiceOtherButton = new System.Windows.Forms.RadioButton();
            this.choiceStrategyButton = new System.Windows.Forms.RadioButton();
            this.choiceDriverButton = new System.Windows.Forms.RadioButton();
            this.choiceCodeButton = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // submitButton
            // 
            this.submitButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.submitButton.ImageAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.submitButton.Location = new System.Drawing.Point(238, 284);
            this.submitButton.Margin = new System.Windows.Forms.Padding(2);
            this.submitButton.Name = "submitButton";
            this.submitButton.Padding = new System.Windows.Forms.Padding(0, 0, 0, 2);
            this.submitButton.Size = new System.Drawing.Size(75, 23);
            this.submitButton.TabIndex = 3;
            this.submitButton.Text = "Submit";
            this.submitButton.UseVisualStyleBackColor = true;
            this.submitButton.Click += new System.EventHandler(this.SubmitButton_Click);
            // 
            // reasonLabel
            // 
            this.reasonLabel.AutoSize = true;
            this.reasonLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.reasonLabel.Location = new System.Drawing.Point(10, 90);
            this.reasonLabel.Name = "reasonLabel";
            this.reasonLabel.Size = new System.Drawing.Size(238, 17);
            this.reasonLabel.TabIndex = 6;
            this.reasonLabel.Text = "Why does your team use Synthesis?";
            // 
            // teamInput
            // 
            this.teamInput.Location = new System.Drawing.Point(229, 62);
            this.teamInput.MaxLength = 4;
            this.teamInput.Name = "teamInput";
            this.teamInput.Size = new System.Drawing.Size(67, 21);
            this.teamInput.TabIndex = 7;
            this.teamInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.TeamInput_KeyPress);
            // 
            // teamLabel
            // 
            this.teamLabel.AutoSize = true;
            this.teamLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F);
            this.teamLabel.Location = new System.Drawing.Point(10, 62);
            this.teamLabel.Name = "teamLabel";
            this.teamLabel.Size = new System.Drawing.Size(213, 17);
            this.teamLabel.TabIndex = 8;
            this.teamLabel.Text = "What is your FRC team number?";
            // 
            // otherInput
            // 
            this.otherInput.Location = new System.Drawing.Point(28, 245);
            this.otherInput.Name = "otherInput";
            this.otherInput.Size = new System.Drawing.Size(285, 21);
            this.otherInput.TabIndex = 10;
            this.otherInput.Visible = false;
            // 
            // skipButton
            // 
            this.skipButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.skipButton.Location = new System.Drawing.Point(10, 282);
            this.skipButton.Name = "skipButton";
            this.skipButton.Size = new System.Drawing.Size(75, 23);
            this.skipButton.TabIndex = 11;
            this.skipButton.Text = "Skip";
            this.skipButton.UseVisualStyleBackColor = true;
            this.skipButton.Click += new System.EventHandler(this.SkipButton_Click);
            // 
            // choiceDesignButton
            // 
            this.choiceDesignButton.AutoSize = true;
            this.choiceDesignButton.Location = new System.Drawing.Point(13, 111);
            this.choiceDesignButton.Name = "choiceDesignButton";
            this.choiceDesignButton.Size = new System.Drawing.Size(137, 19);
            this.choiceDesignButton.TabIndex = 12;
            this.choiceDesignButton.TabStop = true;
            this.choiceDesignButton.Text = "Robot design testing";
            this.choiceDesignButton.UseVisualStyleBackColor = true;
            // 
            // choiceOtherButton
            // 
            this.choiceOtherButton.AutoSize = true;
            this.choiceOtherButton.Location = new System.Drawing.Point(13, 211);
            this.choiceOtherButton.Name = "choiceOtherButton";
            this.choiceOtherButton.Size = new System.Drawing.Size(146, 19);
            this.choiceOtherButton.TabIndex = 13;
            this.choiceOtherButton.TabStop = true;
            this.choiceOtherButton.Text = "Other: (please specify)";
            this.choiceOtherButton.UseVisualStyleBackColor = true;
            this.choiceOtherButton.CheckedChanged += new System.EventHandler(this.ChoiceOtherButton_CheckedChanged);
            // 
            // choiceStrategyButton
            // 
            this.choiceStrategyButton.AutoSize = true;
            this.choiceStrategyButton.Location = new System.Drawing.Point(13, 186);
            this.choiceStrategyButton.Name = "choiceStrategyButton";
            this.choiceStrategyButton.Size = new System.Drawing.Size(183, 19);
            this.choiceStrategyButton.TabIndex = 15;
            this.choiceStrategyButton.TabStop = true;
            this.choiceStrategyButton.Text = "Competition strategy analysis";
            this.choiceStrategyButton.UseVisualStyleBackColor = true;
            // 
            // choiceDriverButton
            // 
            this.choiceDriverButton.AutoSize = true;
            this.choiceDriverButton.Location = new System.Drawing.Point(13, 161);
            this.choiceDriverButton.Name = "choiceDriverButton";
            this.choiceDriverButton.Size = new System.Drawing.Size(99, 19);
            this.choiceDriverButton.TabIndex = 16;
            this.choiceDriverButton.TabStop = true;
            this.choiceDriverButton.Text = "Drive practice";
            this.choiceDriverButton.UseVisualStyleBackColor = true;
            // 
            // choiceCodeButton
            // 
            this.choiceCodeButton.AutoSize = true;
            this.choiceCodeButton.Location = new System.Drawing.Point(13, 136);
            this.choiceCodeButton.Name = "choiceCodeButton";
            this.choiceCodeButton.Size = new System.Drawing.Size(127, 19);
            this.choiceCodeButton.TabIndex = 17;
            this.choiceCodeButton.TabStop = true;
            this.choiceCodeButton.Text = "Robot code testing";
            this.choiceCodeButton.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(15, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(312, 31);
            this.label1.TabIndex = 18;
            this.label1.Text = "This form will only display once, and helps improve to Synthesis.";
            // 
            // AnalyticsSurvey
            // 
            this.AcceptButton = this.submitButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(339, 314);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.choiceCodeButton);
            this.Controls.Add(this.choiceDriverButton);
            this.Controls.Add(this.choiceStrategyButton);
            this.Controls.Add(this.choiceOtherButton);
            this.Controls.Add(this.choiceDesignButton);
            this.Controls.Add(this.skipButton);
            this.Controls.Add(this.otherInput);
            this.Controls.Add(this.teamLabel);
            this.Controls.Add(this.teamInput);
            this.Controls.Add(this.reasonLabel);
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
        private System.Windows.Forms.Label reasonLabel;
        private System.Windows.Forms.TextBox teamInput;
        private System.Windows.Forms.Label teamLabel;
        private System.Windows.Forms.TextBox otherInput;
        private System.Windows.Forms.Button skipButton;
        private System.Windows.Forms.RadioButton choiceDesignButton;
        private System.Windows.Forms.RadioButton choiceOtherButton;
        private System.Windows.Forms.RadioButton choiceStrategyButton;
        private System.Windows.Forms.RadioButton choiceDriverButton;
        private System.Windows.Forms.RadioButton choiceCodeButton;
        private System.Windows.Forms.Label label1;
    }
}