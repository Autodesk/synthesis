using System.Windows.Forms;

namespace EditorsLibrary
{
    public partial class ParametersEditor : Form
    {
        public ParametersEditor()
        {
            InitializeComponent();

            FormClosing += delegate (object sender, FormClosingEventArgs e) { LegacyInterchange.LegacyEvents.OnRobotModified(); };

        }
    }
}
