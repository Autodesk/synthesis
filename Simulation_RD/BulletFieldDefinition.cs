using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using BulletSharp;

namespace Simulation_RD
{
    /// <summary>
    /// Field mesh definition for the Bullet world
    /// </summary>
    class BulletFieldDefinition : FieldDefinition
    {
        const bool debug = true;

        private BulletFieldDefinition(Guid guid, string name) : base(guid, name) { }

        /// <summary>
        /// Bullet definition of the collision mesh
        /// </summary>
        public RigidBody BulletObject;

        private void CreateMesh(string filePath)
        {
            BXDAMesh mesh = new BXDAMesh();
            mesh.ReadFromFile(filePath);

            CompoundShape shape = new CompoundShape();
            
            foreach(FieldNode node in NodeGroup.EnumerateAllLeafFieldNodes())
            {
                /* //Materials stuff. Ignore for now. Bullet materials are a mess.
                TriangleIndexVertexMaterialArray mats = new TriangleIndexVertexMaterialArray();
                if(node.SubMeshID != -1)
                {
                    BXDAMesh.BXDASubMesh subMesh = mesh.meshes[node.SubMeshID];
                    for(int i = 0; i < mats.Length; i++)
                    {
                        MaterialProperties m = new MaterialProperties();
                        m.
                        mats.AddMaterialProperties()
                    }
                }
                */

                if (GetPropertySets().ContainsKey(node.PropertySetID))
                {
                    PropertySet current = GetPropertySets()[node.PropertySetID];
                    CollisionShape subShape = null;
                    switch (current.Collider.CollisionType)
                    {
                        case PropertySet.PropertySetCollider.PropertySetCollisionType.BOX:
                            {
                                //Create a box shape
                                PropertySet.BoxCollider colliderInfo = (PropertySet.BoxCollider)current.Collider;
                                subShape = new BoxShape(colliderInfo.Scale.x, colliderInfo.Scale.y, colliderInfo.Scale.z);
                                //if (debug) Console.WriteLine("Created Box");
                                break;
                            }
                        case PropertySet.PropertySetCollider.PropertySetCollisionType.SPHERE:
                            {
                                //Create a sphere shape
                                PropertySet.SphereCollider colliderInfo = (PropertySet.SphereCollider)current.Collider;
                                subShape = new SphereShape(colliderInfo.Scale);
                                //if (debug) Console.WriteLine("Created Sphere");
                                break;
                            }
                        case PropertySet.PropertySetCollider.PropertySetCollisionType.MESH:
                            {
                                //Create a mesh shape
                                if(node.CollisionMeshID != -1)
                                {
                                    PropertySet.MeshCollider colliderInfo = (PropertySet.MeshCollider)current.Collider;
                                    Vector3[] vertices = MeshUtilities.DataToVector(mesh.colliders[node.CollisionMeshID].verts);

                                    if (colliderInfo.Convex)
                                    {
                                        subShape = new ConvexHullShape(vertices);
                                        //if (debug) Console.WriteLine("Created Convex Mesh");
                                    }
                                    else
                                    {
                                        TriangleMesh tMesh = MeshUtilities.BulletShapeFromSubMesh(mesh.colliders[node.CollisionMeshID], vertices);
                                        subShape = new BvhTriangleMeshShape(tMesh, false);
                                        //if (debug) Console.WriteLine("Created Concave Mesh");
                                    }
                                }
                                break;
                            }
                    }
                    Console.WriteLine("Created " + node.NodeID);
                    
                    if (null != subShape)
                    {
                        //set sub shape local position/rotation and add it to the compound shape
                        Vector3 Translation = new Vector3(node.Position.x, node.Position.y, node.Position.z);
                        Quaternion rotation = new Quaternion(node.Rotation.X, node.Rotation.Y, node.Rotation.Z, node.Rotation.W);
                        shape.AddChildShape(Matrix4.CreateTranslation(Translation) * Matrix4.CreateFromQuaternion(rotation), shape);
                    }
                    
                }
            }

            DefaultMotionState m = new DefaultMotionState();
            RigidBodyConstructionInfo rb = new RigidBodyConstructionInfo(mesh.physics.mass, m, shape);
            BulletObject = new RigidBody(rb);
        }

        /// <summary>
        /// Creates a new BulletFieldDefinition from a set of .bxda and .bxdf files
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static BulletFieldDefinition FromFile(string path)
        {
            Factory = delegate (Guid guid, string name) { return new BulletFieldDefinition(guid, name); };
            BulletFieldDefinition toReturn = (BulletFieldDefinition)BXDFProperties.ReadProperties(path + "definition.bxdf");
            toReturn.CreateMesh(path + "mesh.bxda");
            return toReturn;
        }
    }
}
