
varying vec3 normal,lightDir[2],halfVector[2];
varying float dist[2];

void main()
{	
	vec4 ecPos;
	vec3 aux;

	normal = normalize(gl_NormalMatrix * gl_Normal);

	int i;
	for (i = 0; i<2; i++){
		ecPos = gl_ModelViewMatrix * gl_Vertex;
		aux = vec3(gl_LightSource[i].position-ecPos);
		lightDir[i] = normalize(aux);
		dist[i] = length(aux);

		halfVector[i] = normalize(gl_LightSource[i].halfVector.xyz);
	}


	gl_Position = gl_ProjectionMatrix * ecPos;
} 
