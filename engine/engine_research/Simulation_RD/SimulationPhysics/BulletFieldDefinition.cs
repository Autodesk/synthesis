using System;
using System.Collections.Generic;
using OpenTK;
using BulletSharp;
using Simulation_RD.Graphics;
using Simulation_RD.Utility;

namespace Simulation_RD.SimulationPhysics
{
    /// <summary>
    /// Field mesh definition for the Bullet world
    /// </summary>
    class BulletFieldDefinition : FieldDefinition
    {
        const bool debug = false;

        private BulletFieldDefinition(Guid guid, string name) : base(guid, name) { }

        /// <summary>
        /// Bullet definition of the collision mesh
        /// </summary>
        public List<RigidBody> Bodies;

        /// <summary>
        /// More detailed meshes for drawing
        /// </summary>
        public List<Mesh> VisualMeshes;

        private void CreateMesh(string filePath)
        {
            Bodies = new List<RigidBody>();
            VisualMeshes = new List<Mesh>();

            BXDAMesh mesh = new BXDAMesh();
            mesh.ReadFromFile(filePath);
                        
            foreach(FieldNode node in NodeGroup.EnumerateAllLeafFieldNodes())
            {                
                if (GetPropertySets().ContainsKey(node.PropertySetID))
                {
                    PropertySet current = GetPropertySets()[node.PropertySetID];
                    CollisionShape subShape = null;
                    switch (current.Collider.CollisionType)
                    {
                        case PropertySet.PropertySetCollider.PropertySetCollisionType.BOX:
                            {
                                //Create a box shape
                                //This is a mess, though I was told that this is how it works
                                Vector3[] vertices = MeshUtilities.DataToVector(mesh.meshes[node.SubMeshID].verts);
                                StridingMeshInterface temp = MeshUtilities.BulletShapeFromSubMesh(mesh.meshes[node.SubMeshID]);
                                Vector3 min, max;
                                temp.CalculateAabbBruteForce(out min, out max);
                                
                                PropertySet.BoxCollider colliderInfo = (PropertySet.BoxCollider)current.Collider;
                                subShape = new BoxShape((max - min) * colliderInfo.Scale.Convert() * 0.5f);
                                if (debug) Console.WriteLine("Created Box");
                                break;
                            }
                        case PropertySet.PropertySetCollider.PropertySetCollisionType.SPHERE:
                            {
                                //Create a sphere shape
                                PropertySet.SphereCollider colliderInfo = (PropertySet.SphereCollider)current.Collider;
                                subShape = new SphereShape(colliderInfo.Scale);
                                if (debug) Console.WriteLine("Created Sphere");
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
                                        if (debug) Console.WriteLine("Created Convex Mesh");
                                    }
                                    else
                                    {
                                        StridingMeshInterface sMesh = MeshUtilities.BulletShapeFromSubMesh(mesh.colliders[node.CollisionMeshID]);
                                        subShape = new ConvexTriangleMeshShape(sMesh, true);
                                        if (debug) Console.WriteLine("Created Concave Mesh");
                                    }
                                }
                                break;
                            }
                    }
                    
                    if (null != subShape)
                    {
                        //set sub shape local position/rotation and add it to the compound shape
                        Vector3 Translation = node.Position.Convert();
                        Quaternion rotation = node.Rotation.Convert();
                        
                        DefaultMotionState motion = new DefaultMotionState(Matrix4.CreateFromQuaternion(rotation) * Matrix4.CreateTranslation(Translation));
                        motion.CenterOfMassOffset = Matrix4.CreateTranslation(mesh.physics.centerOfMass.Convert());
                        
                        RigidBodyConstructionInfo info = new RigidBodyConstructionInfo(current.Mass, motion, subShape, subShape.CalculateLocalInertia(current.Mass));
                        info.Friction = current.Friction;
                        Bodies.Add(new RigidBody(info));
                        
                        VisualMeshes.Add(new Mesh(mesh.meshes[node.SubMeshID], Translation));                        
                        if(debug) Console.WriteLine("Created " + node.PropertySetID);
                    }                    
                }
            }
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
