using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;

public static class InventorChooser
{

    public static Inventor.Application InventorApplication;

    public static SelectEvents SelectEvents;

    public static List<ComponentOccurrence> Components;

    public static bool InteractionActive;

    private static InteractionEvents interactionEvents;

    static InventorChooser()
    {
        Exporter.LoadInventorInstance();
        InventorApplication = Exporter.INVENTOR_APPLICATION;

        Components = new List<ComponentOccurrence>();
    }

    public static void EnableInteraction()
    {
        Components.Clear();

        interactionEvents = InventorApplication.CommandManager.CreateInteractionEvents();
        interactionEvents.OnActivate += interactionEvents_OnActivate;
        interactionEvents.Start();

        InteractionActive = true;
    }

    public static void DisableInteraction()
    {
        foreach (ComponentOccurrence component in SelectEvents.SelectedEntities)
        {
            Components.Add(component);
        }

        interactionEvents.Stop();

        InventorApplication.ActiveDocument.SelectSet.Clear();

        InteractionActive = false;
    }

    private static void interactionEvents_OnActivate()
    {
        SelectEvents = interactionEvents.SelectEvents;
        SelectEvents.AddSelectionFilter(SelectionFilterEnum.kAssemblyOccurrenceFilter);
    }

}
