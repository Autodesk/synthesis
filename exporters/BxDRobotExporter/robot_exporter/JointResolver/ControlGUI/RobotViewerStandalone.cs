using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace JointResolver.ControlGUI
{
    public partial class RobotViewerStandalone : Form
    {
        public RobotViewerStandalone()
        {
            InitializeComponent();
        }

        public EditorsLibrary.RobotViewer Viewer { get => robotViewer1; set => robotViewer1 = value; }
    }
}
