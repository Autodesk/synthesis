using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InventorServices.Persistence
{
    public interface IContextManager
    {
        dynamic BindingContextManager { get; set; }
    }
}
