using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class FieldDefinition : FieldDefinition_Base
{
    public FieldDefinition()
        : this(null)
    {
    }

    public FieldDefinition(string ID)
    {
        definitionID = ID;
    }
}
