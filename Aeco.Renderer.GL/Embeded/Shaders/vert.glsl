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
    gl_Position = vec4(vertex, 1) * ObjectToWorld * Matrix_VP;
    o.Position = gl_Position.xyz;
    o.TexCoord = texCoord;
    o.Normal = (vec4(normal, 1) * transpose(WorldToObject)).xyz;
}