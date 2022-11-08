#version 410 core

#include <aeco/lighting.glsl>

uniform sampler2D ColorBuffer;
uniform sampler2D TransparencyAccumBuffer;
uniform sampler2D TransparencyAlphaBuffer;
uniform sampler2D DepthBuffer;

in vec2 TexCoord;
out vec4 FragColor;

subroutine vec3 PostFunc();
subroutine uniform PostFunc PostFuncUniform;

subroutine(PostFunc) vec3 ShowColor()
{
    return texture(ColorBuffer, TexCoord).rgb;
}

subroutine(PostFunc) vec3 ShowDepth()
{
    float depth = LinearizeDepth(textureLod(DepthBuffer, TexCoord, 3).r);
    return vec3(depth, depth, depth);
}

subroutine(PostFunc) vec3 ShowTransparencyAccum() {
    return texture(TransparencyAccumBuffer, TexCoord).rgb;
}

subroutine(PostFunc) vec3 ShowTransparencyAlpha() {
    return vec3(texture(TransparencyAlphaBuffer, TexCoord).r);
}

vec3 ACESToneMapping(vec3 color)
{
    const float A = 2.51f;
    const float B = 0.03f;
    const float C = 2.43f;
    const float D = 0.59f;
    const float E = 0.14f;
    return (color * (A * color + B)) / (color * (C * color + D) + E);
}

void main()
{
    const float gamma = 2.2;
    vec3 hdrColor = PostFuncUniform();
  
    // tone mapping
    vec3 mapped = ACESToneMapping(hdrColor);

    // gamma correction 
    mapped = pow(mapped, vec3(1.0 / gamma));

    FragColor = vec4(mapped, 1.0);
}