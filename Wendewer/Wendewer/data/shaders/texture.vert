// Uniforms
uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;
uniform sampler2D Texture;

// Input
in vec4 vPosition;  // Sem  (Position    0)
in vec3 vNormal;    // Sem  (Normal      0)
in vec2 vUV;        // Sem  (Texcoord    0)
in vec4 vColor;     // Sem  (Color       0)

// Output
out vec4 fColor;
out vec2 fUV;

void main() {
	fColor = vColor;
    fUV = vUV;

	vec4 worldPos = vPosition * World;
	vec4 viewPos = worldPos * View;
	gl_Position = viewPos * Projection;
}