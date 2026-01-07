#version 430 core

in float vDepth;
in vec3 vColor;
in float vEdgeDist;

out vec4 FragColor;

void main()
{
    // ----- analytic anti-aliasing -----
    float halfWidth = abs(vEdgeDist);

    // fwidth gives pixel-space derivative
    float aa = fwidth(vEdgeDist) * 1.5;

    float alphaEdge = 1.0 - smoothstep(
        halfWidth - aa,
        halfWidth + aa,
        abs(vEdgeDist)
    );

    alphaEdge = pow(alphaEdge, 0.5);

    // ---- fog (correct) ----
    float fogDensity = 0.0005;
    float fog = exp(-fogDensity * vDepth);
    fog = clamp(fog, 0.0, 1.0);

    float alpha = alphaEdge * fog;
    FragColor = vec4(vColor * fog, alpha);
}
