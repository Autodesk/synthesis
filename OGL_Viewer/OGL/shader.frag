varying vec3 vertex_light_position;
varying vec3 vertex_light_half_vector;
varying vec3 vertex_normal;
uniform vec4 tintColor = vec4(1,1,1,1);

void lightFragShader() {
	// Calculate the ambient term
	vec4 ambient_color = gl_LightModel.ambient * gl_FrontMaterial.ambient;

	// Calculate the diffuse term
	vec4 diffuse_color = vec4(0,0,0,0);

	// Calculate the specular value
	vec4 specular_color = vec4(0,0,0,0);
	int i;
	for (i = 0; i<2; i++) {
		ambient_color += gl_FrontMaterial.ambient * gl_LightSource[i].ambient;
		diffuse_color += gl_FrontMaterial.diffuse * gl_LightSource[i].diffuse;
		specular_color += gl_LightSource[i].specular;
	}
	specular_color *= gl_FrontMaterial.specular * pow(max(dot(vertex_normal, vertex_light_half_vector), 0.0), gl_FrontMaterial.shininess);

	// Set the diffuse value (darkness). This is done with a dot product between the normal and the light
	// and the maths behind it is explained in the maths section of the site.
	float diffuse_value = max(dot(vertex_normal, vertex_light_position), 0.0);

	// Set the output color of our current pixel
	gl_FragColor = (ambient_color + diffuse_color * diffuse_value) * tintColor + specular_color;
}

void main() {
	lightFragShader();
}