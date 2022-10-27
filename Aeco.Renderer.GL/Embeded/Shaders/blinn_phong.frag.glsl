#version 410 core

uniform sampler2D DiffuseTex;
uniform sampler2D SpecularTex;
uniform sampler2D EmissionTex;

#include <nagule/blinn_phong.glsl>

in VertexOutput {
    vec3 Position;
    vec2 TexCoord;
    vec3 Normal;
} i;

out vec4 FragColor;

void main()
{
    FragColor = BlinnPhong(i.Position, i.TexCoord, i.Normal);
}