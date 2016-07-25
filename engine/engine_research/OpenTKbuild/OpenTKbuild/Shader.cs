using OpenTK;
using OpenTK.Graphics.OpenGL;

namespace OpenTKbuild
{
    class Shader
    {

        public int ShaderProgram = 0;
        public static int vertex, fragment, geometry;

        public Shader(string vertexPath, string fragmentPath)
        {
            string vCode = null, fCode = null;
            try
            {
                vCode = System.IO.File.ReadAllText(vertexPath);
                fCode = System.IO.File.ReadAllText(fragmentPath);
            }
            catch
            {
                System.Console.WriteLine("FILE NOT READ SUCCESSFULLY\n");
            }

            vertex = GL.CreateShader(ShaderType.VertexShader);
            GL.CompileShader(vertex);

            fragment = GL.CreateShader(ShaderType.FragmentShader);
            GL.CompileShader(fragment);

            compileShader(vertex, vCode);
            compileShader(fragment, fCode);

            ShaderProgram = GL.CreateProgram();
            GL.AttachShader(ShaderProgram, vertex);
            GL.AttachShader(ShaderProgram, fragment);
            GL.LinkProgram(ShaderProgram);

            string info;
            GL.GetProgramInfoLog(ShaderProgram, out info);
            System.Console.WriteLine(info);
        }

        public Shader(string vertexPath, string fragmentPath, string geometryPath)
        {
            #region OLD
            //string vertexCode = null, fragmentCode = null;

            //System.IO.FileStream vShaderFile = new System.IO.FileStream(vertexPath, System.IO.FileMode.Open);
            //System.IO.FileStream fShaderFile = new System.IO.FileStream(fragmentPath, System.IO.FileMode.Open);
            #endregion

            string vCode = null, fCode = null, gCode = null;

            try
            {
                #region OLD
                //System.IO.StreamReader vShaderRead = new System.IO.StreamReader(vShaderFile);
                //System.IO.StreamReader fShaderRead = new System.IO.StreamReader(fShaderFile);
                //vShaderFile.Close();
                //fShaderFile.Close();

                //vertexCode = vShaderRead.ToString();
                //fragmentCode = fShaderRead.ToString();
                #endregion
                vCode = System.IO.File.ReadAllText(vertexPath);
                fCode = System.IO.File.ReadAllText(fragmentPath);
                gCode = System.IO.File.ReadAllText(geometryPath);

            }
            catch
            {
                System.Console.WriteLine("FILE NOT READ SUCCESSFULLY\n");
            }

            #region OLD
            //char[] vShaderCodeChar = vertexCode.ToCharArray();
            //char[] fShaderCodeChar = fragmentCode.ToCharArray();

            //string[] vShaderCode = new string[vShaderCodeChar.Length];
            //string[] fShaderCode = new string[fShaderCodeChar.Length];

            //for (int i = 0; i < vShaderCodeChar.Length; i++)
            //{
            //    vShaderCode[i] = vShaderCodeChar[i].ToString();
            //}

            //for (int i = 0; i < vShaderCodeChar.Length; i++)
            //{
            //    vShaderCode[i] = vShaderCodeChar[i].ToString();
            //}
            #endregion



            vertex = GL.CreateShader(ShaderType.VertexShader);
            GL.CompileShader(vertex);

            fragment = GL.CreateShader(ShaderType.FragmentShader);
            GL.CompileShader(fragment);

            geometry = GL.CreateShader(ShaderType.GeometryShaderExt);
            GL.CompileShader(geometry);

            compileShader(vertex, vCode);
            compileShader(fragment, fCode);
            compileShader(geometry, gCode);

            ShaderProgram = GL.CreateProgram();
            GL.AttachShader(ShaderProgram, vertex);
            GL.AttachShader(ShaderProgram, fragment);
            GL.LinkProgram(ShaderProgram);

            string info;
            GL.GetProgramInfoLog(ShaderProgram, out info);
            System.Console.WriteLine(info);

            GL.ProgramParameter(ShaderProgram, Version32.GeometryInputType, (int)All.Lines);
            GL.ProgramParameter(ShaderProgram, Version32.GeometryOutputType, (int)All.LineStrip);
            int tmp;
            GL.GetInteger((GetPName)ExtGeometryShader4.MaxGeometryOutputVerticesExt, out tmp);


            #region OLD

            //int success;
            //char[] infolog = new char[512];

            //int[] vcount = null;
            //int[] fcount = null;
            //int nullint = 0;

            //System.Text.StringBuilder sb = new System.Text.StringBuilder();
            //sb.Append(infolog);

            //vertex = GL.CreateShader(ShaderType.VertexShader);
            //GL.CompileShader(vertex);

            //GL.GetShader(vertex, ShaderParameter.CompileStatus, out success);
            //if (success == 0)
            //{
            //    GL.GetShaderInfoLog(vertex, 512, out nullint, sb);
            //    System.Console.WriteLine("Error: Shader : Vertex : Compilation Failed\n" + infolog);
            //}

            //fragment = GL.CreateShader(ShaderType.FragmentShader);
            //GL.CompileShader(fragment);

            //GL.GetShader(fragment, ShaderParameter.CompileStatus, out success);
            //if (success == 0)
            //{
            //    GL.GetShaderInfoLog(fragment, 512, out nullint, sb);
            //    System.Console.WriteLine("Error: Shader : Fragment : Compilation Failed\n" + infolog);
            //}

            //Program = GL.CreateProgram();
            //GL.AttachShader(Program, vertex);
            //GL.AttachShader(Program, fragment);
            //GL.LinkProgram(Program);

            //GL.DeleteShader(vertex);
            //GL.DeleteShader(fragment);

            #endregion


        }

        public void Use()
        {
            GL.UseProgram(ShaderProgram);
        }

        private void compileShader(int shader, string source)
        {
            GL.ShaderSource(shader, source);
            GL.CompileShader(shader);

            string info;
            GL.GetShaderInfoLog(shader, out info);
            System.Console.WriteLine(info);

            int compileResult;
            GL.GetShader(shader, ShaderParameter.CompileStatus, out compileResult);
            if (compileResult != 1)
            {
                System.Console.WriteLine("CompileError : " + source);
            }
        }

        public void cleanUp()
        {
            if (fragment != 0)
                GL.DeleteShader(fragment);
            if (vertex != 0)
                GL.DeleteShader(vertex);
            if (geometry != 0)
                GL.DeleteShader(geometry);
        }

    }
}
