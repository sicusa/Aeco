#version 410 core

#include <nagule/common.glsl>

uniform sampler2D ColorBuffer;
uniform sampler2D TransparencyAccumBuffer;
uniform sampler2D TransparencyAlphaBuffer;
uniform sampler2D MaxDepthBuffer;
uniform sampler2D MinDepthBuffer;

in vec2 TexCoord;
out vec4 FragColor;

subroutine vec4 PostFunc();
subroutine uniform PostFunc PostFuncUniform;

subroutine(PostFunc) vec4 ShowColor()
{
    const float gamma = 2.2;
    vec3 hdrColor = texture(ColorBuffer, TexCoord).rgb;
  
    // reinhard tone mapping
    vec3 mapped = hdrColor / (hdrColor + vec3(1.0));

    // gamma correction 
    mapped = pow(mapped, vec3(1.0 / gamma));

    return vec4(mapped, 1.0);
}

subroutine(PostFunc) vec4 ShowMaxDepth()
{
    float depth = LinearizeDepth(textureLod(MaxDepthBuffer, TexCoord, 3).r);
    return vec4(depth, depth, depth, 1.0);
}

subroutine(PostFunc) vec4 ShowMinDepth()
{
    float depth = LinearizeDepth(textureLod(MinDepthBuffer, TexCoord, 3).r);
    return vec4(depth, depth, depth, 1.0);
}

subroutine(PostFunc) vec4 ShowTransparencyAccum() {
    return texture(TransparencyAccumBuffer, TexCoord);
}

subroutine(PostFunc) vec4 ShowTransparencyAlpha() {
    return vec4(texture(TransparencyAlphaBuffer, TexCoord).r);
}

void main()
{
    FragColor = PostFuncUniform();
}