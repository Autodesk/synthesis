using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Inventor;

public class InventorManager
{

    private static Lazy<InventorManager> _instance = new Lazy<InventorManager>(() => new InventorManager());
    public static InventorManager Instance
    {
        get
        {
            try
            {
                return _instance.Value;
            }
            catch
            {
                _instance = new Lazy<InventorManager>(() => new InventorManager());
                return null;
            }
        }
    }

    public bool loaded = false;

    public Application InventorInstance;
    //private AssemblyDocument RobotDocument;

    public _Document ActiveDocument
    {
        get
        {
            if (!loaded) LoadInventor();
            return InventorInstance.ActiveDocument;
        }
    }

    public AssemblyDocument AssemblyDocument
    {
        get
        {
            if (!loaded) LoadInventor();
            return (AssemblyDocument) ActiveDocument;
        }
    }

    public AssemblyComponentDefinition ComponentDefinition
    {
        get
        {
            if (!loaded) LoadInventor();
            return AssemblyDocument.ComponentDefinition;
        }
    }

    public ComponentOccurrences ComponentOccurrences
    {
        get
        {
            if (!loaded) LoadInventor();
            return ComponentDefinition.Occurrences;
        }
    }

    public CommandManager CommandManager
    {
        get
        {
            if (!loaded) LoadInventor();
            return InventorInstance.CommandManager;
        }
    }

    public TransientGeometry TransientGeometry
    {
        get
        {
            if (!loaded) LoadInventor();
            return InventorInstance.TransientGeometry;
        }
    }

    public TransientObjects TransientObjects
    {
        get
        {
            if (!loaded) LoadInventor();
            return InventorInstance.TransientObjects;
        }
    }

    public UserInterfaceManager UserInterfaceManager
    {
        get
        {
            if (!loaded) LoadInventor();
            return InventorInstance.UserInterfaceManager;
        }
    }

    private InteractionEvents _interactionEvents;
    public InteractionEvents InteractionEvents
    {
        get
        {
            if (!loaded) LoadInventor();

            if (_interactionEvents == null) _interactionEvents = CommandManager.CreateInteractionEvents();
            return _interactionEvents;
        }
    }

    public SelectEvents SelectEvents
    {
        get
        {
            if (!loaded) LoadInventor();
            return InteractionEvents.SelectEvents;
        }
    }

    private InventorManager() 
    {
        LoadInventor();
    }

    ~InventorManager()
    {
        ReleaseInventor();
    }

    private void LoadInventor()
    {
        try
        {
            InventorInstance = (Application)Marshal.GetActiveObject("Inventor.Application");
            _interactionEvents = null;
            loaded = true;
        }
        catch (COMException e)
        {
            throw new COMException("Couldn't load Inventor instance", e);
        }
    }

    public static void ReleaseInventor()
    {
        if (Instance == null || !Instance.loaded) return;

        try
        {
            Instance.UserInterfaceManager.UserInteractionDisabled = false;

            Marshal.ReleaseComObject(Instance.InventorInstance);
        }
        catch (COMException e)
        {
            Console.WriteLine("Couldn't release Inventor instance");
            Console.WriteLine(e);
        }

        Instance.loaded = false;
    }

    public static void Reload()
    {
        ReleaseInventor();
        Instance.LoadInventor();
    }

}

