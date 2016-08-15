using System;
using System.Collections.Generic;
using System.Linq;
using BulletSharp;
using BulletSharp.SoftBody;
using OpenTK;
using OpenTK.Input;
using Simulation_RD.GameFeatures;
using Simulation_RD.Utility;

namespace Simulation_RD.SimulationPhysics
{
    /// <summary>
    /// Handles all of the Physics of the environment
    /// </summary>
    class Physics
    {
        float angle;

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

            //Actual scaling is unknown, this gravity is probably not right
            World.Gravity = new Vector3(0, -98.1f, 0);
            World.SetInternalTickCallback(new DynamicsWorld.InternalTickCallback((w, f) => DriveJoints.UpdateAllMotors(Skeleton, cachedArgs)));

            //Roobit
            RigidNode_Base.NODE_FACTORY = (Guid guid) => new BulletRigidNode(guid);
            string RobotPath = @"C:\Program Files (x86)\Autodesk\Synthesis\Synthesis\Robots\";
            Exception ex;
            string dir = RobotPath;

            GetFromDirectory(RobotPath, s => { Skeleton = (BulletRigidNode)BXDJSkeleton.ReadSkeleton(s + "skeleton.bxdj"); dir = s; });
            List<RigidNode_Base> nodes = Skeleton.ListAllNodes();
            for(int i = 0; i < nodes.Count; i++)
            {
                BulletRigidNode bNode = (BulletRigidNode)nodes[i];
                bNode.CreateRigidBody(dir + bNode.ModelFileName);
                bNode.CreateJoint();

                if (bNode.joint != null)
                    World.AddConstraint(bNode.joint, true);
                World.AddCollisionObject(bNode.BulletObject);
                collisionShapes.Add(bNode.BulletObject.CollisionShape);
            }

            //Field
            string fieldPath = @"C:\Program Files (x86)\Autodesk\Synthesis\Synthesis\Fields\";
            GetFromDirectory(fieldPath, s => f = BulletFieldDefinition.FromFile(s));

            foreach (RigidBody b in f.Bodies)
            {
                World.AddRigidBody(b);
                collisionShapes.Add(b.CollisionShape);
            }

            ResetRobot();

            World.StepSimulation(0.1f, 100);
        }

        /// <summary>
        /// Steps the world
        /// </summary>
        /// <param name="elapsedTime">elapsed time</param>
        public virtual void Update(float elapsedTime, KeyboardKeyEventArgs args)
        {
            DriveJoints.UpdateAllMotors(Skeleton, args);
            cachedArgs = args;

            if (Controls.GameControls[Controls.Control.ResetRobot] == args.Key)
                ResetRobot();
            
            World.StepSimulation(elapsedTime, 1000/*, 1f / 300f*/);

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

                AuxFunctions.OrientRobot(Wheels, Skeleton.BulletObject);
                
                bNode.BulletObject.WorldTransform = Matrix4.CreateTranslation(0, 10, 0);
                bNode.BulletObject.InterpolationLinearVelocity = Vector3.Zero;
                bNode.BulletObject.InterpolationAngularVelocity = Vector3.Zero;                
            }
        }        

        private void GetFromDirectory(string startDirectory, Action<string> useDir)
        {
            FileSelector fs = new FileSelector(startDirectory);
            Console.WriteLine("Enter the associated number to move into that directory");
            Console.WriteLine("Enter ! to select the current directory");
            Console.WriteLine("Enter . to go up one level");

            Exception ex;
            string dir = "";
            
            do
            {
                string cmd;
                do
                {
                    int i = 0;
                    foreach (string s in fs.Directories)
                    {
                        i++;
                        Console.WriteLine(i + " " + s);
                    }

                    cmd = Console.ReadLine();
                    if (cmd == ".")
                        fs.MoveUp();
                    else if (cmd != "!")
                        try
                        {
                            fs.MoveInto(fs.Directories.ToArray()[int.Parse(cmd) - 1]);
                        }
                        catch (Exception e) { Console.WriteLine("Invalid command"); }

                } while (cmd != "!");

                dir = fs.current + "\\";
                ex = null;
                try { useDir(dir); }
                catch (Exception e) { Console.WriteLine($"{e}\n{dir}"); ex = e; }
            } while (ex != null);            
        }
    }
}
