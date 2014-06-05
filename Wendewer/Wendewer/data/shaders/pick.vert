// Uniforms
uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

// Input
in vec4 vPosition;  // Sem  (Position   0)

void main() {
	vec4 worldPos = vPosition * World;
	vec4 viewPos = worldPos * View;
	gl_Position = viewPos * Projection;
}