using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BxDRobotExporter.PrecheckPanel
{
    public partial class DOFKeyPane : UserControl
    {
        public DOFKeyPane()
        {
            InitializeComponent();

            string html = Properties.Resources.dofkey;
            webBrowser1.DocumentText = html;
        }
    }
}
