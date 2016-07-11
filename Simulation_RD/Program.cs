using System;
using System.Collections.Generic;
using BulletSharp;
using BulletSharp.SoftBody;
using OpenTK;

namespace Simulation_RD
{
    //Issue: Stepping the world with both a field and robot object will cause a stack overflow exception.
    //Just adding them will not cause an exception.
    //Just stepping with either a field or a robot rigid body will work.
    //I have no explanation for this
    //Good luck

    class Program
    {
        static GameWindow game;

        static void Main(string[] args)
        {
            List<RigidBody> btBodies = new List<RigidBody>();
            /*
            //GameWindow constructor does have options for arguments, but we're using default constructor for now.
            game = new GameWindow();

            game.RenderFrame += Draw;
            game.Run(60.0); //Starts the game at 60fps
            */
            BroadphaseInterface broadphase = new DbvtBroadphase();
            SoftBodyRigidBodyCollisionConfiguration collisionConfig = new SoftBodyRigidBodyCollisionConfiguration();
            CollisionDispatcher dispatcher = new CollisionDispatcher(collisionConfig);
            GImpactCollisionAlgorithm.RegisterAlgorithm(dispatcher);
            SequentialImpulseConstraintSolver solver = new SequentialImpulseConstraintSolver();
            DiscreteDynamicsWorld world = new SoftRigidDynamicsWorld(dispatcher, broadphase, solver, collisionConfig);
            world.Gravity = new Vector3(0, -9.81f, 0);

            //Field
            string fieldPath = @"C:\Program Files (x86)\Autodesk\Synthesis\Synthesis\Fields\2015\";
            Console.WriteLine("[STARTING FIELD]");

            BulletFieldDefinition field = BulletFieldDefinition.FromFile(fieldPath);

            //btBodies.Add(field.BulletObject);
            Console.WriteLine("[DONE FIELD]");

            //Robot
            Console.WriteLine("[STARTING ROBOT]");
            string robotPath = @"C:\Program Files (x86)\Autodesk\Synthesis\Synthesis\Robots\Sample Robot\";
            RigidNode_Base.NODE_FACTORY = delegate (Guid guid) { return new BulletRigidNode(guid); };
            RigidNode_Base skeleton = BXDJSkeleton.ReadSkeleton(robotPath + "skeleton.bxdj");
            List<RigidNode_Base> nodes = skeleton.ListAllNodes();
            for(int i = 0; i < nodes.Count; i++)
            {
                BulletRigidNode bNode = (BulletRigidNode)nodes[i % nodes.Count];
                bNode.CreateRigidBody(robotPath + bNode.ModelFileName);
                //btBodies.Add("Field Mesh " + i, field.BulletObject);
                //if (i == 20)
                //{
                //    bNode = (BulletRigidNode)nodes[0];
                //    bNode.CreateRigidBody(robotPath + bNode.ModelFileName);
                    
                //}
                btBodies.Add(bNode.BulletObject);
            }
            Console.WriteLine("[DONE ROBOT]");

            field.Bodies.ForEach((b) => btBodies.Add(b));

            foreach (var entry in btBodies)
            {
                //Console.WriteLine("Adding " + entry.Key);
                world.AddRigidBody(entry);
                world.StepSimulation(1.0f / 60.0f);
                Console.WriteLine("Adding/Stepping");
            }

            //for (int i = 0; i < 30; i++)
            //    world.StepSimulation(1.0f / 60.0f, 10);

            world.Dispose();
            Console.WriteLine("[WORLD DISPOSED]\nPress Enter to exit...");
            Console.ReadLine();
        }

        /// <summary>
        /// Draws all of the game window
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        static void Draw(object sender, FrameEventArgs e)
        {
            //Draw things here

            game.SwapBuffers();
        }
    }
}
