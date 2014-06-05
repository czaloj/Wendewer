// Uniforms
uniform mat4 World;
uniform mat4 VP;
uniform sampler2D Texture;

// Input
in vec4 vPosition;  // Sem  (Position    0)
in vec2 vUV;        // Sem  (Texcoord    0)
in vec4 vUVRect;    // Sem  (Texcoord    1)
in vec4 vTint;      // Sem  (Color       0)

// Output
out vec2 fUV;
out vec4 fUVRect;
out vec4 fTint;

void main() {
    // Pass Coloring Information
	fTint = vTint;
    fUV = vUV;
    fUVRect = vUVRect;

    // Project Onto The Screen
	vec4 worldPos = vPosition * World;
	gl_Position = worldPos * VP;
}