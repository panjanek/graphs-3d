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
uniform mat4 view;
uniform vec2 viewportSize;

out float vDepth;
out vec3 vColor;
out float vEdgeDist;

void main()
{
    float baseLineWidth = 500.5;

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

    uint edgeIndex = uint(gl_VertexID) / 6u;
    uint corner    = uint(gl_VertexID) % 6u;

    const uint quadCorner[6] = uint[6](0u,1u,2u, 2u,1u,3u);
    uint qc = quadCorner[corner];

    Edge e = edges[edgeIndex];

    vec3 p0 = nodes[e.a].position.xyz;
    vec3 p1 = nodes[e.b].position.xyz;

    //--- vDepth
    vec4 v0 = view * vec4(p0, 1.0);
    vec4 v1 = view * vec4(p1, 1.0);
    bool isSecondPoint = (qc & 1u) != 0u;
    vDepth = isSecondPoint ? -v1.z : -v0.z;

    vec4 clip0 = projection * vec4(p0, 1.0);
    vec4 clip1 = projection * vec4(p1, 1.0);

    vec2 ndc0 = clip0.xy / clip0.w;
    vec2 ndc1 = clip1.xy / clip1.w;

    vec2 dir = normalize(ndc1 - ndc0);
    vec2 normal = vec2(-dir.y, dir.x);

    bool isTop         = (qc & 2u) != 0u;

    float depth = isSecondPoint ? -clip1.z : -clip0.z;
    float width = baseLineWidth / (depth + 1.0);

    vec2 offset = normal * (isTop ? width : -width) / viewportSize;

    vec4 clip = isSecondPoint ? clip1 : clip0;
    clip.xy += offset * clip.w;

    gl_Position = clip;

    float side = (isTop ? 1.0 : -1.0);
    vEdgeDist = side * width;

    vColor = colors[e.player % 8];
}