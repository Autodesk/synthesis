using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Inventor;

namespace InventorServices.Persistence
{
    public interface IDocumentManager
    {
        AssemblyDocument ActiveAssemblyDoc { get; set; }
        DrawingDocument ActiveDrawingDoc { get; set; }
        PartDocument ActivePartDoc { get; set; }
        Document ActiveDocument { get; }
        Inventor.Application InventorApplication { get; set; }
        ApprenticeServerComponent ActiveApprenticeServer { get; set; }             
    }
}
