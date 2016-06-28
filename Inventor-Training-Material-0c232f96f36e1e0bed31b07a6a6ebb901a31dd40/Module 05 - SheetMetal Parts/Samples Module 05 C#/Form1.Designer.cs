// VBConversions Note: VB project level imports
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Diagnostics;
using Microsoft.VisualBasic;
using System.Linq;
using System;
using System.Collections;
using System.Xml.Linq;
using System.Windows.Forms;
// End of VB project level imports


namespace InvExeApp
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
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(Form1));
			this.ComboBoxMacros = new System.Windows.Forms.ComboBox();
			this.Button1 = new System.Windows.Forms.Button();
			this.Button1.Click += new System.EventHandler(this.Button1_Click);
			this.SuspendLayout();
			//
			//ComboBoxMacros
			//
			this.ComboBoxMacros.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ComboBoxMacros.FormattingEnabled = true;
			this.ComboBoxMacros.Location = new System.Drawing.Point(11, 29);
			this.ComboBoxMacros.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.ComboBoxMacros.Name = "ComboBoxMacros";
			this.ComboBoxMacros.Size = new System.Drawing.Size(365, 23);
			this.ComboBoxMacros.TabIndex = 0;
			//
			//Button1
			//
			this.Button1.Enabled = false;
			this.Button1.Location = new System.Drawing.Point(11, 76);
			this.Button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Button1.Name = "Button1";
			this.Button1.Size = new System.Drawing.Size(367, 39);
			this.Button1.TabIndex = 1;
			this.Button1.Text = "Execute";
			this.Button1.UseVisualStyleBackColor = true;
			//
			//Form1
			//
			this.AutoScaleDimensions = new System.Drawing.SizeF((float) (8.0F), (float) (15.0F));
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(392, 127);
			this.Controls.Add(this.Button1);
			this.Controls.Add(this.ComboBoxMacros);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Icon = (System.Drawing.Icon) (resources.GetObject("$this.Icon"));
			this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
			this.Name = "Form1";
			this.Text = "Form1";
			this.ResumeLayout(false);
			
		}
		internal System.Windows.Forms.ComboBox ComboBoxMacros;
		internal System.Windows.Forms.Button Button1;
		
	}
	
}
