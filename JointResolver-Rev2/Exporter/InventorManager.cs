using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Inventor;

public class InventorManager
{

    private static readonly Lazy<InventorManager> _instance = new Lazy<InventorManager>(() => new InventorManager());
    public static InventorManager Instance
    {
        get
        {
            return _instance.Value;
        }
    }

    private bool loaded = false;

    private Application InventorInstance;

    public _Document ActiveDocument
    {
        get
        {
            if (!loaded) throw new InvalidComObjectException("Inventor instance not loaded");
            else return InventorInstance.ActiveDocument;
        }
    }

    public AssemblyDocument AssemblyDocument
    {
        get
        {
            if (!loaded) throw new InvalidComObjectException("Inventor instance not loaded");
            else return (AssemblyDocument) ActiveDocument;
        }
    }

    public CommandManager CommandManager
    {
        get
        {
            if (!loaded) throw new InvalidComObjectException("Inventor instance not loaded");
            else return InventorInstance.CommandManager;
        }
    }

    public TransientGeometry TransientGeometry
    {
        get
        {
            if (!loaded) throw new InvalidComObjectException("Inventor instance not loaded");
            else return InventorInstance.TransientGeometry;
        }
    }

    public TransientObjects TransientObjects
    {
        get
        {
            if (!loaded) throw new InvalidComObjectException("Inventor instance not loaded");
            else return InventorInstance.TransientObjects;
        }
    }

    public UserInterfaceManager UserInterfaceManager
    {
        get
        {
            if (!loaded) throw new InvalidComObjectException("Inventor instance not loaded");
            else return InventorInstance.UserInterfaceManager;
        }
    }

    private InteractionEvents _interactionEvents;
    public InteractionEvents InteractionEvents
    {
        get
        {
            if (!loaded) throw new InvalidComObjectException("Inventor instance not loaded");

            if (_interactionEvents == null) _interactionEvents = CommandManager.CreateInteractionEvents();
            return _interactionEvents;
        }
    }

    public SelectEvents SelectEvents
    {
        get
        {
            if (!loaded) throw new InvalidComObjectException("Inventor instance not loaded");
            else return InteractionEvents.SelectEvents;
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
        }
        catch (COMException e)
        {
            Console.WriteLine("Couldn't load Inventor instance");
            Console.WriteLine(e);
        }

        loaded = true;
    }

    public static void ReleaseInventor()
    {
        try
        {
            Marshal.ReleaseComObject(Instance.InventorInstance);
            Marshal.ReleaseComObject(Instance._interactionEvents);
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

