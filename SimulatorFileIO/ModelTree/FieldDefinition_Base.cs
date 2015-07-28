using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

/// <summary>
/// Stores the type of collision for the node. Extends byte for writability.
/// </summary>
public enum PhysicsGroupCollisionType : byte
{
    /// <summary>
    /// No collision.
    /// </summary>
    NONE = 1,

    /// <summary>
    /// Mesh collider.
    /// </summary>
    MESH = 2,

    /// <summary>
    /// Box collider.
    /// </summary>
    BOX = 3
}

/// <summary>
/// Stores physical properties for a node or group of nodes.
/// </summary>
public struct PhysicsGroup
{
    /// <summary>
    /// ID of the PhysicsGroup.
    /// </summary>
    public string physicsGroupID;

    /// <summary>
    /// Collision type of the PhysicsGroup.
    /// </summary>
    public PhysicsGroupCollisionType collisionType;

    /// <summary>
    /// Friction value of the PhysicsGroup.
    /// </summary>
    public int friction;

    /// <summary>
    /// Constructs a new PhysicsGroup with the specified values.
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="type"></param>
    /// <param name="frictionValue"></param>
    public PhysicsGroup(string ID, PhysicsGroupCollisionType type, int frictionValue)
    {
        physicsGroupID = ID;
        collisionType = type;
        friction = frictionValue;
    }
}

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
    /// A dictionary containing each PhysicsGroup and a string identifier.
    /// </summary>
    private Dictionary<string, PhysicsGroup> physicsGroups = new Dictionary<string,PhysicsGroup>();

    /// <summary>
    /// A list of each of the child nodes.
    /// </summary>
    private List<FieldNode_Base> children = new List<FieldNode_Base>();

    /// <summary>
    /// The mesh to be exported.
    /// </summary>
    private BXDAMesh mesh;

    /// <summary>
    /// Adds a child PhysicsGroup to physicsGroups;
    /// </summary>
    /// <param name="name"></param>
    /// <param name="group"></param>
    public void AddPhysicsGroup(PhysicsGroup group)
    {
        physicsGroups.Add(group.physicsGroupID, group);
    }

    /// <summary>
    /// Returns a Dictionary containing each PhysicsGroup.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, PhysicsGroup> GetPhysicsGroups()
    {
        return physicsGroups;
    }

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
