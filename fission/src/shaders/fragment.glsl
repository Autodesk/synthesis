uniform float rColor;
uniform float gColor;
uniform float bColor;

varying vec3 vPosition;

#include "/node_modules/lygia/generative/snoise.glsl" // no other known way to include this file 

vec3 getColor(float noiseValue, float rColor, float gColor, float bColor) {
	vec3 blueAccent = vec3(16.0/255.0, 35.0/255.0, 110.0/255.0);

	vec3 skyColor = vec3(rColor, gColor, bColor);
	vec3 horizonColor = mix(vec3(0.0, 0.0, 0.0), blueAccent, noiseValue);
	vec3 voidColor = vec3(0.0, 0.0, 0.0);	

    float tHorizon = smoothstep(200.0, 700.0, vPosition.y);
    float tVoid = smoothstep(-700.0, -200.0, vPosition.y);

    vec3 blendedColor = mix(horizonColor, skyColor, tHorizon);
    vec3 finalColor = mix(voidColor, blendedColor, tVoid);

    return finalColor;
}

// passing noise function through x^2(3-2x) to get more cloud-like results
float func(float x) {
	return x*x*(3.0-2.0*x);
}


void main() {
	vec4 noiseCoord = vec4(vPosition.xz * 0.001, 1.0, 1.0); 
	float noiseValue = snoise(noiseCoord) * 0.5 + 0.5;
	noiseValue = func(noiseValue);

	vec3 color = getColor(noiseValue, rColor, gColor, bColor);

	gl_FragColor = vec4(color, 1.0);
}