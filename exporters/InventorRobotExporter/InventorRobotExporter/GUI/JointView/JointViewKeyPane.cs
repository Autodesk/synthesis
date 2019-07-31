using System.Windows.Forms;

namespace InventorRobotExporter.GUI.JointView
{
    public partial class JointViewKeyPane : UserControl
    {
        public JointViewKeyPane()
        {
            InitializeComponent();
            webBrowser1.DocumentText = Properties.Resources.DOFKey;
        }
    }
}
