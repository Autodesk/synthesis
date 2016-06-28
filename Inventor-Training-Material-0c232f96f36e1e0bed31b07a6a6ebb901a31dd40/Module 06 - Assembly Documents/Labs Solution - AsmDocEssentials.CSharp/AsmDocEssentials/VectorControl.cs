using Inventor;

namespace AsmDocEssentials
{
    public partial class VectorControl
    {

        private Inventor.Application mApp;

        private DigitTextBox xTxtBox;
        private DigitTextBox yTxtBox;
        private DigitTextBox zTxtBox;

        public VectorControl()
        {

            // This call is required by the Windows Form Designer.
            InitializeComponent();

            xTxtBox = new DigitTextBox();
            xTxtBox.Name = "DigitTextBox1";
            xTxtBox.Location = new System.Drawing.Point(5, 20);
            xTxtBox.Size = new System.Drawing.Size(35, 30);
            xTxtBox.TabIndex = 1;
            xTxtBox.Text = "";

            yTxtBox = new DigitTextBox();
            yTxtBox.Name = "DigitTextBox1";
            yTxtBox.Location = new System.Drawing.Point(45, 20);
            yTxtBox.Size = new System.Drawing.Size(35, 30);
            yTxtBox.TabIndex = 2;
            yTxtBox.Text = "";

            zTxtBox = new DigitTextBox();
            zTxtBox.Name = "DigitTextBox3";
            zTxtBox.Location = new System.Drawing.Point(85, 20);
            zTxtBox.Size = new System.Drawing.Size(35, 30);
            zTxtBox.TabIndex = 3;
            zTxtBox.Text = "";

            this.Controls.Add(xTxtBox);
            this.Controls.Add(yTxtBox);

            this.Controls.Add(zTxtBox);
        }


        public Inventor.Application SetApp
        {

            set { mApp = value; }
        }



        public string VectorName
        {

            get { return label1.Text; }

            set { label1.Text = value; }
        }



        public Vector Vector
        {

            get
            {
                double x = 0;
                double y = 0;
                double z = 0;

                if ((xTxtBox.TextLength == 0))
                {
                    x = 0;
                }
                else
                {
                    x = System.Double.Parse(xTxtBox.Text);
                }

                if ((yTxtBox.TextLength == 0))
                {
                    y = 0;
                }
                else
                {
                    y = System.Double.Parse(yTxtBox.Text);
                }

                if ((zTxtBox.TextLength == 0))
                {
                    z = 0;
                }
                else
                {
                    z = System.Double.Parse(zTxtBox.Text);
                }


                return mApp.TransientGeometry.CreateVector(x, y, z);
            }

            set
            {

                xTxtBox.Text = value.X.ToString("F2");
                yTxtBox.Text = value.Y.ToString("F2");

                zTxtBox.Text = value.Z.ToString("F2");
            }
        }
    }
}