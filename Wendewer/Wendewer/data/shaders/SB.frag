// Uniforms
uniform mat4 World;
uniform mat4 VP;
uniform sampler2D Texture;

// Input
in vec2 fUV;
in vec4 fUVRect;
in vec4 fTint;

void main() {
    // Allow Repeating
    fUV = mod(fUV, 1);

    // Move Into New Texture Space
    fUV = fUV * fUVRect.zw + fUVRect.xy;

    // Sample Color
	gl_FragColor = texture(Texture, fUV) * fTint;
}