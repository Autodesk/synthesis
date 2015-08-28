using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FieldExporter.Forms
{
    public partial class UnhandledExceptionForm : Form
    {
        public UnhandledExceptionForm(string error)
        {
            InitializeComponent();
            ErrorBox.Text = error;
        }
    }
}
