###How to set up:
Open Assets/scene/stablescene in unity
####Make sure:
*Init.cs should be attached to an empty GameObject

In order to build the project properly, all shaders used in 
runtime must also be present in the Unity scene. 
Current shaders used in runtime: 
* Diffuse
* Specular
* Transparent/Diffuse
* Transparent/Specular

To keep these on build, attach them to any textured primitive.
