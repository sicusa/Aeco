#version 410 core

layout(points) in;
layout(points, max_vertices = 1) out;

in mat4 originalObjectToWorld[1];
flat in int objectVisible[1];

out mat4 culledObjectToWorld;

void main() {

   /* only emit primitive if the object is visible */
   if (objectVisible[0] == 1) {
      culledObjectToWorld = originalObjectToWorld[0];
      EmitVertex();
      EndPrimitive();
   }
}
