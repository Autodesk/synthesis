using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EditorsLibrary
{
    public partial class ExporterGUI : Form
    {
        public ExporterGUI()
        {
            InitializeComponent();
        }

        public void OpenExisting()
        {

        }

        private void bxdaEditorPane1_Load(object sender, EventArgs e)
        {

        }

        private void robotViewer1_Load(object sender, EventArgs e)
        {

        }

        private void bxdaEditorPane1_Load_1(object sender, EventArgs e)
        {

        }

        private void jointEditorPane1_Load(object sender, EventArgs e)
        {
            RigidNode_Base.NODE_FACTORY = delegate()
            {
                return new RigidNode_Base();
            };
            RigidNode_Base nodeBase = BXDJSkeleton.ReadSkeleton(BXDSettings.Instance.LastSkeletonDirectory + "\\skeleton.bxdj");

            jointEditorPane1.SetSkeleton(nodeBase);
        }
    }
}
