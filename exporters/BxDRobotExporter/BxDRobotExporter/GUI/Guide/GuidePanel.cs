using System.Windows.Forms;

namespace BxDRobotExporter.GUI.Guide
{
    public partial class ExportGuidePanel : UserControl
    {
        public ExportGuidePanel()
        {
            InitializeComponent();

            string documentText = Properties.Resources.ExportGuide;
            
            webBrowser1.Refresh();
            webBrowser1.DocumentText = documentText;
        }
    }
}