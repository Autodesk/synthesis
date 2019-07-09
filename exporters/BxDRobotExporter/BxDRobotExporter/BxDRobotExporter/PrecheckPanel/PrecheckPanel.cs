using System.Windows.Forms;

namespace BxDRobotExporter.PrecheckPanel
{
    public partial class PrecheckPanel : UserControl
    {
        public PrecheckPanel()
        {
            InitializeComponent();

            string documentText = Properties.Resources.ExportGuide;
            
            webBrowser1.Refresh();
            webBrowser1.DocumentText = documentText;
        }
    }
}