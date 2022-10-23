#version 410 core

#include <nagule/common.glsl>

uniform sampler2D DepthBuffer;

in vec2 TexCoord;

void main()
{
    float depth = texture(DepthBuffer, TexCoord).x;
    vec3 viewPos = GetViewSpacePositionFromDepth(depth, TexCoord);
    vec3 worldPos = viewPos * mat3(inverse(Matrix_V));

    vec4 prevClipPos = vec4(worldPos, 1) * Matrix_Prev_VP;
    vec3 prevViewPos = prevClipPos.xyz;
    prevViewPos.xyz /= prevClipPos.w;
    prevViewPos.xy = prevViewPos.xy * 0.5 + 0.5;

    gl_FragDepth = distance(viewPos, prevViewPos) > 1.5 ? 1 : depth;
}