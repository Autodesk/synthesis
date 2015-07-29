using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;

public class FieldNode : FieldNode_Base
{
    public FieldNode(string ID)
        : this(ID, "")
    {
    }

    public FieldNode(string ID, string groupID)
    {
        nodeID = ID;
        this.physicsGroupID = groupID;
    }
}
