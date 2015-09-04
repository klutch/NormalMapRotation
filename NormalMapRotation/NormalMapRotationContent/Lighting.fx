float3 LightDirection;
sampler TextureSampler : register(s0);
sampler NormalSampler : register(s1);

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float4 normalMap = tex2D(NormalSampler, texCoord);
	float3 n = normalize(2 * normalMap.rgb - 1);
	float3 l = normalize(LightDirection);
	float v = dot(n, l);

	return float4(v, v, v, 1);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
