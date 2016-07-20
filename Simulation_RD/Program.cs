namespace Simulation_RD
{
    class Program
    {
        //static GameWindow game;

        static void Main(string[] args)
        {
            Scene scene = new Scene();

            Shader shader = new Shader(@"\resources\vertexShader.vs\", @"\resources\fragmentShader.frag");
            
            scene.Run();
        }
    }
}
