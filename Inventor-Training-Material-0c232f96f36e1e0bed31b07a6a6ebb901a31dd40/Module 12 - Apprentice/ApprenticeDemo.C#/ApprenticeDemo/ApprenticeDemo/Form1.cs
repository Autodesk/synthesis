using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using InventorApprentice;

namespace ApprenticeDemo
{
    public partial class Form1 : Form
    {
        ApprenticeServerComponent mApprenticeServer ; 
        ApprenticeServerDocument mCurrentDoc ; 
        ApprenticeServerDrawingDocument mCurrentDrawingDoc;

        public Form1()
        {
            InitializeComponent();
        }

        private void OpenButton_Click(object sender, EventArgs e)
        {
            if (((mCurrentDoc != null)))
            {
                mCurrentDoc.Close();
            }

            string filename = OpenFile();


            if ((!string.IsNullOrEmpty(filename)))
            {
                mCurrentDoc = mApprenticeServer.Open(filename);


                if ((mCurrentDoc.DocumentType == DocumentTypeEnum.kDrawingDocumentObject))
                {
                    mCurrentDrawingDoc = (ApprenticeServerDrawingDocument)mCurrentDoc;
                }


                IPictureDisp oPicDisp = (IPictureDisp)mCurrentDoc.Thumbnail;
                Image image = Microsoft.VisualBasic.Compatibility.VB6.Support.IPictureDispToImage(oPicDisp);

                //image.Save("c:\Temp\Thumbnail.bmp", System.Drawing.Imaging.ImageFormat.Bmp)

                PreviewPic.Image = image;
                PreviewPic.Refresh();

                lbFilename.Text = "File: " + mCurrentDoc.DisplayName;

                ViewerButton.Enabled = true;
            }


        }

        private void ViewerButton_Click(object sender, EventArgs e)
        {
            Form2 viewer = new Form2(mCurrentDoc.FullFileName); 
            viewer.ShowDialog();
        }

        private void btnSetPro_Click(object sender, EventArgs e)
        {
            SetProperty(TextBoxAuthor.Text);
        }

        //Small helper function that prompts user for a file selection
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

            if ((ofDlg.ShowDialog() == DialogResult.OK))
            {
                filename = ofDlg.FileName;
            }

            return filename;

        }

        // set property [Author]
        private void SetProperty(string author)
        {
            ApprenticeServerDocument oApprenticeDoc = default(ApprenticeServerDocument);
            oApprenticeDoc = mApprenticeServer.Open(OpenFile());

            //Get "Inventor Summary Information" PropertySet
            PropertySet oPropertySet = oApprenticeDoc.PropertySets["{F29F85E0-4FF9-1068-AB91-08002B27B3D9}"];

            //Get Author property
            InventorApprentice.Property oProperty = oPropertySet["Author"];

            oProperty.Value = author;

            oApprenticeDoc.PropertySets.FlushToFile();
            oApprenticeDoc.Close();

        }


        private void Form1_Load(object sender, EventArgs e)
        {
               mApprenticeServer = new ApprenticeServerComponent();
            ViewerButton.Enabled = false;

            PreviewPic.SizeMode = PictureBoxSizeMode.StretchImage;

            mCurrentDoc = null;

        }


    }
}
