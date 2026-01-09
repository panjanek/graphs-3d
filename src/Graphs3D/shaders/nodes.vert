#version 430

struct Node
{
   vec4 position;
   vec4 velocity;
   vec4 prevForce;
   int player;
   int flags;
   int  cellIndex;
   int level;
   int leaf;
   int win;
   int _pad1;
   int _pad2;
};

layout(std430, binding = 2) buffer OutputBuffer {
    Node points[];
};

uniform mat4 view;
uniform mat4 projection;
uniform float paricleSize;
uniform vec2 viewportSize;
uniform vec4 trackedPos;

layout(location = 0) out vec3 vColor;
layout(location = 1) out float vDepth;
layout(location = 2) out float vFadingAlpha;
layout(location = 3) out vec3 vCenterView;
layout(location = 4) out vec2 vQuad;
layout(location = 5) in vec2 quadPos;
layout(location = 6) out vec3 vOffsetView;
layout(location = 7) out float vSphereRadiusMult;

float fading_alpha(float r2)
{
    float sigma2 = 500*500;
    float minAlpha = 0.5;
    float a = exp(-(r2) / sigma2);
    return max(a, minAlpha);
}

void main()
{
    float sphereRadius = 2 * paricleSize + (viewportSize.x/1920);
    uint id = gl_InstanceID;
    Node p = points[id];
    vSphereRadiusMult = 1.0;

    // player coloring
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
    int colorIdx = p.leaf == 0 ? p.player : p.win;
    vColor = colorIdx >= 0 ? colors[colorIdx % 8] : vec3(1.0, 0.0, 0.0);
    if (p.leaf == 1)
    {
        vSphereRadiusMult = 2;
        if (vColor.r == 0) vColor.r = 0.3;
        if (vColor.g == 0) vColor.g = 0.3;
        if (vColor.b == 0) vColor.b = 0.3;
    }

    //marker
    if (p.flags == 1) 
    {
        vSphereRadiusMult = 3;
        vColor = vec3(0.0, 0.0, 1.0);
    }

    sphereRadius *= vSphereRadiusMult;

    //hide
    if (p.flags == 2) sphereRadius = 0;

    //tracked
    vFadingAlpha = 1.0;
    if (trackedPos.x > -100000)
    {
        vec4 d = p.position - trackedPos;
        float r2 = dot(d, d);
        vFadingAlpha = fading_alpha(r2);
    }


    //real spheres
    vec4 viewPos = view * vec4(p.position.xyz, 1.0);
    vCenterView = viewPos.xyz;
    vQuad = quadPos;

    // In VIEW SPACE the camera basis is fixed
    float inflate = 1.5;
    vec3 offset = vec3(quadPos * sphereRadius * inflate, 0.0);

    vOffsetView = offset;

    vec4 pos = viewPos + vec4(offset, 0.0);
    gl_Position = projection * pos;
}