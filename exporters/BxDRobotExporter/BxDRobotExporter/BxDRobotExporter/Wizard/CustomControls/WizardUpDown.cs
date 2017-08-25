using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BxDRobotExporter.Wizard
{
    public class WizardUpDown : NumericUpDown
    {
        /// <summary>
        /// The text that will be added to the numeric Up-Down after the 
        /// </summary>
        public string Unit { get; set; }

        protected override void UpdateEditText()
        {
            base.UpdateEditText();
            this.Text += " " + Unit;
        }

           
    }
}
