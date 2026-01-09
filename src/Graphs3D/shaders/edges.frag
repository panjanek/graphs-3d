#version 430 core

in float vDepth;
in vec3 vColor;
in float vEdgeDist;
in vec2 vP0;
in vec2 vP1;

uniform float lineWidth;

out vec4 FragColor;

void main()
{
    // ----- analytic anti-aliasing -----
    /*
    float halfWidth = abs(vEdgeDist);

    // fwidth gives pixel-space derivative
    float aa = fwidth(vEdgeDist) * 1.5;

    float alphaEdge = 1.0 - smoothstep(
        halfWidth - aa,
        halfWidth + aa,
        abs(vEdgeDist)
    );

    alphaEdge = pow(alphaEdge, 0.8);
    */


    
    vec2 p = gl_FragCoord.xy;

    vec2 v = vP1 - vP0;
    vec2 w = p - vP0;

    float t = clamp(dot(w, v) / dot(v, v), 0.0, 1.0);
    vec2 closest = vP0 + t * v;

    float dist = length(p - closest);

    // signed distance
    float d = dist - (lineWidth-0.5);

    // analytic AA
    float aa = max(fwidth(d)*1.5, 1.0);
    float coverage = 1.0 - smoothstep(0.0, aa, d);
    float alphaEdge = coverage * 0.8;

    // ---- fog (correct) ----
    float fogDensity = 0.0005;
    float fog = exp(-fogDensity * vDepth);
    fog = clamp(fog, 0.0, 1.0);

    float alpha = alphaEdge * fog;
    FragColor = vec4(vColor * fog, alpha);
}
