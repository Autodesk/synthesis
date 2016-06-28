using System;
using Inventor;

namespace CommonDoc
{
    public partial class Form1: System.Windows.Forms.Form
    {
        Inventor.Application mApp;
        UnitsOfMeasure mUOM;

        public Form1(Inventor.Application oApp)
        {
            InitializeComponent();

            mApp = oApp;
        }

        private void Button1_Click(object sender, EventArgs e)
        {
            if (TextBox1.Text.Length > 0)
            {
                PartDocument oDoc = (PartDocument)mApp.Documents.Add(DocumentTypeEnum.kPartDocumentObject,
                    mApp.FileManager.GetTemplateFile(DocumentTypeEnum.kPartDocumentObject), 
                                                      true);

                //Author property is contained in "Inventor Summary Information" Set
                PropertySet oPropertySet = oDoc.PropertySets["{F29F85E0-4FF9-1068-AB91-08002B27B3D9}"];

                //Using Inventor.Property to avoid confusion
                Inventor.Property oProperty = oPropertySet["Author"];

                oProperty.Value = TextBox1.Text;

                //Get "Inventor User Defined Properties" set 
                oPropertySet = oDoc.PropertySets["{D5CDD505-2E9C-101B-9397-08002B2CF9AE}"];

                //Create new property
                oProperty = oPropertySet.Add("Parts R Us", "Supplier", null);

                //Save document, prompt user for filename
                FileDialog oDLG = null;
                mApp.CreateFileDialog(out oDLG);

                oDLG.FileName = @"C:\Temp\NewPart.ipt";
                oDLG.Filter = "Inventor Files (*.ipt)|*.ipt";
                oDLG.DialogTitle = "Save Part";

                oDLG.ShowSave();

                if (oDLG.FileName != "")
                {
                    oDoc.SaveAs(oDLG.FileName, false);
                    oDoc.Close(true);
                }
            }
        }

        private void Button2_Click(object sender, EventArgs e)
        {
            try
            {
                PartDocument oPartDoc = mApp.ActiveDocument as PartDocument;
                Parameter oParam = oPartDoc.ComponentDefinition.Parameters["Length"];

                double value =   (double)mUOM.GetValueFromExpression(TextBox2.Text, UnitsTypeEnum.kDefaultDisplayLengthUnits)   ;
                oParam.Value = value;

                oPartDoc.Update();
                mApp.ActiveView.Fit(true);
            }
            catch
            {
                System.Windows.Forms.MessageBox.Show("Error: Parameter \"Length\" does not exist in active document...");
            }
        }

        private void TextBox2_TextChanged(object sender, EventArgs e)
        {
            mUOM = mApp.ActiveDocument.UnitsOfMeasure;

            if (TextBox2.Text.Length > 0) 
            {
                if (mUOM.IsExpressionValid(TextBox2.Text, UnitsTypeEnum.kDefaultDisplayLengthUnits)) 
                {
                    TextBox2.ForeColor = System.Drawing.Color.Black;
                    Button2.Enabled = true;
                }
                else
                {
                    TextBox2.ForeColor = System.Drawing.Color.Red;
                    Button2.Enabled = false;
                }
            }
        }
    }
}
