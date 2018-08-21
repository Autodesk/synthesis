using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InternalFieldExporter.FieldWizard
{
    public partial class CrashForm : Form
    {
        /// <summary>
        /// Initializes a new instance of the CrashForm class
        /// </summary>
        /// <param name="error"></param>
        public CrashForm(string error)
        {
            InitializeComponent();
        }
    }
}