#version 410 core

#include <nagule/common.glsl>

uniform sampler2D DiffuseTex;
uniform sampler2D SpecularTex;
uniform sampler2D EmissionTex;

in VertOutput {
    vec3 Position;
    vec2 TexCoord;
    vec3 Normal;
} i;

out vec4 fragColor;

void main()
{
    vec2 tiledCoord = i.TexCoord * Tiling;
    //vec3 lightDir = (vec4(MainLight.Direction, 1) * WorldToObject).xyz;
    //vec3 cameraPos = (vec4(CameraPosition, 1) * WorldToObject).xyz;

    vec3 lightDir = MainLight.Direction;
    vec3 ambientColor = Ambient.xyz * Ambient.w;
    vec3 lightColor = MainLight.Color.xyz * MainLight.Color.w;

    // diffuse 
    float diff = max(dot(i.Normal, -lightDir), 0.0);
    vec4 diffuseColor = Diffuse * texture(DiffuseTex, tiledCoord);
    vec3 diffuse = (diff * lightColor + ambientColor) * diffuseColor.xyz;
    
    // specular
    vec3 viewDir = normalize(CameraPosition - i.Position);
    vec3 reflectDir = reflect(lightDir, i.Normal);
    float spec = pow(max(dot(viewDir, reflectDir), 0.0), Shininess);
    vec4 specularColor = Specular * texture(SpecularTex, tiledCoord);
    vec3 specular = (spec * lightColor + ambientColor) * specularColor.xyz;

    // emission
    vec4 emissionColor = Emission * texture(EmissionTex, tiledCoord);
    vec3 emission = emissionColor.xyz * emissionColor.w;

    // result
    fragColor = vec4(diffuse + specular + emission, 1);
}