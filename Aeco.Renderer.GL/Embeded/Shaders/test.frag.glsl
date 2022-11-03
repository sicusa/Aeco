#version 410 core

#include <nagule/common.glsl>

uniform sampler2D DepthBuffer;

in vec2 TexCoord;
flat in int TileIndex;

out vec4 FragColor;

void main()
{
    int total = TileCount.x * TileCount.y;
    FragColor = vec4(float(TileIndex) / total, 1, 1, 1);
}