#version 430 core

in float vDepth;
in float vStretch;

out vec4 FragColor;

//uniform float uFogNear;
//uniform float uFogFar;
uniform vec3 uBaseColor;

void main()
{
    float uFogNear = 5.0;
    float uFogFar = 1000;

    float fog = clamp((uFogFar - vDepth) / (uFogFar - uFogNear), 0.0, 1.0);

    // Stretch visualization
    vec3 stretchColor = mix(
        vec3(0.2, 0.8, 1.0),   // compressed
        vec3(1.0, 0.3, 0.3),   // stretched
        clamp(vStretch * 5.0 + 0.5, 0.0, 1.0)
    );

    vec3 color = mix(uBaseColor, stretchColor, 0.6);
    FragColor = vec4(color * fog, fog);
}
