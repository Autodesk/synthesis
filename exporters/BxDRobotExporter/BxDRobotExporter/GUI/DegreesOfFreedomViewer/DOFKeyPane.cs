using System.Windows.Forms;

namespace BxDRobotExporter.GUI.DegreesOfFreedomViewer
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
