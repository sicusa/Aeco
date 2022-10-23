#version 410 core

#include <nagule/common.glsl>

uniform sampler2D ColorBuffer;
uniform sampler2D MaxDepthBuffer;
uniform sampler2D MinDepthBuffer;

in vec2 TexCoord;
out vec4 FragColor;

void main()
{
    const float gamma = 2.2;
    vec3 hdrColor = texture(ColorBuffer, TexCoord).rgb;
  
    // reinhard tone mapping
    vec3 mapped = hdrColor / (hdrColor + vec3(1.0));

    // gamma correction 
    mapped = pow(mapped, vec3(1.0 / gamma));
  
    float depth = LinearizeDepth(textureLod(MaxDepthBuffer, TexCoord, 1).x);
    mapped = vec3(depth, depth, depth);
    FragColor = vec4(mapped, 1.0);
}