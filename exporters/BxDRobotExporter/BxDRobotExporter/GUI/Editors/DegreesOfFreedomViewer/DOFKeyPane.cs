using System.Windows.Forms;

namespace BxDRobotExporter.GUI.Editors.DegreesOfFreedomViewer
{
    public partial class DOFKeyPane : UserControl
    {
        public DOFKeyPane()
        {
            InitializeComponent();

            string html = Properties.Resources.DOFKey;
            webBrowser1.DocumentText = html;
        }
    }
}
