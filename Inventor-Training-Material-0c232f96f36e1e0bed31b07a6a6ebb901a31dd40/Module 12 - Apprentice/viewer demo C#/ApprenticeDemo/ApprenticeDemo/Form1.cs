using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

 
namespace ApprenticeDemo
{
    public partial class Form1 : Form
    {
        string mFilename;

        public Form1()
        {
            InitializeComponent();
        }

        private void btnOpen_Click(object sender, EventArgs e)
        {
              mFilename = OpenFile();

            if (mFilename != "") 
                axInventorViewControl1.FileName = mFilename;
             
        }

        private string OpenFile()
        {

         string filename = "";

         System.Windows.Forms.OpenFileDialog ofDlg = new System.Windows.Forms.OpenFileDialog();

         string user = System.Windows.Forms.SystemInformation.UserName;

         ofDlg.Title = "Open Inventor File";
         ofDlg.InitialDirectory = "C:\\Documents and Settings\\" + user + "\\Desktop\\";

         //"Inventor Files (*.ipt)|*.ipt|Inventor Assemblies (*.iam)|*.iam|Inventor Drawings (*.idw)|*.idw"
         ofDlg.Filter = "Inventor files (*.ipt; *.iam; *.idw)|*.ipt;*.iam;*.idw";
         ofDlg.FilterIndex = 1;
         ofDlg.RestoreDirectory = true;

         if ((ofDlg.ShowDialog() == DialogResult.OK)) {
          filename = ofDlg.FileName;
         }

         return filename;

        }


    }
}
