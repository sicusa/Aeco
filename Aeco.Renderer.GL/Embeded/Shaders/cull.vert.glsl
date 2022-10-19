#version 410 core

#include <nagule/common.glsl>

layout(location = 0) in mat4 objectToWorld;
out mat4 originalObjectToWorld;

flat out int objectVisible;

void main()
{
    originalObjectToWorld = objectToWorld;

    /* calculate MVP matrix */
    mat4 mvp = objectToWorld * Matrix_VP;

    /* create the bounding box of the object in world space */
    vec4 boundingBox[8];
    boundingBox[0] = vec4(BoundingBoxMax.x, BoundingBoxMax.y, BoundingBoxMax.z, 1.0) * mvp;
    boundingBox[1] = vec4(BoundingBoxMin.x, BoundingBoxMax.y, BoundingBoxMax.z, 1.0) * mvp;
    boundingBox[2] = vec4(BoundingBoxMax.x, BoundingBoxMin.y, BoundingBoxMax.z, 1.0) * mvp;
    boundingBox[3] = vec4(BoundingBoxMin.x, BoundingBoxMin.y, BoundingBoxMax.z, 1.0) * mvp;
    boundingBox[4] = vec4(BoundingBoxMax.x, BoundingBoxMax.y, BoundingBoxMin.z, 1.0) * mvp;
    boundingBox[5] = vec4(BoundingBoxMin.x, BoundingBoxMax.y, BoundingBoxMin.z, 1.0) * mvp;
    boundingBox[6] = vec4(BoundingBoxMax.x, BoundingBoxMin.y, BoundingBoxMin.z, 1.0) * mvp;
    boundingBox[7] = vec4(BoundingBoxMin.x, BoundingBoxMin.y, BoundingBoxMin.z, 1.0) * mvp;

    /* check how the bounding box resides regarding to the view frustum */   
    int outOfBound[6] = int[6](0, 0, 0, 0, 0, 0);

    for (int i = 0; i < 8; i++) {
        if (boundingBox[i].x >  boundingBox[i].w) outOfBound[0]++;
        if (boundingBox[i].x < -boundingBox[i].w) outOfBound[1]++;
        if (boundingBox[i].y >  boundingBox[i].w) outOfBound[2]++;
        if (boundingBox[i].y < -boundingBox[i].w) outOfBound[3]++;
        if (boundingBox[i].z >  boundingBox[i].w) outOfBound[4]++;
        if (boundingBox[i].z < -boundingBox[i].w) outOfBound[5]++;
    }

    bool inFrustum = true;

    for (int i = 0; i < 6; ++i) {
        if (outOfBound[i] == 8) {
            inFrustum = false;
        }
    }

    objectVisible = inFrustum ? 1 : 0;
}
