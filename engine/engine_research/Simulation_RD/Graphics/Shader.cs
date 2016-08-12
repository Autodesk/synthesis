using OpenTK.Graphics.OpenGL;
using System.Drawing;
using System.Drawing.Imaging;

namespace Simulation_RD.Graphics
{
    /// <summary>
    /// No idea what this does. Ask Toby to explain
    /// </summary>
    class Shader
    {
        public int program;

        /// <summary>
        /// Should load a texture from a file, but I'm not sure if it works.
        /// </summary>
        /// <param name="file">File path</param>
        /// <returns></returns>
        public static int LoadTexture(string file)
        {
            Bitmap bitmap = new Bitmap(file);

            int tex;
            GL.Hint(HintTarget.PerspectiveCorrectionHint, HintMode.Nicest);

            GL.GenTextures(1, out tex);
            GL.BindTexture(TextureTarget.Texture2D, tex);

            BitmapData data = bitmap.LockBits(new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);

            GL.TexImage2D(TextureTarget.Texture2D, 0, PixelInternalFormat.Rgba, data.Width, data.Height, 0,
                OpenTK.Graphics.OpenGL.PixelFormat.Bgra, PixelType.UnsignedByte, data.Scan0);
            bitmap.UnlockBits(data);


            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMinFilter, (int)TextureMinFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapS, (int)TextureWrapMode.Repeat);
            GL.TexParameter(TextureTarget.Texture2D, TextureParameterName.TextureWrapT, (int)TextureWrapMode.Repeat);

            return tex;
        }

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
