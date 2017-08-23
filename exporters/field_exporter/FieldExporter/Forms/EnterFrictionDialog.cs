using System.Windows.Forms;

namespace FieldExporter.Forms
{
    public partial class EnterFrictionDialog : Form
    {
        /// <summary>
        /// Used for getting the friction value entered in the dialog.
        /// </summary>
        public int Friction
        {
            get
            {
                return (int)frictionNumericUpDown.Value;
            }
        }

        /// <summary>
        /// Initializes a new instance of the EnterFrictionDialog class.
        /// </summary>
        public EnterFrictionDialog(int initalValue)
        {
            InitializeComponent();

            frictionNumericUpDown.Value = initalValue;
        }
    }
}
