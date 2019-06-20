using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JointResolver.EditorsLibrary
{
    public partial class AnalyticsPrompt : Form
    {
        public AnalyticsPrompt()
        {
            InitializeComponent();
            
        }

        private void Label1_Click(object sender, EventArgs e)
        {

        }

        private void ButtonSettings_Click(object sender, EventArgs e)
        {
            SynthesisGUI.Instance.PromptExporterSettings();
        }

        private void ButtonClose_Click(object sender, EventArgs e)
        {
            Close();
        }

    }
}
