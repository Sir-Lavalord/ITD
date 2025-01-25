float3 uColor;
float3 uSecondaryColor;
float uSaturation;
float uOpacity;
sampler2D uImage0;
sampler2D uImage1;
sampler2D uImage2;
float4 uShaderSpecificData;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float2 uImageSize0;
float2 uImageSize1;
float2 uImageSize2;

float4 Telegraph(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    
    float2 uv = float2(coords.x - 0.5, coords.y * 2. - 1.);

    float4 mask = tex2D(uImage0, float2(coords.x * 30 - uTime * 1.25, coords.y));
    mask.a = mask.r;
    float4 Color1 = float4(tex2D(uImage0, float2(coords.x * 35 - uTime, coords.y)));
    Color1 *= uColor.rgbr * mask.a;
    float4 Color2 = float4(tex2D(uImage0, float2(coords.x * 35, coords.y)) * step(0.75, abs(uv.y)));

    float4 Color3 = abs(uv.y) * uColor.rgbr;



    return saturate(Color1 + Color2 + Color3) * length(coords.x) * 5;
}

technique Technique1
{
    pass TelegraphPass
    {
        
        PixelShader = compile ps_2_0 Telegraph();
    }
}