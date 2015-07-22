using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Inventor;

public partial class InventorChooserPane : UserControl
{

    public Inventor.Application InventorApplication;

    private InteractionEvents interactionEvents;

    private SelectEvents selectEvents;

    private bool interactionActive;

    private BackgroundWorker SelectionAdder;

    public InventorChooserPane()
    {
        InitializeComponent();

        SelectionAdder = new BackgroundWorker();
        SelectionAdder.DoWork += selectionAdder_DoWork;
        SelectionAdder.RunWorkerCompleted += selectionAdder_RunWorkerCompleted;
    }

    public void Kill()
    {
        if (!interactionActive) return;

        SelectionAdder.CancelAsync();
        DisableInteraction();
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

    private void EnableInteraction()
    {
        interactionEvents = InventorApplication.CommandManager.CreateInteractionEvents();
        interactionEvents.OnActivate += interactionEvents_OnActivate;
        interactionEvents.Start();

        treeviewInventor.HotTracking = false;
        buttonSelect.Text = "End selection";

        interactionActive = true;
    }

    private void DisableInteraction()
    {
        interactionEvents.Stop();

        InventorApplication.ActiveDocument.SelectSet.Clear();

        buttonAdd.Enabled = false;
        treeviewInventor.HotTracking = true;
        buttonSelect.Text = "Select from Inventor";

        interactionActive = false;
    }

    private void interactionEvents_OnActivate()
    {
        selectEvents = interactionEvents.SelectEvents;
        selectEvents.AddSelectionFilter(SelectionFilterEnum.kAssemblyOccurrenceFilter);
        selectEvents.OnSelect += selectEvents_OnSelect;
        selectEvents.OnPreSelect += selectEvents_OnPreSelect;
    }

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
    private void selectEvents_OnPreSelect(ref object PreSelectEntity, out bool DoHighlight, ref ObjectCollection MorePreSelectEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
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
    private void selectEvents_OnSelect(ObjectsEnumerator JustSelectedEntities, SelectionDeviceEnum SelectionDevice, Inventor.Point ModelPosition, Point2d ViewPosition, Inventor.View View)
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
            selectEvents.SelectedEntities, new Action<int, int>((int progress, int total) =>
                {
                    ExporterProgressForm.SetProgressTextStatic((Math.Round((progress / (float)total) * 100.0f, 2)).ToString() + "%");
                    ExporterProgressForm.AddProgressStatic(
                                                       (int)Math.Round(((progress / (float)total) - ExporterProgressForm.GetProgressStatic()) * 100.0f, 2));
                }));
    }

    private void selectionAdder_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
    {
        ExporterProgressForm.ResetProgressStatic();

        InventorApplication.UserInterfaceManager.UserInteractionDisabled = false;

        DisableInteraction();
        buttonSelect.Enabled = true;

        List<ComponentOccurrence> c = GetComponents();
        ExporterProgressForm.Instance.UpdateComponents(c);
    }

    private void buttonSelect_Click(object sender, EventArgs e)
    {
        if (InventorApplication == null) return;

        if (!interactionActive)
        {
            EnableInteraction();
        }
        else
        {
            DisableInteraction();
        }
    }

    private void buttonAdd_Click(object sender, EventArgs e)
    {
        InventorApplication.UserInterfaceManager.UserInteractionDisabled = true;

        buttonAdd.Enabled = false;
        buttonSelect.Enabled = false;

        ExporterProgressForm.ResetProgressStatic();

        SelectionAdder.RunWorkerAsync();
    }

}
