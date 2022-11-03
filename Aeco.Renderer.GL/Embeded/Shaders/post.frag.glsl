#version 410 core

#include <nagule/common.glsl>
#include <nagule/lighting.glsl>

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

subroutine(PostFunc) vec3 ShowTiles() {
    float portion = float(GetTileLightOffset(gl_FragCoord.xy)) / TILE_TOTAL_COUNT;
    return vec3(portion * 0.01);
}

void main()
{
    const float gamma = 2.2;
    vec3 hdrColor = PostFuncUniform();
  
    // reinhard tone mapping
    vec3 mapped = hdrColor / (hdrColor + vec3(1.0));

    // gamma correction 
    mapped = pow(mapped, vec3(1.0 / gamma));

    FragColor = vec4(mapped, 1.0);
}