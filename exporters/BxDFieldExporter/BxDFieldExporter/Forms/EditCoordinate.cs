using System;
using System.Windows.Forms;
using Inventor;

namespace BxDFieldExporter {
    public partial class EditCoordinate : Form
    {
        UserCoordinateSystem UCS;
        Matrix oTranslationMatrix;
        Matrix oMatrix;
        TransientGeometry oTG;
        public EditCoordinate()
        {
            InitializeComponent();
            oTG = StandardAddInServer.InventorApplication.TransientGeometry;
            oTranslationMatrix = oTG.CreateMatrix();
            oMatrix = oTG.CreateMatrix();
        }

        private void textBoxX_TextChanged(object sender, EventArgs e)
        {
            double XTranform = 0;
            try
            {
                XTranform = Convert.ToDouble(textBoxX.Text);
            }
            catch (Exception)
            {
                if (textBoxX.Text.Length > 0 && ! textBoxX.Text.Equals("-"))
                {
                    MessageBox.Show("warning, incorrect input");
                }
            }
            oTranslationMatrix.SetTranslation(oTG.CreateVector(XTranform, UCS.Transformation.Translation.Y, UCS.Transformation.Translation.Z));
            UCS.Definition.Transformation = oTranslationMatrix;
        }

        private void textBoxY_TextChanged(object sender, EventArgs e)
        {
            double YTranform = 0;
            try
            {
                YTranform = Convert.ToDouble(textBoxY.Text);
            }
            catch (Exception)
            {
                if (textBoxY.Text.Length > 0 && ! textBoxY.Text.Equals("-"))
                {
                    MessageBox.Show("warning, incorrect input");
                }
            }
            oTranslationMatrix.SetTranslation(oTG.CreateVector(UCS.Transformation.Translation.X, YTranform, UCS.Transformation.Translation.Z));
            UCS.Definition.Transformation = oTranslationMatrix;
        }

        private void textBoxZ_TextChanged(object sender, EventArgs e)
        {
            double ZTranform = 0;
            try
            {
                ZTranform = Convert.ToDouble(textBoxZ.Text);
            }
            catch (Exception)
            {
                if (textBoxZ.Text.Length > 0 && ! textBoxZ.Text.Equals("-"))
                {
                    MessageBox.Show("warning, incorrect input");
                }
            }
            oTranslationMatrix.SetTranslation(oTG.CreateVector(UCS.Transformation.Translation.X, UCS.Transformation.Translation.Y, ZTranform));
            UCS.Definition.Transformation = oTranslationMatrix;
        }
        public void readData(UserCoordinateSystem ucs)
        {
            UCS = ucs;
            textBoxX.Text = UCS.Transformation.Translation.X.ToString();
            textBoxY.Text = UCS.Transformation.Translation.Y.ToString();
            textBoxZ.Text = UCS.Transformation.Translation.Z.ToString();
        }
    }
}
