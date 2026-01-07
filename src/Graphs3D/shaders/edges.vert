#version 430 core

struct Node
{
   vec4 position;
   vec4 velocity;
   vec4 prevForce;
   int player;
   int flags;
   int  cellIndex;
   int level;
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
out vec3 vColor;

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

    const vec3 colors[] = vec3[](
        vec3(1.0, 1.0, 0.0), // yellow
        vec3(1.0, 0.0, 1.0), // magenta
        vec3(0.0, 1.0, 1.0), // cyan
        vec3(1.0, 0.0, 0.0), // red
        vec3(0.0, 1.0, 0.0), // green
        vec3(0.0, 0.0, 1.0), // blue
        vec3(1.0, 1.0, 1.0), // white
        vec3(0.5, 0.5, 0.5)  // gray
    );

    vColor = colors[e.player % 8];
}