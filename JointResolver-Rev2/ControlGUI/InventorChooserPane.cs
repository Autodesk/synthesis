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

    #region Events
    /// <summary>
    /// Allows the user to see if they have already added a collision component in select mode.
    /// </summary>
    /// <param name="PreSelectEntity"></param>
    /// <param name="DoHighlight"></param>
    /// <param name="MorePreSelectEntities"></param>
    /// <param name="SelectionDevice"></param>
    /// <param name="ModelPosition"></param>
    /// <param name="ViewPosition"></param>
    /// <param name="View"></param>
    private void selectEvents_OnPreSelect(ref object PreSelectEntity, out bool DoHighlight, ref ObjectCollection MorePreSelectEntities, 
                                             SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
    {
        DoHighlight = true;
    }

    /// <summary>
    /// Enables the "Add Selection" button when an object in Inventor is selected.
    /// </summary>
    /// <param name="JustSelectedEntities"></param>
    /// <param name="SelectionDevice"></param>
    /// <param name="ModelPosition"></param>
    /// <param name="ViewPosition"></param>
    /// <param name="View"></param>
    private void selectEvents_OnSelect(ObjectsEnumerator JustSelectedEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, 
                                       Point2d ViewPosition, Inventor.View View)
    {
        if (InventorChooser.Components.Count <= 1)
        {
            buttonAdd.Invoke(new Action(() =>
            {
                buttonAdd.Text = "Add Selection";
            }));
        }
    }

    private void buttonSelect_Click(object sender, EventArgs e)
    {
        if (!InventorChooser.InteractionActive)
        {
            InventorChooser.EnableInteraction();
            InventorManager.Instance.UserInterfaceManager.UserInteractionDisabled = false;

            buttonSelect.Text = "End selection";
            buttonAdd.Text = "Add all";
            buttonAdd.Enabled = true;

            InventorManager.Instance.SelectEvents.OnPreSelect += selectEvents_OnPreSelect;
            InventorManager.Instance.SelectEvents.OnSelect += selectEvents_OnSelect;
        }
        else
        {
            InventorChooser.DisableInteraction();

            buttonAdd.Enabled = false;
            buttonSelect.Text = "Select in Inventor";
        }
    }

    private void buttonAdd_Click(object sender, EventArgs e)
    {
        if (InventorChooser.Components.Count >= 1)
        {
            InventorManager.Instance.UserInterfaceManager.UserInteractionDisabled = true;

            buttonAdd.Enabled = false;
            buttonSelect.Enabled = false;
            buttonSelect.Text = "Select in Inventor";

            ExporterForm.Instance.ResetProgress();

            InventorChooser.DisableInteraction();

            RigidNode_Base skeleton = Exporter.ExportSkeleton(InventorChooser.Components);
            ExporterForm.Instance.UpdateComponents(skeleton);

            InventorManager.Instance.UserInterfaceManager.UserInteractionDisabled = false;
            buttonSelect.Enabled = true;
            SynthesisGUI.Instance.ExporterReset();
            SynthesisGUI.Instance.ExporterOverallReset();
        }
        else
        {
            InventorManager.Instance.UserInterfaceManager.UserInteractionDisabled = true;

            buttonAdd.Enabled = false;
            buttonSelect.Enabled = false;
            buttonSelect.Text = "Select in Inventor";

            ExporterForm.Instance.ResetProgress();

            InventorChooser.DisableInteraction();

            RigidNode_Base skeleton = Exporter.ExportSkeleton(InventorManager.Instance.ComponentOccurrences.Cast<ComponentOccurrence>().ToList());
            ExporterForm.Instance.UpdateComponents(skeleton);

            InventorManager.Instance.UserInterfaceManager.UserInteractionDisabled = false;
            buttonSelect.Enabled = true;
            SynthesisGUI.Instance.ExporterReset();
            SynthesisGUI.Instance.ExporterOverallReset();
        }
    }
    #endregion

    public void Cleanup()
    {
        if (InventorChooser.InteractionActive) InventorChooser.DisableInteraction();
    }

}
