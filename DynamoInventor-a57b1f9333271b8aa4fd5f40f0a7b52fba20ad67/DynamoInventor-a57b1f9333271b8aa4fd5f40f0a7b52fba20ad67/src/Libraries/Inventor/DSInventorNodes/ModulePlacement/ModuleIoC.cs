using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InventorServices.Persistence;
using SimpleInjector;
using Autodesk.DesignScript.Runtime;

namespace InventorLibrary.ModulePlacement
{
    [IsVisibleInDynamoLibrary(false)]
    public class ModuleIoC
    {
        public static void LetThereBeIoC()
        {
            //PersistenceManager.IoC.Register<IModules, Modules>(Lifestyle.Singleton);
            //PersistenceManager.IoC.Register<IPointsList, ModulePoints>(Lifestyle.Singleton);
            HasRegistered = true;
            PersistenceManager.IoC.Verify();
        }


        //TODO: This is a hack. Change this.  This is a namespace/code organization issue.
        private static bool hasRegistered = false;

        public static bool HasRegistered
        {
            get { return hasRegistered; }
            set { hasRegistered = value; }
        }
    }
}
