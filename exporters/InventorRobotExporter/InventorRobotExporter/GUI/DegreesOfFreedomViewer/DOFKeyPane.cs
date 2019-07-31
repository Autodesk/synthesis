using System.Windows.Forms;

namespace InventorRobotExporter.GUI.DegreesOfFreedomViewer
{
    public partial class DofKeyPane : UserControl
    {
        public DofKeyPane()
        {
            InitializeComponent();
            webBrowser1.DocumentText = Properties.Resources.DOFKey;
        }
    }
}
