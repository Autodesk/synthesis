using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Inventor;


namespace BrowserSample
{
    public partial class UserControl1 : UserControl
    {
        public UserControl1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MessageBox.Show("The control in my pane is clicked !");
        }
        public void DrawASketchRectangle(Inventor.Application InvApp)
        {
            PartDocument oPartDoc = InvApp.ActiveDocument as PartDocument;
            PartComponentDefinition oPartDef = oPartDoc.ComponentDefinition;

            Point2d oPoint1 = InvApp.TransientGeometry.CreatePoint2d(0, 0);
            Point2d oPoint2 = InvApp.TransientGeometry.CreatePoint2d(10, 10);

            oPartDef.Sketches[1].SketchLines.AddAsTwoPointRectangle(oPoint1, oPoint2);

            InvApp.ActiveDocument.Views[1].Fit();

        }


    }
}
