// VBConversions Note: VB project level imports
using System.Collections;
using System;
using Microsoft.VisualBasic;
using System.Diagnostics;
// End of VB project level imports


namespace AddInEvents
{
	[global::Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]public partial class Form1 : System.Windows.Forms.Form
	{
		
		//Form overrides dispose to clean up the component list.
		[System.Diagnostics.DebuggerNonUserCode()]protected override void Dispose(bool disposing)
		{
			try
			{
				if (disposing && components != null)
				{
					components.Dispose();
				}
			}
			finally
			{
				base.Dispose(disposing);
			}
		}
		
		//Required by the Windows Form Designer
		private System.ComponentModel.Container components = null;
		
		//NOTE: The following procedure is required by the Windows Form Designer
		//It can be modified using the Windows Form Designer.
		//Do not modify it using the code editor.
		[System.Diagnostics.DebuggerStepThrough()]private void InitializeComponent()
		{
			this.Button1 = new System.Windows.Forms.Button();
			this.Button1.Click += new System.EventHandler(this.Button1_Click);
			this.SuspendLayout();
			//
			//Button1
			//
			this.Button1.Location = new System.Drawing.Point(12, 12);
			this.Button1.Name = "Button1";
			this.Button1.Size = new System.Drawing.Size(145, 41);
			this.Button1.TabIndex = 1;
			this.Button1.Text = "Start Simple Interaction";
			this.Button1.UseVisualStyleBackColor = true;
			//
			//Form1
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF((float) (6.0F), (float) (13.0F));
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(171, 98);
			this.Controls.Add(this.Button1);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			
		}
		internal System.Windows.Forms.Button Button1;
	}
	
}
