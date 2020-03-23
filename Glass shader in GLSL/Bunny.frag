#version 120

uniform vec4 fvAmbient;
uniform vec4 fvSpecular;
uniform vec4 fvDiffuse;
uniform float fSpecularPower;
uniform float fres;

uniform sampler2D baseMap;
uniform samplerCube LightCube;

varying vec3 ViewDirection;
varying vec3 LightDirection;
varying vec3 Normal;

float Fresnel(float cosTheta, float F0)
{
   
    return F0 + (1.0 - F0) * pow(1.0 - cosTheta, 5.0);

}


void main( void )
{
   vec3 L = normalize(LightDirection);  // bun to lamp
   vec3 N = normalize(Normal);          // bun to any
   vec3 E = normalize(-ViewDirection);  // bun to eye
   
   float oxygen = 1.00029;
   float glass = 1.5;
   float n1 = oxygen;
   float n2 = glass;

   // Entry Angle
   float entry = dot(-E, N);
   // Exit Angle
   float exit = sqrt((1.0 - pow(n1 / n2, 2.0)) * (1.0 - pow(entry, 2.0)));
   // Reflection
   vec3 R = reflect(-E,N);
   // Refraction
   vec3 refraction = ((n1 / n2) * -E) + (exit - ((n1 / n2) * entry) * N);
   
   // Fresnel
   float fresnel = Fresnel(entry, 0.04);
   fresnel *= fres;
   
   vec3 reflectionCol = textureCube(LightCube, R).xyz;
   vec3 refractionCol = textureCube(LightCube, refraction).xyz;
   
   fresnel = smoothstep(0.0, 1.0, fresnel);
   
   reflectionCol *= fresnel;
   refractionCol *= (1.0 - fresnel);

   gl_FragColor = vec4(reflectionCol + refractionCol, 1.0);
}