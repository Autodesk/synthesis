using System.Windows.Forms;

namespace BxDRobotExporter.GUI.Editors.DegreesOfFreedomViewer
{
    public partial class DofKeyPane : UserControl
    {
        public DofKeyPane()
        {
            InitializeComponent();

            string html = Properties.Resources.DOFKey;
            webBrowser1.DocumentText = html;
        }
    }
}
