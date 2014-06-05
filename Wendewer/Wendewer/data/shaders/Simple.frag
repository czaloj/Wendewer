// Uniforms
uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;

// Input
in vec4 fColor;

void main() {
	gl_FragColor = fColor;
}