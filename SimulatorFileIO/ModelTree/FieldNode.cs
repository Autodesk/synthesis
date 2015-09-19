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
    /// Contains the submeshes that make up the node.
    /// </summary>
    private List<BXDAMesh.BXDASubMesh> subMeshes = new List<BXDAMesh.BXDASubMesh>();

    /// <summary>
    /// Constructs a new instance of the FieldNode class.
    /// </summary>
    /// <param name="nodeID"></param>
    public FieldNode(string nodeID)
        : this(nodeID, "")
    {
    }

    /// <summary>
    /// Constructs a new instance of the FieldNode class.
    /// </summary>
    /// <param name="nodeID"></param>
    /// <param name="physicsGroupID"></param>
    public FieldNode(string nodeID, string physicsGroupID)
    {
        NodeID = nodeID;
        PhysicsGroupID = physicsGroupID;
    }

    /// <summary>
    /// Add a submesh to the list of submeshes.
    /// </summary>
    /// <param name="subMesh"></param>
    public void AddSubMesh(BXDAMesh.BXDASubMesh subMesh)
    {
        subMeshes.Add(subMesh);
    }

    /// <summary>
    /// Adds multiple submeshes to the list of submeshes from the mesh supplied.
    /// </summary>
    /// <param name="mesh"></param>
    public void AddSubMeshes(BXDAMesh mesh)
    {
        subMeshes.AddRange(mesh.meshes);
    }

    /// <summary>
    /// Clears the list of submeshes.
    /// </summary>
    public void ClearSubMeshes()
    {
        subMeshes.Clear();
    }

    /// <summary>
    /// Returns the list of submeshes.
    /// </summary>
    /// <returns></returns>
    public List<BXDAMesh.BXDASubMesh> GetSubMeshes()
    {
        return subMeshes;
    }

}