using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class FieldNode_Base
{
    /// <summary>
    /// Delegate for returning new FieldNode_Base instances.
    /// </summary>
    /// <returns></returns>
    public delegate FieldNode_Base FieldNodeFactory();

    /// <summary>
    /// Default delegate for returning new instances.
    /// </summary>
    public static FieldNodeFactory FIELDNODE_FACTORY = delegate()
    {
        return new FieldNode_Base();
    };

    /// <summary>
    /// The string ID for the node.
    /// </summary>
    public string nodeID
    {
        get;
        set;
    }

    /// <summary>
    /// The string ID for the parent PhysicsGroup.
    /// </summary>
    public string physicsGroupID
    {
        get;
        set;
    }

    /// <summary>
    /// Contains the submeshes that make up the node.
    /// </summary>
    private List<BXDAMesh.BXDASubMesh> subMeshes = new List<BXDAMesh.BXDASubMesh>();

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