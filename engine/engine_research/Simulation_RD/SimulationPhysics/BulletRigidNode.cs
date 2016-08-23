using System;
using BulletSharp;
using BulletSharp.SoftBody;
using OpenTK;
using Simulation_RD.Utility;
using System.Collections.Generic;
using System.Linq;

namespace Simulation_RD.SimulationPhysics
{
    /// <summary>
    /// Defines a robot jointad connected body
    /// </summary>
    class BulletRigidNode : RigidNode_Base
    {
        /// <summary>
        /// Writes some stuff to the console
        /// </summary>
        private const bool debug = true;

        /// <summary>
        /// Defines the Bullet collision object. Might be able to be a soft body in the future
        /// </summary>
        public RigidBody BulletObject;

        /// <summary>
        /// makes joint do. A better way of doing this really should be found.
        /// </summary>
        public Action<float> Update;

        /// <summary>
        /// Bullet Joint data
        /// </summary>
        public TypedConstraint joint;

        public BulletRigidNode(Guid guid) : base(guid) { }

        /// <summary>
        /// Creates a Rigid Body from a .bxda file
        /// </summary>
        /// <param name="FilePath"></param>
        public void CreateRigidBody(string FilePath)
        {
            CollisionShape shape;
            WheelDriverMeta wheel = null;
            DefaultMotionState motion;
            BXDAMesh mesh = new BXDAMesh();
            mesh.ReadFromFile(FilePath);
            Vector3 loc;
            Quaternion rot = Quaternion.Identity;

            //Is it a wheel?
            if ((wheel = GetSkeletalJoint()?.cDriver?.GetInfo<WheelDriverMeta>()) != null && true)
            {
                //Align the cylinders
                Vector3 min, max;
                GetShape(mesh).GetAabb(Matrix4.Identity, out min, out max);
                Vector3 extents = max - min;

                //Find the thinnest dimension, that is probably wheat the cylinder should be aligned to
                if(extents.X < extents.Y) //X or Z
                {
                    if (extents.X < extents.Z)
                        shape = new CylinderShapeX(wheel.width, wheel.radius, wheel.radius); //X
                    else
                        shape = new CylinderShapeZ(wheel.radius, wheel.radius, wheel.width); //Z
                }
                else //Y or Z
                {
                    if (extents.Y < extents.Z)
                        shape = new CylinderShape(wheel.radius, wheel.width, wheel.radius); //Y
                    else
                        shape = new CylinderShapeZ(wheel.radius, wheel.radius, wheel.width); //Z
                }
                
                loc = MeshUtilities.MeshCenter(mesh);
            }
            //Rigid Body Construction
            else
            {
                shape = GetShape(mesh);
                loc = MeshUtilities.MeshCenter(mesh);
            }
            
            if(debug) Console.WriteLine("Rotation is " + rot);

            motion = new DefaultMotionState(Matrix4.CreateTranslation(loc + new Vector3(0, 100, 0)));
            RigidBodyConstructionInfo info = new RigidBodyConstructionInfo(mesh.physics.mass, motion, shape, shape.CalculateLocalInertia(mesh.physics.mass));

            //Temp?
            info.Friction = 100;
            info.RollingFriction = 100;
            //info.AngularDamping = 0f;
            //info.LinearDamping = 0.5f;

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
            List<Vector3> verts = new List<Vector3>();

            //Soft body construction
            verts = mesh.AllColliderVertices().ToList();

            SoftBody temp = SoftBodyHelpers.CreateFromConvexHull(worldInfo, verts.ToArray());
            //BulletObject = temp;
        }

        /// <summary>
        /// Creates the joint data
        /// </summary>
        public void CreateJoint()
        {
            if (joint != null || GetSkeletalJoint() == null)
                return;
            
            switch (GetSkeletalJoint().GetJointType())
            {
                case SkeletalJointType.ROTATIONAL:
                    RotationalJoint_Base nodeR = (RotationalJoint_Base)GetSkeletalJoint();
                    CollisionObject parentObject = ((BulletRigidNode)GetParent()).BulletObject;
                    WheelDriverMeta wheel = GetSkeletalJoint().cDriver.GetInfo<WheelDriverMeta>();

                    Vector3 pivot = nodeR.basePoint.Convert();
                    
                    Matrix4 locJ, locP; //Local Joint Pivot, Local Parent Pivot

                    BulletObject.WorldTransform = parentObject.WorldTransform * Matrix4.CreateTranslation(pivot);

                    if(debug) Console.WriteLine("Pivot at " + pivot);

                    GetFrames(nodeR.basePoint.Convert(), parentObject.WorldTransform, BulletObject.WorldTransform, out locP, out locJ);

                    //HingeConstraint temp = new HingeConstraint((RigidBody)parentObject, /*(RigidBody)*/BulletObject, locP, locJ);
                    HingeConstraint temp = new HingeConstraint((RigidBody)parentObject, BulletObject, pivot, Vector3.Zero, nodeR.axis.Convert(), nodeR.axis.Convert());
                    joint = temp;
                    temp.SetAxis(nodeR.axis.Convert());

                    if (nodeR.hasAngularLimit)
                        temp.SetLimit(nodeR.angularLimitLow, nodeR.angularLimitHigh);

                    //also need to find a less screwy way to do this
                    Update = (f) => { (/*(RigidBody)*/BulletObject).ApplyTorque(nodeR.axis.Convert() * f * 25); };

                    if(debug)
                        Console.WriteLine("{0} joint made", wheel == null ? "Rotational" : "Wheel");
                    break;
                default:
                    if(debug)
                        Console.WriteLine("Received joint of type {0}", GetSkeletalJoint().GetJointType());
                    break;
            }            
        }

        /// <summary>
        /// Turns a BXDA mesh into a CompoundShape centered around the origin
        /// </summary>
        /// <param name="mesh"></param>
        /// <returns></returns>
        private static CompoundShape GetShape(BXDAMesh mesh)
        {
            CompoundShape shape = new CompoundShape();
            Vector3[] meshVertices = mesh.AllColliderVertices().ToArray();

            for (int i = 0; i < mesh.colliders.Count; i++)
            {
                BXDAMesh.BXDASubMesh sub = mesh.colliders[i];
                Vector3[] vertices = sub.GetVertexData();
                StridingMeshInterface sMesh = MeshUtilities.CenteredBulletShapeFromSubMesh(sub);

                //Add the shape at a location relative to the compound shape such that the compound shape is centered at (0, 0) but child shapes are properly placed
                shape.AddChildShape(Matrix4.CreateTranslation(MeshUtilities.MeshCenterRelative(sub, mesh)), new ConvexTriangleMeshShape(sMesh));
                Console.WriteLine("Successfully created and added sub shape");                
            }

            return shape;
        }

        /// <summary>
        /// Gets the pivot/axis joint for each rigid body for a rotational joint
        /// </summary>
        /// <param name="jointPivot">joint pivot for parent</param>
        /// <param name="jointTransform">world transform for the child object</param>
        /// <param name="parentTransform">world transform for the parent object</param>
        /// <param name="parentFrame">Matrix to be assigned to the joint's rotational frame</param>
        /// <param name="jointFrame">Matrix to be assigned to the parent's rotational frame</param>
        private static void GetFrames(Vector3 jointPivot, Matrix4 parentTransform, Matrix4 jointTransform, out Matrix4 parentFrame, out Matrix4 jointFrame)
        {
            parentFrame = Matrix4.CreateTranslation(jointPivot);
            jointFrame = parentFrame * parentTransform * jointTransform.Inverted();
        }
    }
}
