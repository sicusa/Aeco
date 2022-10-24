#version 410 core

uniform sampler2D DiffuseTex;
uniform sampler2D SpecularTex;
uniform sampler2D EmissionTex;

#include <nagule/blinn_phong.glsl>
#include <nagule/transparency.glsl>

in VertOutput {
    vec3 Position;
    vec2 TexCoord;
    vec3 Normal;
} i;

layout(location = 0) out vec4 AccumColor;
layout(location = 1) out float AccumAlpha;

void main()
{
    vec4 color = BlinnPhong(i.Position, i.TexCoord, i.Normal);
    AccumAlpha = GetWeightedCoverage(color.a);
    AccumColor = vec4(color.rgb * AccumAlpha, color.a);
}