#version 430 core

in float vDepth;
in float vStretch;

out vec4 FragColor;



void main()
{
    vec3 baseColor = vec3(1,1,1);

    //float fog = clamp((uFogFar - vDepth) / (uFogFar - uFogNear), 0.0, 1.0);

    // Stretch visualization
    vec3 stretchColor = mix(
        vec3(0.2, 0.8, 1.0),   // compressed
        vec3(1.0, 0.3, 0.3),   // stretched
        clamp(vStretch * 5.0 + 0.5, 0.0, 1.0)
    );

    vec3 color = mix(baseColor, stretchColor, 0.6);
    
    float fogDensity = 0.0005;
    float fog = exp(fogDensity * vDepth);
    fog = clamp(fog, 0.0, 1.0);


    FragColor = vec4(color * fog, fog);
}
