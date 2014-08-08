using System;
using System.IO;
using Tao.OpenGl;

public class ShaderLoader {
	public static int loadShaderPair() {
		int shaderProgram = Gl.glCreateProgramObjectARB();
        int vertexShader = Gl.glCreateShaderObjectARB(Gl.GL_VERTEX_SHADER);
        int fragmentShader = Gl.glCreateShaderObjectARB(Gl.GL_FRAGMENT_SHADER);
        int fxxaShader = Gl.glCreateShaderObjectARB(Gl.GL_FRAGMENT_SHADER);
        int compositeShader = Gl.glCreateShaderObjectARB(Gl.GL_FRAGMENT_SHADER);

        System.Console.WriteLine(Directory.GetCurrentDirectory());
		string vertexShaderSource = File.ReadAllText(Directory.GetCurrentDirectory() +"/../../shader.vert");
        string fragmentShaderSource = File.ReadAllText(Directory.GetCurrentDirectory() + "/../../shader.frag");
        //string fxxaShaderSource = File.ReadAllText(Directory.GetCurrentDirectory() + "/../../fxxa.frag");
        //string compositeShaderSource = "void lightFragShader(); void fxxaFragShader(); void main() { lightFragShader(); fxxaFragShader(); }";


        Gl.glShaderSourceARB(vertexShader, 1, ref vertexShaderSource, null);
        Gl.glCompileShaderARB(vertexShader);
        Console.WriteLine("Compile vert shader");

        Gl.glShaderSourceARB(fragmentShader, 1, ref fragmentShaderSource, null);
        Gl.glCompileShaderARB(fragmentShader);
        Console.WriteLine("Compile frag shader");

       /* Gl.glShaderSourceARB(fxxaShader, 1, ref fxxaShaderSource, null);
        Gl.glCompileShaderARB(fxxaShader);
        Console.WriteLine("Compile fxxa shader");

        Gl.glShaderSourceARB(compositeShader, 1, ref compositeShaderSource, null);
        Gl.glCompileShaderARB(compositeShader);
        Console.WriteLine("Compile composite shader");*/

        Gl.glAttachObjectARB(shaderProgram, vertexShader);
        Gl.glAttachObjectARB(shaderProgram, fragmentShader);
        /*Gl.glAttachObjectARB(shaderProgram, fxxaShader);
        Gl.glAttachObjectARB(shaderProgram, compositeShader);*/
        Console.WriteLine("Attached objects");
        Gl.glLinkProgramARB(shaderProgram);
        Console.WriteLine("Linked");
		return shaderProgram;
	}
}