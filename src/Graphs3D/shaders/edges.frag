#version 330 core

in float vDepth;
out vec4 FragColor;


void main()
{
    float uFogNear = 5.0;
    float uFogFar = 1000;
    float fog = clamp((uFogFar - vDepth) / (uFogFar - uFogNear), 0.0, 1.0);

    vec3 color = mix(vec3(0.4, 0.4, 0.4), vec3(0.7, 0.9, 1.0), fog);
    FragColor = vec4(color, fog);
}
