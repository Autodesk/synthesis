using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BulletSharp;
using BulletSharp.SoftBody;
using OpenTK;

namespace Simulation_RD
{
    class BulletRigidNode : RigidNode_Base
    {
        /// <summary>
        /// Defines collision mesh. Not explicitly a <see cref="BulletSharp.RigidBody"/> because this might be a soft body.
        /// </summary>
        public RigidBody BulletObject;

        public BulletRigidNode(Guid guid) : base(guid) { }

        /// <summary>
        /// Creates a Rigid Body from a .bxda file
        /// </summary>
        /// <param name="FilePath"></param>
        public void CreateRigidBody(string FilePath)
        {
            BXDAMesh mesh = new BXDAMesh();
            mesh.ReadFromFile(FilePath);

            //Rigid Body Construction
            DefaultMotionState motion = new DefaultMotionState(Matrix4.CreateTranslation(0, 10, 0));
            RigidBodyConstructionInfo info = new RigidBodyConstructionInfo(mesh.physics.mass, motion, GetShape(mesh));
            BulletObject = new RigidBody(info);
        }

        /// <summary>
        /// Creates a Soft body from a .bxda file [NOT YET PROPERLY IMPLEMENTED (I THINK)]
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="worldInfo"></param>
        public void CreateSoftBody(string filePath, SoftBodyWorldInfo worldInfo)
        {
            BXDAMesh mesh = new BXDAMesh();
            mesh.ReadFromFile(filePath);

            //Soft body construction
            //BulletObject = new SoftBody(worldInfo);
            BulletObject.CollisionShape = GetShape(mesh);
        }

        private static CompoundShape GetShape(BXDAMesh mesh)
        {
            CompoundShape shape = new CompoundShape();

            for (int i = 0; i < mesh.colliders.Count; i++)
            {
                BXDAMesh.BXDASubMesh sub = mesh.colliders[i];
                Vector3[] vertices = MeshUtilities.DataToVector(sub.verts);
                StridingMeshInterface sMesh = MeshUtilities.BulletShapeFromSubMesh(mesh.colliders[i], vertices);

                //I don't believe there are any transformations necessary here.
                shape.AddChildShape(Matrix4.Zero, new GImpactMeshShape(sMesh));
                //Console.WriteLine("Successfully created and added sub shape");
            }

            return shape;
        }
    }
}
