using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inventor;


namespace InventorServices.Persistence
{
    public class ModuleContextManager : IContextManager
    {
        //For now this class will return the Inventor specific class. However,
        //the InventorServices.Persistence.ReferenceManager may be a better
        //future choice when BReps are in the mix.
        ReferenceKeyManager refManager;

        dynamic IContextManager.BindingContextManager
        {
            get 
            {
                //I thought about doing something like this:

                //if (refManager == null)
                //{
                //    return PersistenceManager.ActiveDocument.ReferenceKeyManager;
                //}
                //else
                //{
                //    return refManager;
                //}

                //But that is probably a bad idea. ReferenceKeyManager should
                //be explicitly set for this to work rather than falling back on 
                //assuming that the ActiveDocument should be the context for 
                //binding.

                //This is better.  Things will fail if client code doesn't set this
                //property.  Downside is more client code responsibility.
                return refManager;
            }
            set { refManager = value; }
        }
    }
}
