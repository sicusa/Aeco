#version 410 core

#include <nagule/common.glsl>

uniform sampler2D ColorBuffer;
uniform sampler2D DepthBuffer;

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
  
    //float depth = LinearizeDepth(textureLod(DepthBuffer, TexCoord, 0).r);
    //mapped = vec3(depth, depth, depth);
    FragColor = vec4(mapped, 1.0);
}