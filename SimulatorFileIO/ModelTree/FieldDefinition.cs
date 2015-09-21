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
    public string PhysicsGroupID;

    /// <summary>
    /// Collision type of the PhysicsGroup.
    /// </summary>
    public PhysicsGroupCollisionType CollisionType;

    /// <summary>
    /// Friction value of the PhysicsGroup.
    /// </summary>
    public int Friction;

    /// <summary>
    /// Stores the mass of the object (only has effect when PhyicsGroup is dynamic).
    /// </summary>
    public double Mass;

    /// <summary>
    /// Constructs a new PhysicsGroup with the specified values.
    /// </summary>
    /// <param name="ID"></param>
    /// <param name="type"></param>
    /// <param name="frictionValue"></param>
    public PhysicsGroup(string physicsGroupID, PhysicsGroupCollisionType collisionType, int friction, double mass = 0.0)
    {
        this.PhysicsGroupID = physicsGroupID;
        this.CollisionType = collisionType;
        this.Friction = friction;
        this.Mass = mass;
    }
}

public class FieldDefinition
{
    /// <summary>
    /// The globally unique identifier.
    /// </summary>
    public Guid GUID
    {
        get;
        private set;
    }

    /// <summary>
    /// The group containing each child node.
    /// </summary>
    public FieldNodeGroup NodeGroup;

    /// <summary>
    /// A dictionary containing each PhysicsGroup and a string identifier.
    /// </summary>
    private Dictionary<string, PhysicsGroup> physicsGroups = new Dictionary<string, PhysicsGroup>();

    /// <summary>
    /// The mesh to be exported.
    /// </summary>
    private BXDAMesh mesh;

    /// <summary>
    /// Initailizes a new instance of the FieldDefinition class.
    /// </summary>
    /// <param name="guid"></param>
    public FieldDefinition(Guid guid)
    {
        GUID = guid;
        NodeGroup = new FieldNodeGroup("NodeGroup");
    }

    /// <summary>
    /// Initializes a new instance of the FieldDefinition class.
    /// </summary>
    /// <param name="definitionID"></param>
    public FieldDefinition(Guid guid, string nodeGroupName)
    {
        GUID = guid;
        NodeGroup = new FieldNodeGroup(nodeGroupName);
    }

    /// <summary>
    /// Adds a child PhysicsGroup to physicsGroups.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="group"></param>
    public void AddPhysicsGroup(PhysicsGroup group)
    {
        physicsGroups.Add(group.PhysicsGroupID, group);
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
        int currentMeshID = 0;

        foreach (FieldNode node in NodeGroup.EnumerateAllLeafFieldNodes())
        {
            mesh.meshes.Add(node.SubMesh);
            node.MeshID = currentMeshID;
            currentMeshID++;
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
