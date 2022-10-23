#version 410 core

#include <nagule/common.glsl>

uniform sampler2D DiffuseTex;
uniform sampler2D SpecularTex;
uniform sampler2D EmissionTex;
uniform sampler2D DepthBuffer;

in VertOutput {
    vec3 Position;
    vec2 TexCoord;
    vec3 Normal;
} i;

out vec4 FragColor;

void main()
{
    vec2 tiledCoord = i.TexCoord * Tiling;

    vec3 lightDir = MainLightDirection;
    vec3 lightColor = MainLightColor.xyz * MainLightColor.w;
    vec3 ambientColor = Ambient.xyz * Ambient.w;

    // diffuse 
    float diff = max(0.5 * dot(i.Normal, -lightDir) + 0.5, 0.0);
    vec4 diffuseColor = Diffuse * texture(DiffuseTex, tiledCoord);
    vec3 diffuse = (diff * lightColor + ambientColor) * diffuseColor.xyz;
    
    // specular
    vec3 viewDir = normalize(CameraPosition - i.Position);
    vec3 divisor = normalize(viewDir - lightDir);
    float spec = pow(max(dot(divisor, i.Normal), 0.0), Shininess);
    vec4 specularColor = Specular * texture(SpecularTex, tiledCoord);
    vec3 specular = spec * lightColor * specularColor.xyz;

    // emission
    vec4 emissionColor = Emission * texture(EmissionTex, tiledCoord);
    vec3 emission = emissionColor.xyz * emissionColor.w;

    // result
    FragColor = vec4(diffuse + specular + emission, 1);
}