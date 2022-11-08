#ifndef AECO_LIGHTING
#define AECO_LIGHTING

#include <aeco/common.glsl>

#define CLUSTER_COUNT_X 16
#define CLUSTER_COUNT_Y 9
#define CLUSTER_COUNT_Z 24
#define CLUSTER_COUNT CLUSTER_COUNT_X * CLUSTER_COUNT_Y * CLUSTER_COUNT_Z
#define CLUSTER_MAXIMUM_LIGHT_COUNT 32

#define LIGHT_NONE          0
#define LIGHT_AMBIENT       1
#define LIGHT_DIRECTIONAL   2
#define LIGHT_POINT         3
#define LIGHT_SPOT          4
#define LIGHT_AREA          5

#define LIGHT_COMPONENT_COUNT 19

layout(std140) uniform LightingEnv {
    float ClusterDepthSliceMultiplier;
    float ClusterDepthSliceSubstractor;

    int LightClusters[CLUSTER_COUNT * CLUSTER_MAXIMUM_LIGHT_COUNT];
};

struct Light {
    int Category;       // 4    1
    vec4 Color;         // 16   5
    vec3 Position;      // 12   8
    vec3 Direction;     // 12   11
    vec3 Up;            // 12   14

    float AttenuationConstant;  // 4    15
    float AttenuationLinear;    // 4    16
    float AttenuationQuadratic; // 4    17

    vec2 ConeCutoffsOrAreaSize; // 8 19
};

uniform samplerBuffer LightBuffer;

int CalculateClusterDepthSlice(float z) {
    return int(floor(log(z) * ClusterDepthSliceMultiplier - ClusterDepthSliceSubstractor));
}

int GetClusterIndex(vec3 fragCoord)
{
    int depthSlice = CalculateClusterDepthSlice(fragCoord.z);
    float tileSize = ViewportWidth / CLUSTER_COUNT_X;

    return fragCoord.x / tileSize
        + CLUSTER_COUNT_X * fragCoord.y / tileSize
        + (CLUSTER_COUNT_X * CLUSTER_COUNT_Y) * depthSlice;
}

Light FetchLight(int index)
{
    int offset = index * LIGHT_COMPONENT_COUNT;
    Light light;

    int category = int(texelFetch(LightBuffer, offset).r);
    light.Category = category;

    light.Color = vec4(
        texelFetch(LightBuffer, offset + 1).r,
        texelFetch(LightBuffer, offset + 2).r,
        texelFetch(LightBuffer, offset + 3).r,
        texelFetch(LightBuffer, offset + 4).r);

    light.Position = vec3(
        texelFetch(LightBuffer, offset + 5).r,
        texelFetch(LightBuffer, offset + 6).r,
        texelFetch(LightBuffer, offset + 7).r);
    light.Direction = vec3(
        texelFetch(LightBuffer, offset + 8).r,
        texelFetch(LightBuffer, offset + 9).r,
        texelFetch(LightBuffer, offset + 10).r);
    light.Up = vec3(
        texelFetch(LightBuffer, offset + 11).r,
        texelFetch(LightBuffer, offset + 12).r,
        texelFetch(LightBuffer, offset + 13).r);

    light.AttenuationConstant = texelFetch(LightBuffer, offset + 14).r;
    light.AttenuationLinear = texelFetch(LightBuffer, offset + 15).r;
    light.AttenuationQuadratic = texelFetch(LightBuffer, offset + 16).r;

    light.ConeCutoffsOrAreaSize = vec2(
        texelFetch(LightBuffer, offset + 17).r,
        texelFetch(LightBuffer, offset + 18).r);

    return light;
}

float CalculateLightAttenuation(Light light, float distance)
{
    return 1 / (
        light.AttenuationConstant +
        light.AttenuationLinear * distance +
        light.AttenuationQuadratic * distance * distance);
}

#endif