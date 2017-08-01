using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

public class FieldDefinition
{
    /// <summary>
    /// Used for creating new instances of a FieldDefinition.
    /// </summary>
    /// <param name="guid"></param>
    /// <param name="nodeGroupName"></param>
    /// <returns></returns>
    public delegate FieldDefinition FieldDefinitionFactory(Guid guid, string name = BXDFProperties.BXDF_DEFAULT_NAME);

    /// <summary>
    /// The default delegate for creating new FieldDefinition instances.
    /// </summary>
    public static FieldDefinitionFactory Factory = delegate(Guid guid, string name)
    {
        return new FieldDefinition(guid, name);
    };

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
    public FieldNodeGroup NodeGroup
    {
        get;
        private set;
    }

    /// <summary>
    /// A dictionary containing each PropertySet and a string identifier.
    /// </summary>
    private Dictionary<string, PropertySet> propertySets;

    /// <summary>
    /// The mesh to be exported.
    /// </summary>
    private BXDAMesh mesh;

    /// <summary>
    /// Initailizes a new instance of the FieldDefinition class.
    /// </summary>
    /// <param name="guid"></param>
    protected FieldDefinition(Guid guid, string name)
    {
        GUID = guid;
        NodeGroup = new FieldNodeGroup(name);
        propertySets = new Dictionary<string, PropertySet>();
        mesh = new BXDAMesh(GUID);
    }

    /// <summary>
    /// Adds a child PhysicsGroup to physicsGroups.
    /// </summary>
    /// <param name="name"></param>
    /// <param name="group"></param>
    public void AddPropertySet(PropertySet group)
    {
        propertySets.Add(group.PropertySetID, group);
    }

    /// <summary>
    /// Returns a Dictionary containing each PhysicsGroup.
    /// </summary>
    /// <returns></returns>
    public Dictionary<string, PropertySet> GetPropertySets()
    {
        return propertySets;
    }

    /// <summary>
    /// Used for adding a submesh and creating a sub mesh ID for the given node.
    /// </summary>
    /// <param name="subMesh"></param>
    /// <param name="node"></param>
    public void AddSubMesh(BXDAMesh.BXDASubMesh subMesh)
    {
        mesh.meshes.Add(subMesh);
    }

    /// <summary>
    /// Used for adding a collision mesh and creating a collision mesh ID for the given node.
    /// </summary>
    /// <param name="collisionMesh"></param>
    /// <param name="node"></param>
    public void AddCollisionMesh(BXDAMesh.BXDASubMesh collisionMesh)
    {
        mesh.colliders.Add(collisionMesh);
    }

    /// <summary>
    /// Used for getting a submesh from the given ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public BXDAMesh.BXDASubMesh GetSubMesh(int id)
    {
        return mesh.meshes[id];
    }

    /// <summary>
    /// Used for getting a collision mesh from the given ID.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public BXDAMesh.BXDASubMesh GetCollisionMesh(int id)
    {
        return mesh.colliders[id];
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