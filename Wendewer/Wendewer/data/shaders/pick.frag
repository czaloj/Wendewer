// Uniforms
uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;
uniform vec4 UUID;

void main() {
	gl_FragColor = UUID;
}