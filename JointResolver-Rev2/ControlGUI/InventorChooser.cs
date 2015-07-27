using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Inventor;

public static class InventorChooser
{

    public static List<ComponentOccurrence> Components;

    public static bool InteractionActive;

    static InventorChooser()
    {
        Components = new List<ComponentOccurrence>();
    }

    public static void EnableInteraction()
    {
        Components.Clear();

        InventorManager.Instance.InteractionEvents.OnActivate += interactionEvents_OnActivate;
        InventorManager.Instance.InteractionEvents.Start();

        InteractionActive = true;
    }

    public static void DisableInteraction()
    {
        foreach (ComponentOccurrence component in InventorManager.Instance.SelectEvents.SelectedEntities)
        {
            Components.Add(component);
        }

        InventorManager.Instance.InteractionEvents.Stop();

        InventorManager.Instance.ActiveDocument.SelectSet.Clear();

        InteractionActive = false;
    }

    private static void interactionEvents_OnActivate()
    {
        InventorManager.Instance.SelectEvents.AddSelectionFilter(SelectionFilterEnum.kAssemblyOccurrenceFilter);
    }

}
