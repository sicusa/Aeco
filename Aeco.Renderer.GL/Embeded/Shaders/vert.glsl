#version 400 core

uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

layout(location = 0) in vec3 vertex;
layout(location = 1) in vec2 uv;

out vec2 texCoord;

void main()
{
    texCoord = uv;
    gl_Position = vec4(vertex, 1.0f) * World * View * Projection;
}