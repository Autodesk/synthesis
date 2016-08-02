using Simulation_RD.Graphics;

namespace Simulation_RD
{
    /// <summary>
    /// Main Class
    /// </summary>
    class Program
    {
        //static GameWindow game;

        /// <summary>
        /// You know what this does
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Scene scene = new Scene();

            Shader shader = new Shader(@"\resources\vertexShader.vs\", @"\resources\fragmentShader.frag");
            
            scene.Run();
        }
    }
}
