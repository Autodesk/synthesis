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
    /// The string ID for the parent PhysicsGroup.
    /// </summary>
    public string PhysicsGroupID;

    /// <summary>
    /// The mesh ID for the FieldNode.
    /// </summary>
    public int MeshID;

    /// <summary>
    /// Contains the submeshes that make up the node.
    /// </summary>
    public BXDAMesh.BXDASubMesh SubMesh;

    /// <summary>
    /// Constructs a new instance of the FieldNode class.
    /// </summary>
    /// <param name="nodeID"></param>
    /// <param name="physicsGroupID"></param>
    public FieldNode(string nodeID, string physicsGroupID = BXDFProperties.BXDF_DEFAULT_NAME)
    {
        NodeID = nodeID;
        PhysicsGroupID = physicsGroupID;
    }
}