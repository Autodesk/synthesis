
varying vec3 normal,lightDir[2],halfVector[2];
varying float dist[2];
uniform vec4 tintColor = vec4(1,1,1,1);


void main()
{
	vec3 n,halfV,viewV,ldir;
	float NdotL,NdotHV;
	vec4 color = gl_LightModel.ambient * gl_FrontMaterial.ambient;
	float attenuation;
	n = normalize(normal);

	int i;
	for (i = 0; i<2; i++){
		NdotL = max(dot(n,normalize(lightDir[i])),0.0);

		if (NdotL > 0.0) {
			attenuation = 1.0 / (gl_LightSource[i].constantAttenuation +
				gl_LightSource[i].linearAttenuation * dist[i] +
				gl_LightSource[i].quadraticAttenuation * dist[i] * dist[i]);
			color += attenuation * (gl_FrontMaterial.diffuse * gl_LightSource[i].diffuse * NdotL) + (gl_FrontMaterial.ambient * gl_LightSource[i].ambient);


			halfV = normalize(halfVector[i]);
			NdotHV = max(dot(n,halfV),0.0);
			color += attenuation * gl_FrontMaterial.specular * gl_LightSource[i].specular * 
				pow(NdotHV,gl_FrontMaterial.shininess);
		}
	}

	gl_FragColor = color * tintColor;
}