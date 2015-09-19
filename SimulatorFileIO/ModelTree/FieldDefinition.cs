using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

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
    /// Determines if the PhysicsGroup is dynamic (can be pushed around and stuff).
    /// </summary>
    public bool dynamic;

    /// <summary>
    /// Stores the mass of the object (only has effect when PhyicsGroup is dynamic).
    /// </summary>
    public double mass;

    /// <summary>
    /// Constructs a new PhysicsGroup with the specified values.
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="type"></param>
    /// <param name="frictionValue"></param>
    public PhysicsGroup(string physicsGroupID, PhysicsGroupCollisionType collisionType, int friction, bool dynamic, double mass = 0.0)
    {
        this.physicsGroupID = physicsGroupID;
        this.collisionType = collisionType;
        this.friction = friction;
        this.dynamic = dynamic;
        this.mass = mass;
    }
}

public class FieldDefinition
{
    /// <summary>
    /// Stores the ID of this definition (for naming, labels, etc.)
    /// </summary>
    public string DefinitionID
    {
        get;
        private set;
    }

    /// <summary>
    /// The group containing each child node.
    /// </summary>
    public FieldNodeGroup NodeGroup
    {
        get;
        private set;
    }

    /// <summary>
    /// A dictionary containing each PhysicsGroup and a string identifier.
    /// </summary>
    private Dictionary<string, PhysicsGroup> physicsGroups = new Dictionary<string, PhysicsGroup>();

    /// <summary>
    /// The mesh to be exported.
    /// </summary>
    private BXDAMesh mesh;

    /// <summary>
    /// Initializes a new instance of the FieldDefinition class.
    /// </summary>
    /// <param name="definitionID"></param>
    public FieldDefinition(string definitionID)
    {
        DefinitionID = definitionID;
        NodeGroup = new FieldNodeGroup(definitionID);
    }

    /// <summary>
    /// Adds a child PhysicsGroup to physicsGroups.
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
    /// Combines the SubMeshes of each child and combines it into the mesh to be exported.
    /// </summary>
    public void CreateMesh()
    {
        mesh = new BXDAMesh();
       
        foreach (FieldNode node in NodeGroup.EnumerateFieldNodes())
            mesh.meshes.AddRange(node.GetSubMeshes());
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
