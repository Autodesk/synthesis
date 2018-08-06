using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace InternalFieldExporter
{
    class SynthesisColorTable : ProfessionalColorTable
    {
        /// <summary>
        /// The background color of the color table.
        /// </summary>
        private System.Drawing.Color backgroundColor;

        /// <summary>
        /// Initializes a new instance of the SynthesisColorTable class.
        /// </summary>
        public SynthesisColorTable()
        {
            backgroundColor = System.Drawing.Color.FromArgb(158, 205, 163);
        }

        /// <summary>
        /// Overrides the MenuStripGradientBegin with the background color.
        /// </summary>
        public override System.Drawing.Color MenuStripGradientBegin
        {
            get
            {
                return backgroundColor;
            }
        }

        /// <summary>
        /// Overrides the MenuStripGradientEnd with the background color.
        /// </summary>
        public override System.Drawing.Color MenuStripGradientEnd
        {
            get
            {
                return backgroundColor;
            }
        }
    }
}
