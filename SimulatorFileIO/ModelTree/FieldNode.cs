using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class FieldNode
{
    /// <summary>
    /// The string ID for the node.
    /// </summary>
    public string NodeID;

    /// <summary>
    /// The string ID for the parent PropertySet.
    /// </summary>
    public string PropertySetID;

    /// <summary>
    /// The sub mesh ID for the FieldNode.
    /// </summary>
    public int SubMeshID;

    /// <summary>
    /// The collision mesh ID for the FieldNode.
    /// </summary>
    public int CollisionMeshID;

    /// <summary>
    /// Constructs a new instance of the FieldNode class.
    /// </summary>
    /// <param name="nodeID"></param>
    /// <param name="physicsGroupID"></param>
    public FieldNode(string nodeID, string physicsGroupID = BXDFProperties.BXDF_DEFAULT_NAME)
    {
        NodeID = nodeID;
        PropertySetID = physicsGroupID;
        SubMeshID = -1;
        CollisionMeshID = -1;
    }
}