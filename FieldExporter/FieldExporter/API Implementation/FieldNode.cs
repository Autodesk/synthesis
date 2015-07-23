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
        : this(ID, FieldNodeCollisionType.NONE, true, 0)
    {
    }

    public FieldNode(string ID, FieldNodeCollisionType collisionType, bool isConvex, int frictionValue)
    {
        nodeID = ID;
        nodeCollisionType = collisionType;
        convex = isConvex;
        friction = frictionValue;
    }
}
