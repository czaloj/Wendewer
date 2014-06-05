float4x4 World;
float4x4 View;
float4x4 Projection;

float3 Tint;

sampler2D Texture : register(s0);

struct VSI {
	float4 Position : POSITION0;
	float3 Normal : NORMAL0;
	float2 TexCoord : TEXCOORD0;
};
struct VSO {
	float4 Position : POSITION0;
	float2 TexCoord : TEXCOORD0;
};

VSO VS(VSI input) {
	VSO output;

	float4 worldPos = mul(input.Position, World);
	float4 viewPos = mul(worldPos, View);
	output.Position = mul(viewPos, Projection);
	output.TexCoord = input.TexCoord;

	return output;
}

float4 PS(VSO input) : COLOR0 {
	return float4(Tint, 1) * tex2D(Texture, input.TexCoord);
}

technique Default {
    pass Primary {
        VertexShader = compile vs_3_0 VS();
        PixelShader = compile ps_3_0 PS();
    }
}