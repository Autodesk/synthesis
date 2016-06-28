using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Inventor;

public class FieldNode : FieldNode_Base
{
    public FieldNode()
        : this(null, FieldNodeCollisionType.NONE)
    {
    }

    public FieldNode(string ID, FieldNodeCollisionType collisionType)
    {
        nodeID = ID;
        nodeCollisionType = collisionType;
    }
}
