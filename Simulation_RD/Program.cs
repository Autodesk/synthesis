using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using BulletSharp;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Simulation_RD
{
    class Program
    {
        //static GameWindow game;

        static void Main(string[] args)
        {
            /*
            //GameWindow constructor does have options for arguments, but we're using default constructor for now.
            game = new GameWindow(); 

            game.RenderFrame += Draw;

            game.Run(60.0); //Starts the game at 60fps
            */
            
            //Field
            string fieldPath = @"C:\Program Files (x86)\Autodesk\Synthesis\Synthesis\Fields\2015\";
            Console.WriteLine("[STARTING FIELD]");

            BulletFieldDefinition field = BulletFieldDefinition.FromFile(fieldPath);
                        
            Console.WriteLine("[DONE FIELD]\nPress enter to continue...");
            Console.ReadLine();

            //Robot
            Console.WriteLine("[STARTING ROBOT]");
            string robotPath = @"C:\Program Files (x86)\Autodesk\Synthesis\Synthesis\Robots\Sample Robot\";
            RigidNode_Base.NODE_FACTORY = delegate (Guid guid) { return new BulletRigidNode(guid); };
            RigidNode_Base skeleton = BXDJSkeleton.ReadSkeleton(robotPath + "skeleton.bxdj");
            foreach(RigidNode_Base node in skeleton.ListAllNodes())
            {
                BulletRigidNode bNode = (BulletRigidNode)node;
                bNode.CreateMesh(robotPath + bNode.ModelFileName);                
            }
            Console.WriteLine("[DONE ROBOT]");
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

            //game.SwapBuffers();
        }
    }
}
