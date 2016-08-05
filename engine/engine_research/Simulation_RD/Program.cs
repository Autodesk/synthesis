using Simulation_RD.Graphics;

namespace Simulation_RD
{
    /// <summary>
    /// Main Class
    /// </summary>
    class Program
    {
        /// <summary>
        /// You know what this does
        /// </summary>
        /// <param name="args"></param>
        static void Main(string[] args)
        {
            Scene scene = new Scene();

            //Other thing
            Shader shader = new Shader(@"\resources\vertexShader.vs\", @"\resources\fragmentShader.frag");
            
            //Start game
            scene.Run();
        }
    }
}
