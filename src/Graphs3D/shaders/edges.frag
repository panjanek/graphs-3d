#version 430 core

in float vDepth;
in vec3 vColor;

out vec4 FragColor;

void main()
{
    vec3 color = vColor;
    
    float fogDensity = 0.0005;
    float fog = exp(fogDensity * vDepth);
    fog = clamp(fog, 0.0, 1.0);

    FragColor = vec4(color * fog, fog);
}
