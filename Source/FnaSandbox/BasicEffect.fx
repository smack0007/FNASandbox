float4x4 Transform;

texture2D Texture;

sampler2D TextureSampler = sampler_state
{
    Texture = <Texture>;
    MinFilter = Linear;
    MagFilter = Linear;
    MipFilter = Linear;
    AddressU = Clamp;
    AddressV = Clamp;
    AddressW = Clamp;
};

struct VSData
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 UV : TEXCOORD0;
};

struct PSData
{
    float4 Position : POSITION0;
    float4 Color : COLOR0;
    float2 UV : TEXCOORD0;
};

PSData VSMain(VSData input)
{
    PSData output;
    output.Position = mul(input.Position, Transform);
    output.Color = input.Color;
    output.UV = input.UV;
    return output;
}

float4 PSMain(PSData input) : COLOR0
{
    return tex2D(TextureSampler, input.UV) * input.Color;
}

technique Main
{
    pass P0
    {        
        VertexShader = compile vs_2_0 VSMain();
        PixelShader = compile ps_2_0 PSMain();
    }
}