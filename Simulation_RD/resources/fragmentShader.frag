#version 330 core
in Vector4 vertexColor; // The input variable from the vertex shader (same name and same type)
  
out Vector4 color;

void main()
{
    color = vertexColor;
} 