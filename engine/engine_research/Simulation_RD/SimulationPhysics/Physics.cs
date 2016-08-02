using System;
using System.Collections.Generic;
using BulletSharp;
using BulletSharp.SoftBody;
using OpenTK;
using OpenTK.Input;
using Simulation_RD.GameFeatures;

namespace Simulation_RD.SimulationPhysics
{
    /// <summary>
    /// Handles all of the Physics of the environment
    /// </summary>
    class Physics
    {
        public SoftRigidDynamicsWorld World { get; set; }
        public KeyboardKeyEventArgs cachedArgs = new KeyboardKeyEventArgs();
        CollisionDispatcher dispatcher;
        DbvtBroadphase broadphase;
        List<CollisionShape> collisionShapes = new List<CollisionShape>();
        CollisionConfiguration collisionConf;
        SoftBodySolver solver;
        public BulletFieldDefinition f;
        public BulletRigidNode Skeleton; //3spooky5me
        private Action OnUpdate;
                
        /// <summary>
        /// Default Constructor
        /// </summary>
        public Physics()
        {
            DantzigSolver mlcp = new DantzigSolver();
            collisionConf = new SoftBodyRigidBodyCollisionConfiguration();
            dispatcher = new CollisionDispatcher(new DefaultCollisionConfiguration());
            solver = new DefaultSoftBodySolver();
            ConstraintSolver cSolver = new MlcpSolver(mlcp);

            broadphase = new DbvtBroadphase();
            World = new SoftRigidDynamicsWorld(dispatcher, broadphase, cSolver, collisionConf, solver);
            
            World.Gravity = new Vector3(0, -9.81f, 0);
            World.SetInternalTickCallback(new DynamicsWorld.InternalTickCallback((w, f) => DriveJoints.UpdateAllMotors(Skeleton, cachedArgs)));

            //Roobit
            string RobotPath = @"C:\Program Files (x86)\Autodesk\Synthesis\Synthesis\Robots\Sample Robot\";
            RigidNode_Base.NODE_FACTORY = (Guid guid) => new BulletRigidNode(guid);
            Skeleton = (BulletRigidNode)BXDJSkeleton.ReadSkeleton(RobotPath + "skeleton.bxdj");
            List<RigidNode_Base> nodes = Skeleton.ListAllNodes();
            for(int i = 0; i < nodes.Count; i++)
            {
                BulletRigidNode bNode = (BulletRigidNode)nodes[i];
                bNode.CreateRigidBody(RobotPath + bNode.ModelFileName);
                bNode.CreateJoint();

                if (bNode.joint != null)
                    World.AddConstraint(bNode.joint, true);
                World.AddCollisionObject(bNode.BulletObject);
                collisionShapes.Add(bNode.BulletObject.CollisionShape);
            }

            //Field
            f = BulletFieldDefinition.FromFile(@"C:\Program Files (x86)\Autodesk\Synthesis\Synthesis\Fields\2010\");
            foreach (RigidBody b in f.Bodies)
            {
                World.AddRigidBody(b);
                collisionShapes.Add(b.CollisionShape);
            }

            World.StepSimulation(0.1f, 100);
        }

        /// <summary>
        /// Steps the world
        /// </summary>
        /// <param name="elapsedTime">elapsed time</param>
        public virtual void Update(float elapsedTime, KeyboardKeyEventArgs args)
        {
            //DriveJoints.UpdateAllMotors(Skeleton, args);
            cachedArgs = args;
            if (Controls.GameControls[Controls.Control.ResetRobot] == args.Key) { }
                //ResetRobot();
            //World.StepSimulation(elapsedTime, 100);
            World.StepSimulation(elapsedTime, 1000, 1f / 300f);
            OnUpdate?.Invoke();
        }

        /// <summary>
        /// Disposes all of the objects and the world, essentially a destructor
        /// </summary>
        public void ExitPhysics()
        {
            int i;

            for (i = World.NumConstraints - 1; i >= 0; i--)
            {
                TypedConstraint constraint = World.GetConstraint(i);
                World.RemoveConstraint(constraint);
                constraint.Dispose();
            }

            for (i = World.NumCollisionObjects - 1; i >= 0; i--)
            {
                CollisionObject obj = World.CollisionObjectArray[i];
                RigidBody body = obj as RigidBody;
                if (body != null && body.MotionState != null)
                {
                    body.MotionState.Dispose();
                }
                World.RemoveCollisionObject(obj);
                obj.Dispose();
            }

            foreach (CollisionShape shape in collisionShapes) shape.Dispose();
            collisionShapes.Clear();

            World.Dispose();
            broadphase.Dispose();
            if (dispatcher != null)
            {
                dispatcher.Dispose();
            }
            collisionConf.Dispose();
        }

        /// <summary>
        /// Tries to reset the robot
        /// </summary>
        public void ResetRobot()
        {
            if (Skeleton == null)
                return;

            List<BulletRigidNode> Wheels = new List<BulletRigidNode>();

            foreach(RigidNode_Base node in Skeleton.ListAllNodes())
            {
                BulletRigidNode bNode = (BulletRigidNode)node;

                if (bNode.BulletObject == null)
                    continue;
                
                WheelDriverMeta wheel;
                if ((wheel = bNode.GetSkeletalJoint()?.cDriver?.GetInfo<WheelDriverMeta>()) != null)
                    Wheels.Add(bNode);

                Extensions.AuxFunctions.OrientRobot(Wheels, Skeleton.BulletObject);
                
                bNode.BulletObject.WorldTransform = Matrix4.CreateTranslation(0, 10, 0);
                bNode.BulletObject.InterpolationLinearVelocity = Vector3.Zero;
                bNode.BulletObject.InterpolationAngularVelocity = Vector3.Zero;                
            }
        }        
    }
}
