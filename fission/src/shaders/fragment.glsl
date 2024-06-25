uniform float uTime;
uniform float rColor;
uniform float gColor;
uniform float bColor;

varying vec3 vPosition;
varying vec2 vUv;

#define PI 3.14159265358979323846264338327

#include "/node_modules/lygia/generative/snoise.glsl" // this is wack

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

// some calculations
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


// code for implementing image at the top without warping

	// float phi = atan(vPosition.z, vPosition.x);
	// float theta = acos(vPosition.y / length(vPosition));
	// theta = PI / 2.0 - theta;
	// vec2 adjustedUV = vec2(mod((phi * 3.0 / PI) + 0.5, 1.0), theta / PI);

	// // random calculations
	// vec3 absPos = abs(vPosition);
	// float maxCoord = max(absPos.x, max(absPos.y, absPos.z));
	// vec2 uv = absPos.x == maxCoord ? vec2(vPosition.z, vPosition.y) / maxCoord :
	// 		absPos.y == maxCoord ? vec2(vPosition.x, vPosition.z) / maxCoord :
	// 								vec2(vPosition.x, vPosition.y) / maxCoord;
	// adjustedUV = (uv + 1.0) / 2.0;
	// adjustedUV.x = vPosition.x < 0.0 || vPosition.z < 0.0 ? 1.0 - adjustedUV.x : adjustedUV.x;
	// adjustedUV.y = vPosition.y < 0.0 ? 1.0 - adjustedUV.y : adjustedUV.y;

	// vec4 imageColor = texture2D(uTexture, adjustedUV);
	// vec3 colorMix = mix(vec3(0.0, 0.0, 0.0), blueAccent, noiseValue*0.7);
	// vec3 skyColor = mix(colorMix, imageColor.rgb, imageColor.a);