using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inventor;

public partial class InventorChooserPane : UserControl
{

    public InventorChooserPane()
    {
        InitializeComponent();
    }

    public void ButtonAdd_Click(object sender, EventArgs e)
    {
        if (InventorManager.Instance == null)
        {
            MessageBox.Show("Couldn't detect a running instance of Inventor.");
            return;
        }

        if (InventorManager.Instance.ActiveDocument == null || !(InventorManager.Instance.ActiveDocument is AssemblyDocument))
        {
            MessageBox.Show("Couldn't detect an open assembly");
            return;
        }

        InventorManager.Instance.UserInterfaceManager.UserInteractionDisabled = true;

        buttonAdd.Enabled = false;

        ExporterForm.Instance.ResetProgress();

        RigidNode_Base skeleton = Exporter.ExportSkeleton(InventorManager.Instance.ComponentOccurrences.OfType<ComponentOccurrence>().ToList());
        ExporterForm.Instance.UpdateComponents(skeleton);

        //InventorManager.Instance.UserInterfaceManager.UserInteractionDisabled = false;
        buttonAdd.Enabled = true;
        SynthesisGUI.Instance.ExporterReset();
        SynthesisGUI.Instance.ExporterOverallReset();
    }

}
