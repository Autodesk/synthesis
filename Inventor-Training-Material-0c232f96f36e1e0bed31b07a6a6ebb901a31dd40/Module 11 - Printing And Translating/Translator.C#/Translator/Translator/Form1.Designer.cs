using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

[Microsoft.VisualBasic.CompilerServices.DesignerGenerated()]
partial class Form1 : System.Windows.Forms.Form
{

    //Form overrides dispose to clean up the component list.
    [System.Diagnostics.DebuggerNonUserCode()]
    protected override void Dispose(bool disposing)
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

    private System.ComponentModel.IContainer components;
    //NOTE: The following procedure is required by the Windows Form Designer
    //It can be modified using the Windows Form Designer.  
    //Do not modify it using the code editor.
    [System.Diagnostics.DebuggerStepThrough()]
    private void InitializeComponent()
    {
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Open Options", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Save Options", System.Windows.Forms.HorizontalAlignment.Left);
            this.OptionsView = new System.Windows.Forms.ListView();
            this.OptionName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.OptionValue = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ComboAddIns = new System.Windows.Forms.ComboBox();
            this.Button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // OptionsView
            // 
            this.OptionsView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.OptionName,
            this.OptionValue});
            this.OptionsView.FullRowSelect = true;
            this.OptionsView.GridLines = true;
            listViewGroup1.Header = "Open Options";
            listViewGroup1.Name = "GroupOpenOpts";
            listViewGroup2.Header = "Save Options";
            listViewGroup2.Name = "GroupSaveOpts";
            this.OptionsView.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this.OptionsView.Location = new System.Drawing.Point(1, 5);
            this.OptionsView.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.OptionsView.Name = "OptionsView";
            this.OptionsView.Size = new System.Drawing.Size(368, 335);
            this.OptionsView.TabIndex = 0;
            this.OptionsView.UseCompatibleStateImageBehavior = false;
            // 
            // OptionName
            // 
            this.OptionName.Text = "Option";
            this.OptionName.Width = 180;
            // 
            // OptionValue
            // 
            this.OptionValue.Text = "Value";
            this.OptionValue.Width = 90;
            // 
            // ComboAddIns
            // 
            this.ComboAddIns.FormattingEnabled = true;
            this.ComboAddIns.Location = new System.Drawing.Point(1, 347);
            this.ComboAddIns.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.ComboAddIns.Name = "ComboAddIns";
            this.ComboAddIns.Size = new System.Drawing.Size(368, 23);
            this.ComboAddIns.TabIndex = 1;
            // 
            // Button1
            // 
            this.Button1.Location = new System.Drawing.Point(1, 378);
            this.Button1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Button1.Name = "Button1";
            this.Button1.Size = new System.Drawing.Size(369, 39);
            this.Button1.TabIndex = 2;
            this.Button1.Text = "Display options";
            this.Button1.UseVisualStyleBackColor = true;
            this.Button1.Click += new System.EventHandler(this.Button1_Click);
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(372, 421);
            this.Controls.Add(this.Button1);
            this.Controls.Add(this.ComboAddIns);
            this.Controls.Add(this.OptionsView);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "Form1";
            this.Text = "Translator options";
            this.ResumeLayout(false);

    }
    internal System.Windows.Forms.ListView OptionsView;
    internal System.Windows.Forms.ComboBox ComboAddIns;
    internal System.Windows.Forms.Button Button1;
    internal System.Windows.Forms.ColumnHeader OptionName;

    internal System.Windows.Forms.ColumnHeader OptionValue;
     
}