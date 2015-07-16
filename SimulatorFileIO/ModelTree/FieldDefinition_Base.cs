using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class FieldDefinition_Base
{
    /// <summary>
    /// Delegate for returning new FieldDefinition_Base instances.
    /// </summary>
    /// <returns></returns>
    public delegate FieldDefinition_Base FieldDefinitionFactory();

    /// <summary>
    /// Default delegate for returning new instances.
    /// </summary>
    public static FieldDefinitionFactory FIELDDEFINITION_FACTORY = delegate()
    {
        return new FieldDefinition_Base();
    };

    /// <summary>
    /// Stores the ID of this definition (for naming, labels, etc.)
    /// </summary>
    public string definitionID
    {
        set;
        get;
    }

    /// <summary>
    /// The mesh to be exported.
    /// </summary>
    private BXDAMesh mesh;

    /// <summary>
    /// A list of each of the child nodes.
    /// </summary>
    private List<FieldNode_Base> children = new List<FieldNode_Base>();

    /// <summary>
    /// Adds a child FieldNode_Base to the children.
    /// </summary>
    /// <param name="node"></param>
    public void AddChild(FieldNode_Base node)
    {
        children.Add(node);
    }

    /// <summary>
    /// Returns the list of children.
    /// </summary>
    /// <returns></returns>
    public List<FieldNode_Base> GetChildren()
    {
        return children;
    }

    /// <summary>
    /// Combines the SubMeshes of each child and combines it into the mesh to be exported.
    /// </summary>
    public void CreateMesh()
    {
        mesh = new BXDAMesh();
        foreach (FieldNode_Base node in children)
        {
            mesh.meshes.AddRange(node.GetSubMeshes());
        }
    }

    /// <summary>
    /// Returns the mesh to be exported.
    /// </summary>
    /// <returns></returns>
    public BXDAMesh GetMeshOutput()
    {
        return mesh;
    }
}
