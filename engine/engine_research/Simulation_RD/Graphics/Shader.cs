using OpenTK.Graphics.OpenGL;

namespace Simulation_RD.Graphics
{
    class Shader
    {
        public int program;

        public Shader (string vertexPath, string fragmentPath)
        {
            int vertex, fragment;

            /* unneeded for now */
            //string[] vertexCode = new string[vertexPath.Length];
            //for(int i = 0; i < vertexPath.Length; i++)
            //{
            //    vertexCode[i] = vertexPath.Substring(i, 1);
            //}
            //string[] fragmentCode = new string[fragmentPath.Length];
            //for (int i = 0; i < fragmentPath.Length; i++)
            //{
            //    fragmentCode[i] = fragmentPath.Substring(i, 1);
            //}

            vertex = GL.CreateShader(ShaderType.VertexShader);
            GL.ShaderSource(vertex, vertexPath);
            GL.CompileShader(vertex);

            fragment = GL.CreateShader(ShaderType.FragmentShader);
            GL.ShaderSource(fragment, fragmentPath);
            GL.CompileShader(fragment);

            program = GL.CreateProgram();
            GL.AttachShader(program, vertex);
            GL.AttachShader(program, fragment);
            GL.LinkProgram(program);

            GL.DeleteShader(vertex);
            GL.DeleteShader(fragment);
        }

        public void Use()
        {
            GL.UseProgram(program);
        }
    }
}
