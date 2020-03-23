#version 120

uniform vec3 fvLightPosition;
uniform vec4 vViewPosition;

varying vec3 ViewDirection;
varying vec3 LightDirection;
varying vec3 Normal;
   
void main( void )
{
   gl_Position = ftransform();

   vec4 fvObjectPosition = gl_ModelViewMatrix * gl_Vertex;
   
   ViewDirection  = vViewPosition.xyz - fvObjectPosition.xyz; //bun to eye
   LightDirection = fvLightPosition - fvObjectPosition.xyz; //bun to lamp
   Normal         = gl_NormalMatrix * gl_Normal;
}