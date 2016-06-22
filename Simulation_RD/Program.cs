using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace Simulation_RD
{
    class Program
    {
        static GameWindow game;

        static void Main(string[] args)
        {
            //GameWindow constructor does have options for arguments, but we're using default constructor for now.
            game = new GameWindow(); 

            game.RenderFrame += Draw;

            game.Run(60.0); //Starts the game at 60fps
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
