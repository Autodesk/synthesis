using System;
using System.Runtime.InteropServices;
using Inventor;

namespace BxDRobotExporter.Exporter
{
    public class InventorManager
    {

        private static Lazy<InventorManager> instance = new Lazy<InventorManager>(() => new InventorManager());
        public static InventorManager Instance
        {
            get
            {
                try
                {
                    return instance.Value;
                }
                catch
                {
                    instance = new Lazy<InventorManager>(() => new InventorManager());
                    return null;
                }
            }
        }

        public bool Loaded = false;

        public Application InventorInstance;
        //private AssemblyDocument RobotDocument;

        public _Document ActiveDocument
        {
            get
            {
                if (!Loaded) LoadInventor();
                return InventorInstance.ActiveDocument;
            }
        }

        public AssemblyDocument AssemblyDocument
        {
            get
            {
                if (!Loaded) LoadInventor();
                return (AssemblyDocument) ActiveDocument;
            }
        }

        public AssemblyComponentDefinition ComponentDefinition
        {
            get
            {
                if (!Loaded) LoadInventor();
                return AssemblyDocument.ComponentDefinition;
            }
        }

        public ComponentOccurrences ComponentOccurrences
        {
            get
            {
                if (!Loaded) LoadInventor();
                return ComponentDefinition.Occurrences;
            }
        }

        public CommandManager CommandManager
        {
            get
            {
                if (!Loaded) LoadInventor();
                return InventorInstance.CommandManager;
            }
        }

        public TransientGeometry TransientGeometry
        {
            get
            {
                if (!Loaded) LoadInventor();
                return InventorInstance.TransientGeometry;
            }
        }

        public TransientObjects TransientObjects
        {
            get
            {
                if (!Loaded) LoadInventor();
                return InventorInstance.TransientObjects;
            }
        }

        public UserInterfaceManager UserInterfaceManager
        {
            get
            {
                if (!Loaded) LoadInventor();
                return InventorInstance.UserInterfaceManager;
            }
        }

        private InteractionEvents interactionEvents;
        public InteractionEvents InteractionEvents
        {
            get
            {
                if (!Loaded) LoadInventor();

                if (interactionEvents == null) interactionEvents = CommandManager.CreateInteractionEvents();
                return interactionEvents;
            }
        }

        public SelectEvents SelectEvents
        {
            get
            {
                if (!Loaded) LoadInventor();
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
                interactionEvents = null;
                Loaded = true;
            }
            catch (COMException e)
            {
                throw new COMException("Couldn't load Inventor instance", e);
            }
        }

        public static void ReleaseInventor()
        {
            if (Instance == null || !Instance.Loaded) return;

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

            Instance.Loaded = false;
        }

        public static void Reload()
        {
            ReleaseInventor();
            Instance.LoadInventor();
        }

    }
}

