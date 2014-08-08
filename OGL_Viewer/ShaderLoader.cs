using Tao.OpenGl;

public class ShaderLoader {
	public static int loadShaderPair() {
		int shaderProgram = Gl.glCreateProgramObjectARB();
        int vertexShader = Gl.glCreateShaderObjectARB(Gl.GL_VERTEX_SHADER);
        int fragmentShader = Gl.glCreateShaderObjectARB(Gl.GL_FRAGMENT_SHADER);
		string vertexShaderSource = ("#version 120\n"
						+ "varying vec4 varyingColour;\n"
						+ "varying vec3 varyingNormal;\n"
						+ "varying vec4 varyingVertex;\n"
						+ "\n"
						+ "void main() {\n"
						+ "    varyingColour = gl_Color;\n"
						+ "    varyingNormal = gl_Normal;\n"
						+ "    varyingVertex = gl_Vertex;\n"
						+ "    gl_Position = gl_ModelViewProjectionMatrix * gl_Vertex;\n"
						+ "}");
		string fragmentShaderSource = ("#version 120\n"
						+ "varying vec4 varyingColour;\n"
						+ "varying vec3 varyingNormal;\n"
						+ "varying vec4 varyingVertex;\n"
						+ "\n"
						+ "void main() {\n"
						+ "    vec3 vertexPosition = (gl_ModelViewMatrix * varyingVertex).xyz;\n"
						+ "    vec3 surfaceNormal = normalize((gl_NormalMatrix * varyingNormal).xyz);\n"
						+ "    gl_FragColor.rgb = vec3(0,0,0);\n"
						+ "    gl_FragColor += gl_LightModel.ambient;\n"
						+ "    for (int i = 0; i<2; i++) {"
						+ "    vec3 lightDirection = normalize(gl_LightSource[i].position.xyz - vertexPosition);\n"
						+ "    float diffuseLightIntensity = max(0, dot(surfaceNormal, lightDirection));\n"
						+ "    gl_FragColor.rgb += diffuseLightIntensity * varyingColour.rgb;\n"
						+ "    vec3 reflectionDirection = normalize(reflect(-lightDirection, surfaceNormal));\n"
						+ "    float specular = max(0.0, dot(surfaceNormal, reflectionDirection));\n"
						+ "    if (diffuseLightIntensity != 0) {\n"
						+ "        float fspecular = pow(specular, gl_FrontMaterial.shininess);\n"
						+ "        gl_FragColor += fspecular;\n" + "    }\n"
						+ "    }\n" + "}");

        Gl.glShaderSourceARB(vertexShader, 1, ref vertexShaderSource, null);
        Gl.glCompileShaderARB(vertexShader);

        Gl.glShaderSourceARB(fragmentShader, 1, ref fragmentShaderSource, null);
        Gl.glCompileShaderARB(fragmentShader);

        Gl.glAttachObjectARB(shaderProgram, vertexShader);
        Gl.glAttachObjectARB(shaderProgram, fragmentShader);
        Gl.glLinkProgramARB(shaderProgram);
		return shaderProgram;
	}
}