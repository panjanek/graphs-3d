#version 330 core

layout (location = 0) in vec4 aPosition;

uniform mat4 projection;

out float vDepth;

void main()
{
    vec4 clip = projection * aPosition;
    gl_Position = clip;

    // View-space depth for fog
    vDepth = -clip.z;
}