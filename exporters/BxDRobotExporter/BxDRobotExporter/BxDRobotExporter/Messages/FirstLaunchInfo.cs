using System.Windows.Forms;

namespace BxDRobotExporter.Messages
{
    public partial class FirstLaunchInfo : Form
    {
        public FirstLaunchInfo()
        {
            InitializeComponent();
        }

        private void Button1_Click(object sender, System.EventArgs e)
        {
            Properties.Settings.Default.ShowFirstLaunchInfo = !checkBox1.Checked;
            Properties.Settings.Default.Save();
            Close();
        }
    }
}