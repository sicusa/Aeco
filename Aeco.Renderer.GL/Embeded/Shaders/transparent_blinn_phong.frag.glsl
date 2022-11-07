#version 410 core

#include <nagule/blinn_phong.glsl>
#include <nagule/transparency.glsl>

in VertexOutput {
    vec3 Position;
    vec2 TexCoord;
    vec3 Normal;
} i;

layout(location = 0) out vec4 AccumColor;
layout(location = 1) out float AccumAlpha;

void main()
{
    vec4 color = CalculateBlinnPhongLighting(i.Position, i.TexCoord, i.Normal);
    AccumAlpha = GetTransparency(color.a);
    AccumColor = vec4(color.rgb * AccumAlpha, color.a);
}