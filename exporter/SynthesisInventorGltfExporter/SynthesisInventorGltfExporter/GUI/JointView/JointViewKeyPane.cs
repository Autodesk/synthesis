using System.Windows.Forms;

namespace SynthesisInventorGltfExporter.GUI.JointView
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
