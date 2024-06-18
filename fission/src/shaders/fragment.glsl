uniform float uTime;
uniform float rColor;
uniform float gColor;
uniform float bColor;

varying vec3 vPosition;
varying vec2 vUv;

#include "/node_modules/lygia/generative/snoise.glsl" // this is wack
// #include "colors.glsl"


vec3 getColor(float noiseValue, float rColor, float gColor, float bColor) {

	// vec3 horizonColor = vec3(1.0, 0.0, 0.0);
	vec3 horizonColor = vec3(0.6, 0.8, 1.0); 
    vec3 cloudColor = vec3(1.0, 1.0, 1.0);
	float darkenFactor = 0.7;

	// vec3 horizonColor = rgb2hsl(originalColor);
	// horizonColor.y *= 2.0;
	// horizonColor = hsl2rgb(horizonColor);
    // horizonColor = mix(horizonColor, cloudColor, 0.3);

    vec3 skyColor = mix(horizonColor, cloudColor, noiseValue);
	vec3 voidColor = horizonColor * darkenFactor;

    // vec3 cloudColor = vec3(1.0, 1.0, 1.0);
	// float darkenFactor = 0.6;

    // vec3 horizonColor = vec3(rColor, gColor, bColor);
    // vec3 skyColor = mix(horizonColor, cloudColor, noiseValue);
	// vec3 voidColor = horizonColor * darkenFactor;

    float tHorizon = smoothstep(200.0, 700.0, vPosition.y);
    float tVoid = smoothstep(-700.0, -200.0, vPosition.y);

    vec3 blendedColor = mix(horizonColor, skyColor, tHorizon);
    vec3 finalColor = mix(voidColor, blendedColor, tVoid);

    return finalColor;
}

float func(float x) {
	return x*x*(3.0-2.0*x);
	// float a = x * 0.7 - 1.0;
	// return a * a * a + 1.0;
}

void main() {
	vec4 noiseCoord = vec4(vPosition.xz * 0.005, 1.0, 1.0); 
	float noiseValue = snoise(noiseCoord) * 0.5 + 0.5;
	noiseValue = func(noiseValue);

	vec3 color = getColor(noiseValue, rColor, gColor, bColor);

	gl_FragColor = vec4(color, 1.0);
}