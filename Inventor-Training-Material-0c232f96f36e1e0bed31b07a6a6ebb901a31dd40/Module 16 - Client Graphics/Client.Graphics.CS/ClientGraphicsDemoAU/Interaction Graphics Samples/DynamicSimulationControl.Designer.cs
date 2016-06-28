namespace ClientGraphicsDemoAU.InteractionSamples
{
    partial class DynamicSimulationControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DynamicSimulationControl));
            this.bRun = new System.Windows.Forms.Button();
            this.bRunGraphics = new System.Windows.Forms.Button();
            this.cbTransacting = new System.Windows.Forms.CheckBox();
            this._picBox = new System.Windows.Forms.PictureBox();
            this.rbOverlay = new System.Windows.Forms.RadioButton();
            this.rbPreview = new System.Windows.Forms.RadioButton();
            this.bRunInteract = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            ((System.ComponentModel.ISupportInitialize)(this._picBox)).BeginInit();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.SuspendLayout();
            // 
            // bRun
            // 
            this.bRun.Location = new System.Drawing.Point(6, 19);
            this.bRun.Name = "bRun";
            this.bRun.Size = new System.Drawing.Size(246, 29);
            this.bRun.TabIndex = 0;
            this.bRun.Text = "Run";
            this.bRun.UseVisualStyleBackColor = true;
            this.bRun.Click += new System.EventHandler(this.bRun_Click);
            // 
            // bRunGraphics
            // 
            this.bRunGraphics.Location = new System.Drawing.Point(6, 19);
            this.bRunGraphics.Name = "bRunGraphics";
            this.bRunGraphics.Size = new System.Drawing.Size(123, 29);
            this.bRunGraphics.TabIndex = 1;
            this.bRunGraphics.Text = "Run";
            this.bRunGraphics.UseVisualStyleBackColor = true;
            this.bRunGraphics.Click += new System.EventHandler(this.bRunGraphics_Click);
            // 
            // cbTransacting
            // 
            this.cbTransacting.AutoSize = true;
            this.cbTransacting.Location = new System.Drawing.Point(172, 26);
            this.cbTransacting.Name = "cbTransacting";
            this.cbTransacting.Size = new System.Drawing.Size(82, 17);
            this.cbTransacting.TabIndex = 2;
            this.cbTransacting.Text = "Transacting";
            this.cbTransacting.UseVisualStyleBackColor = true;
            // 
            // _picBox
            // 
            this._picBox.Location = new System.Drawing.Point(11, 221);
            this._picBox.Name = "_picBox";
            this._picBox.Size = new System.Drawing.Size(258, 55);
            this._picBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this._picBox.TabIndex = 4;
            this._picBox.TabStop = false;
            // 
            // rbOverlay
            // 
            this.rbOverlay.AutoSize = true;
            this.rbOverlay.Checked = true;
            this.rbOverlay.Location = new System.Drawing.Point(172, 9);
            this.rbOverlay.Name = "rbOverlay";
            this.rbOverlay.Size = new System.Drawing.Size(61, 17);
            this.rbOverlay.TabIndex = 5;
            this.rbOverlay.TabStop = true;
            this.rbOverlay.Text = "Overlay";
            this.rbOverlay.UseVisualStyleBackColor = true;
            this.rbOverlay.CheckedChanged += new System.EventHandler(this.rbOverlay_CheckedChanged);
            // 
            // rbPreview
            // 
            this.rbPreview.AutoSize = true;
            this.rbPreview.Location = new System.Drawing.Point(172, 32);
            this.rbPreview.Name = "rbPreview";
            this.rbPreview.Size = new System.Drawing.Size(63, 17);
            this.rbPreview.TabIndex = 6;
            this.rbPreview.Text = "Preview";
            this.rbPreview.UseVisualStyleBackColor = true;
            this.rbPreview.CheckedChanged += new System.EventHandler(this.rbPreview_CheckedChanged);
            // 
            // bRunInteract
            // 
            this.bRunInteract.Location = new System.Drawing.Point(6, 19);
            this.bRunInteract.Name = "bRunInteract";
            this.bRunInteract.Size = new System.Drawing.Size(123, 29);
            this.bRunInteract.TabIndex = 7;
            this.bRunInteract.Text = "Run";
            this.bRunInteract.UseVisualStyleBackColor = true;
            this.bRunInteract.Click += new System.EventHandler(this.bRunInteract_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.bRunInteract);
            this.groupBox1.Controls.Add(this.rbPreview);
            this.groupBox1.Controls.Add(this.rbOverlay);
            this.groupBox1.Location = new System.Drawing.Point(11, 145);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(258, 60);
            this.groupBox1.TabIndex = 8;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Interaction Graphics";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.bRunGraphics);
            this.groupBox2.Controls.Add(this.cbTransacting);
            this.groupBox2.Location = new System.Drawing.Point(11, 79);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(258, 60);
            this.groupBox2.TabIndex = 9;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Client Graphics";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.bRun);
            this.groupBox3.Location = new System.Drawing.Point(11, 12);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(258, 61);
            this.groupBox3.TabIndex = 10;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Direct Simulation";
            // 
            // DynamicSimulationControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(279, 285);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this._picBox);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "DynamicSimulationControl";
            this.Text = "Dynamic Simulation [FPS: 0]";
            ((System.ComponentModel.ISupportInitialize)(this._picBox)).EndInit();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button bRun;
        private System.Windows.Forms.Button bRunGraphics;
        private System.Windows.Forms.CheckBox cbTransacting;
        private System.Windows.Forms.PictureBox _picBox;
        private System.Windows.Forms.RadioButton rbOverlay;
        private System.Windows.Forms.RadioButton rbPreview;
        private System.Windows.Forms.Button bRunInteract;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.GroupBox groupBox3;
    }
}