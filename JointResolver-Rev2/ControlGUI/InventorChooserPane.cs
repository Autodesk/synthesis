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

    private BackgroundWorker SelectionAdder;

    public InventorChooserPane()
    {
        InitializeComponent();

        SelectionAdder = new BackgroundWorker();
        SelectionAdder.DoWork += selectionAdder_DoWork;
        SelectionAdder.RunWorkerCompleted += selectionAdder_RunWorkerCompleted;
    }

    private List<ComponentOccurrence> GetComponents()
    {
        List<ComponentOccurrence> components = new List<ComponentOccurrence>();

        foreach (TreeNode node in treeviewInventor.Nodes)
        {
            components.AddRange(GetComponents(node));
        }

        return components;
    }

    private List<ComponentOccurrence> GetComponents(TreeNode node)
    {
        List<ComponentOccurrence> components = new List<ComponentOccurrence>();

        components.Add((ComponentOccurrence)node.Tag);

        foreach (TreeNode subNode in node.Nodes)
        {
            components.AddRange(GetComponents(subNode));
        }

        return components;
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

        if (PreSelectEntity is ComponentOccurrence)
        {
            ComponentOccurrence componentOccurrence = (ComponentOccurrence)PreSelectEntity;

            if (treeviewInventor.Nodes.Find(componentOccurrence.Name, true).Length > 0)
            {
                treeviewInventor.Invoke(new Action(() =>
                {
                    treeviewInventor.SelectedNode = treeviewInventor.Nodes.Find(componentOccurrence.Name, true)[0];
                    treeviewInventor.SelectedNode.EnsureVisible();
                }));
            }
        }
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
        if (!buttonAdd.Enabled)
        {
            buttonAdd.Invoke(new Action(() =>
            {
                buttonAdd.Enabled = true;
            }));
        }
    }

    private void selectionAdder_DoWork(object sender, DoWorkEventArgs e)
    {
        treeviewInventor.Invoke(new Action<ObjectsEnumerator, Action<int, int>>(treeviewInventor.AddComponents),
            InventorChooser.SelectEvents.SelectedEntities, new Action<int, int>((int progress, int total) =>
                {
                    ExporterForm.Instance.SetProgressText((Math.Round((progress / (float)total) * 100.0f, 2)).ToString() + "%");
                    ExporterForm.Instance.AddProgress((int)Math.Round(((progress / (float)total) - ExporterForm.Instance.GetProgress()) * 100.0f, 2));
                }));
    }

    private void selectionAdder_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        ExporterForm.Instance.ResetProgress();

        Exporter.INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = false;

        InventorChooser.DisableInteraction();
        buttonSelect.Enabled = true;

        ExporterForm.Instance.UpdateComponents(InventorChooser.Components);
    }

    private void buttonSelect_Click(object sender, EventArgs e)
    {
        if (!InventorChooser.InteractionActive)
        {
            InventorChooser.EnableInteraction();

            treeviewInventor.HotTracking = false;
            buttonSelect.Text = "End selection";

            InventorChooser.SelectEvents.OnPreSelect += selectEvents_OnPreSelect;
            InventorChooser.SelectEvents.OnSelect += selectEvents_OnSelect;
        }
        else
        {
            InventorChooser.DisableInteraction();

            buttonAdd.Enabled = false;
            treeviewInventor.HotTracking = true;
            buttonSelect.Text = "Select from Inventor";
        }
    }

    private void buttonAdd_Click(object sender, EventArgs e)
    {
        Exporter.INVENTOR_APPLICATION.UserInterfaceManager.UserInteractionDisabled = true;

        buttonAdd.Enabled = false;
        buttonSelect.Enabled = false;

        ExporterForm.Instance.ResetProgress();

        SelectionAdder.RunWorkerAsync();
    }
    #endregion

    public void Cleanup()
    {
        if (SelectionAdder.IsBusy) SelectionAdder.CancelAsync();
        if (InventorChooser.InteractionActive) InventorChooser.DisableInteraction();
    }

}
