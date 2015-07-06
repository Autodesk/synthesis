#version 330 core

layout(location = 0) in vec3 vertex_Position;
layout(location = 1) in vec3 vertex_Normal;

void main()
{
	gl_Position.xyz = vertex_Position;
	gl_Position.w = 1.0;
}
