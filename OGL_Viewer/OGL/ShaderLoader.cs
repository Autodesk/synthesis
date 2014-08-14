using System;
using System.IO;
using OpenTK.Graphics.OpenGL;

public class ShaderLoader
{
    private static int partShaderInternal = -1;
    public static int PartShader
    {
        get
        {
            if (partShaderInternal == -1)
            {
                partShaderInternal = loadPartShader();
            }
            return partShaderInternal;
        }
        set
        {
            if (partShaderInternal >= 0)
            {
                GL.DeleteProgram(partShaderInternal);
                partShaderInternal = -1;
            }
        }
    }

    private static int loadPartShader()
    {
        int shaderProgram = GL.CreateProgram();
        int vertexShader = GL.CreateShader(ShaderType.VertexShader);
        int fragmentShader = GL.CreateShader(ShaderType.FragmentShader);

        System.Console.WriteLine(Directory.GetCurrentDirectory());
        string vertexShaderSource = File.ReadAllText(Directory.GetCurrentDirectory() + "/../../OGL/shader.vert");
        string fragmentShaderSource = File.ReadAllText(Directory.GetCurrentDirectory() + "/../../OGL/shader.frag");


        GL.ShaderSource(vertexShader, vertexShaderSource);
        GL.CompileShader(vertexShader);

        GL.ShaderSource(fragmentShader, fragmentShaderSource);
        GL.CompileShader(fragmentShader);

        GL.AttachShader(shaderProgram, vertexShader);
        GL.AttachShader(shaderProgram, fragmentShader);
        GL.LinkProgram(shaderProgram);
        Console.WriteLine("Part Shader Link Results: \n" + GL.GetProgramInfoLog(shaderProgram));
        return shaderProgram;
    }
}