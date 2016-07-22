using System;
using BulletSharp;
using BulletSharp.SoftBody;
using OpenTK;

namespace Simulation_RD.SimulationPhysics
{
    /// <summary>
    /// Defines a robot joint
    /// </summary>
    class BulletRigidNode : RigidNode_Base
    {
        /// <summary>
        /// Defines collision mesh.
        /// </summary>
        public CollisionObject BulletObject;
        public Action<float> Update;

        public TypedConstraint joint;

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
            CollisionShape shape = GetShape(mesh);
            RigidBodyConstructionInfo info = new RigidBodyConstructionInfo(mesh.physics.mass, motion, shape, shape.CalculateLocalInertia(mesh.physics.mass));
            BulletObject = new RigidBody(info);
        }

        /// <summary>
        /// Creates a Soft body from a .bxda file [NOT YET PROPERLY IMPLEMENTED?]
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="worldInfo"></param>
        public void CreateSoftBody(string filePath, SoftBodyWorldInfo worldInfo)
        {
            BXDAMesh mesh = new BXDAMesh();
            mesh.ReadFromFile(filePath);
            
            //Soft body construction
            //BulletObject = new SoftBody(worldInfo);
            foreach(BXDAMesh.BXDASubMesh sub in mesh.colliders)
            {
                SoftBody temp = SoftBodyHelpers.CreateFromConvexHull(worldInfo, MeshUtilities.DataToVector(sub.verts));
                temp.WorldTransform += Matrix4.CreateTranslation(0, 10, 0);
                BulletObject = temp;
            }
        }

        /// <summary>
        /// Creates the joint data
        /// </summary>
        public void CreateJoint()
        {
            if (joint != null || GetSkeletalJoint() == null)
            {
                return;
            }

            switch (GetSkeletalJoint().GetJointType())
            {
                case SkeletalJointType.ROTATIONAL:
                    RotationalJoint_Base nodeR = (RotationalJoint_Base)GetSkeletalJoint();
                    CollisionObject parentObject = ((BulletRigidNode)GetParent()).BulletObject;

                    HingeConstraint temp = new HingeConstraint(
                        (RigidBody)BulletObject, 
                        (RigidBody)parentObject, 
                        nodeR.basePoint.Convert(), 
                        nodeR.basePoint.Convert(),
                        nodeR.axis.Convert(),
                        nodeR.axis.Convert());

                    joint = temp;
                    if(nodeR.hasAngularLimit)
                        temp.SetLimit(nodeR.angularLimitLow, nodeR.angularLimitHigh);

                    Update = (f) => { temp.EnableMotor = true; temp.EnableAngularMotor(true, f, 10f); };

                    Console.WriteLine("Rotational/Wheel joint made");
                    break;
                default:
                    Console.WriteLine("Received joint of type {0}", GetSkeletalJoint().GetJointType());
                    break;
            }            
        }

        private static CompoundShape GetShape(BXDAMesh mesh)
        {
            CompoundShape shape = new CompoundShape();

            for (int i = 0; i < mesh.colliders.Count; i++)
            {
                BXDAMesh.BXDASubMesh sub = mesh.colliders[i];
                Vector3[] vertices = MeshUtilities.DataToVector(sub.verts);
                StridingMeshInterface sMesh = MeshUtilities.BulletShapeFromSubMesh(sub, vertices);

                //I don't believe there are any transformations necessary here.
                shape.AddChildShape(Matrix4.Identity, new ConvexTriangleMeshShape(sMesh));
                //Console.WriteLine("Successfully created and added sub shape");                
            }

            return shape;
        }
    }
}
