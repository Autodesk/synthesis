using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InventorServices.Persistence
{
    public class ModuleObject : IBindableObject
    {
        public IModuleBinder Binder { get; set; }
        public dynamic ObjectToBind { get; set; }

        public ModuleObject(IModuleBinder binder)
        {
            Binder = binder;   
        }

        public void RegisterContextData(int moduleId, int objectId)
        {
            Binder.ContextData.Context = new Tuple<int, int>(moduleId, objectId);
        }
    }
}
