#version 410 core

#include <nagule/common.glsl>

uniform ivec2 TileCount;

uniform int LightCount;
uniform sampler2D LightBuffer;

uniform int DepthLOD;
uniform sampler2D MaxDepthBuffer;
uniform sampler2D MinDepthBuffer;

in vec2 TexCoord;
flat in int TileIndex;

out vec4 FragColor;

void main()
{
    int total = TileCount.x * TileCount.y;
    FragColor = vec4(float(TileIndex) / total, 1, 1, 1);
}