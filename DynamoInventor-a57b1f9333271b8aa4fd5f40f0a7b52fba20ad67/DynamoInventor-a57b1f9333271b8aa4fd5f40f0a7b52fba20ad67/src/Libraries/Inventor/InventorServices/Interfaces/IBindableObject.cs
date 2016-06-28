using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InventorServices.Persistence
{
    public interface IBindableObject
    {
        IModuleBinder Binder { get; set; }
        dynamic ObjectToBind { get; set; }
        void RegisterContextData(int moduleId, int objectId);
        //TODO: Add IsBinderValid to be checked when calling GetObjectFromTrace or SetObjectForTrace 
        //and thow exception.  Will check that client code has set IContextData and IContextManager.
    }
}
