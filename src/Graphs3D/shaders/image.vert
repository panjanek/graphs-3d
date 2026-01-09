#version 430

out vec2 uv;

uniform vec2 offset;
uniform vec2 size;

void main()
{
    vec2 verts[6] = vec2[](
        vec2(0.0, 0.0),
        vec2(1.0, 0.0),
        vec2(1.0, 1.0),

        vec2(0.0, 0.0),
        vec2(1.0, 1.0),
        vec2(0.0, 1.0)
    );

    vec2 p = verts[gl_VertexID];

    uv = p;
    vec2 ndc = offset + p * size;
    gl_Position = vec4(ndc * 2.0 - 1.0, 0.0, 1.0);
}