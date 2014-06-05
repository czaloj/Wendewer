// Uniforms
uniform mat4 World;
uniform mat4 View;
uniform mat4 Projection;
uniform sampler2D Texture;
uniform vec3 Tint;

// Input
in vec4 fColor;
in vec2 fUV;

void main() {
	gl_FragColor = texture(Texture, fUV) * vec4(Tint, 1);
}