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
        CollisionDispatcher dispatcher;
        DbvtBroadphase broadphase;
        List<CollisionShape> collisionShapes = new List<CollisionShape>();
        CollisionConfiguration collisionConf;
        SoftBodySolver solver;
        public BulletFieldDefinition f;
        public BulletRigidNode Skeleton; //3spooky5me
        private Action OnUpdate;
                
        public Physics()
        {            
            collisionConf = new SoftBodyRigidBodyCollisionConfiguration();
            dispatcher = new CollisionDispatcher(new DefaultCollisionConfiguration());
            solver = new DefaultSoftBodySolver();

            broadphase = new DbvtBroadphase();
            World = new SoftRigidDynamicsWorld(dispatcher, broadphase, null, collisionConf, solver);
            
            World.Gravity = new Vector3(0, -10, 0);
            
            #region old stuff
            ////dynamic Rigid bodies
            //const float mass = 1.0f;
            //const float mass_s = 100.0f;

            //CollisionShape collShape = new BoxShape(1);
            //collisionShapes.Add(collShape);
            //Vector3 localInertia = collShape.CalculateLocalInertia(mass);

            //var rbInfo = new RigidBodyConstructionInfo(mass, null, collShape, localInertia);

            //const float start_x = StartPosX - ArraySizeX / 2;
            //const float start_y = StartPosY;
            //const float start_z = StartPosZ - ArraySizeZ / 2;

            //int x, y, z;
            //for (y = 0; y < ArraySizeY; y++)
            //{
            //    for (x = 0; x < ArraySizeX; x++)
            //    {
            //        for (z = 0; z < ArraySizeZ; z++)
            //        {
            //            Matrix4 startTransform = Matrix4.CreateTranslation(
            //                new Vector3(
            //                    2 * x + start_x,
            //                    2 * y + start_y,
            //                    2 * z + start_z
            //                    )
            //                );

            //            rbInfo.MotionState = new DefaultMotionState(startTransform);

            //            RigidBody body = new RigidBody(rbInfo);

            //            body.Translate(new Vector3(0, 18, 0));

            //            World.AddRigidBody(body);
            //        }
            //    }
            //}

            //CollisionShape sphere = new SphereShape(3);
            //collisionShapes.Add(sphere);
            //Vector3 localInertia_s = sphere.CalculateLocalInertia(mass_s);

            //var rbInfo_s = new RigidBodyConstructionInfo(mass_s, null, sphere, localInertia_s);

            //Matrix4 startTransform_s = Matrix4.CreateTranslation(
            //    new Vector3(
            //        StartPosX + 2,
            //        StartPosY,
            //        StartPosZ + 2
            //        )
            //    );

            //rbInfo_s.MotionState = new DefaultMotionState(startTransform_s);

            //RigidBody body_s = new RigidBody(rbInfo_s);

            //body_s.Translate(new Vector3(0, 70, 0));

            //World.AddRigidBody(body_s);
            #endregion

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
                    World.AddConstraint(bNode.joint);
                World.AddCollisionObject(bNode.BulletObject);
                collisionShapes.Add(bNode.BulletObject.CollisionShape);
            }

            //Field
            f = BulletFieldDefinition.FromFile(@"C:\Program Files (x86)\Autodesk\Synthesis\Synthesis\Fields\2015\");
            foreach (RigidBody b in f.Bodies)
            {
                World.AddRigidBody(b);
                collisionShapes.Add(b.CollisionShape);
            }

            World.StepSimulation(0.1f, 10);
        }

        public virtual void Update(float elapsedTime, KeyboardKeyEventArgs args)
        {
            DriveJoints.UpdateAllMotors(Skeleton, args);
            World.StepSimulation(elapsedTime, 10);
            OnUpdate?.Invoke();
        }

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
    }
}
