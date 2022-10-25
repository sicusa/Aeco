#version 410 core

#include <nagule/common.glsl>

uniform sampler2D DepthBuffer;

in vec2 TexCoord;
out vec2 Motion;

void main()
{
    float depth = texture(DepthBuffer, TexCoord).x;
    vec4 viewPos = GetViewSpacePositionFromDepth(depth, TexCoord);
    vec4 worldPos = viewPos * inverse(Matrix_V);

    vec4 prevClipPos = worldPos * Matrix_Prev_VP;
    prevClipPos.xyz /= prevClipPos.w;
    prevClipPos.xy = prevClipPos.xy * 0.5 + vec2(0.5);

    Motion = TexCoord - prevClipPos.xy;
}