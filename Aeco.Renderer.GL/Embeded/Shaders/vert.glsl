#version 410 core

#include <nagule/common.glsl>

layout(location = 0) in vec3 vertex;
layout(location = 1) in vec2 texCoord;
layout(location = 2) in vec3 normal;

out VertOutput {
    vec3 Position;
    vec2 TexCoord;
    vec3 Normal;
} o;

void main()
{
    vec4 pos = vec4(vertex, 1) * ObjectToWorld;
    gl_Position = pos * Matrix_VP;
    o.Position = pos.xyz;
    o.TexCoord = texCoord;
    o.Normal = normalize(normal * mat3(transpose(WorldToObject)));
}