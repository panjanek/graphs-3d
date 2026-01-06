#version 430 core

struct Node
{
   vec4 position;
   vec4 velocity;
   int species;
   int flags;
   int  cellIndex;
   int  _pad1;
};

struct Edge
{
    uint a;
    uint b;
    float restLength;
    int player;
    int flags;
};

layout(std430, binding = 2) buffer NodesBuffer {
    Node nodes[];
};

layout(std430, binding = 3) buffer EdgesBuffer {
    Edge edges[];
};

uniform mat4 projection;

out float vDepth;
out float vStretch;

void main()
{
    uint edgeIndex = uint(gl_VertexID) >> 1;
    bool isSecond = (gl_VertexID & 1) == 1;

    Edge e = edges[edgeIndex];
    uint nodeIndex = isSecond ? e.b : e.a;

    vec3 pos = nodes[nodeIndex].position.xyz;

    vec4 clip = projection * vec4(pos, 1.0);
    gl_Position = clip;
    vDepth = -clip.z;

    // Optional: visualize edge stretch
    vec3 pa = nodes[e.a].position.xyz;
    vec3 pb = nodes[e.b].position.xyz;
    float len = length(pb - pa);
    vStretch = (len - e.restLength) / e.restLength;
}