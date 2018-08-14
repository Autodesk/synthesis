using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BxDRobotExporter.Forms
{
    public partial class DebugHighlightForm : Form
    {
        public DebugHighlightForm()
        {
            InitializeComponent();
        }

        public DialogResult ShowDialog(out string ComponentName)
        {
            DialogResult Result = ShowDialog();
            if (Result == DialogResult.OK)
            {
                ComponentName = textBox1.Text;
                return DialogResult;
            }
            else
            {
                ComponentName = null;
                return Result;
            }
        }
    }
} 
