float SpriteRotation;
sampler TextureSampler : register(s0);

float4 PixelShaderFunction(float2 texCoord : TEXCOORD0, float4 color : COLOR0) : COLOR0
{
    float angle = SpriteRotation;
    float cosAngle = cos(angle);
    float sinAngle = sin(angle);
    float3x3 rotation = {
        cosAngle, -sinAngle, 0,
        sinAngle, cosAngle, 0,
        0, 0, 1
    };
    float4 normalMap = tex2D(TextureSampler, texCoord);
    float3 normal = 2 * normalMap.rgb - 1;
    float4 final = float4(1, 1, 1, normalMap.a);

    normal = mul(normal, rotation);
    final.rgb = (normal + 1) * 0.5f;
    final.rgb *= final.a;
    return final;
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}