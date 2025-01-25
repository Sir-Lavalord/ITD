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



float4 Blackhole(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
        
    // centered uv
    float2 NormalUV = coords;
    NormalUV -= 0.5;
    NormalUV *= 2.;
    float2 pixelatedUV = round(NormalUV * (128.)) / 128.;

    float d = length(pixelatedUV);
    float angle = atan2(pixelatedUV.y, pixelatedUV.x);
    float2 VortexUV = float2(sin(angle + d * 5 - uTime * 3), d + uTime);
    float circle = saturate((0.5 - distance(float2(0.5 * 2.0 - 1.0, 0.5 * 2.0 - 1.0), NormalUV)));
    float4 finalCol = tex2D(uImage1, VortexUV) * lerp(uSecondaryColor, uColor, saturate(VortexUV.x)).rgbr * smoothstep(1, 0., d);
    finalCol = round(finalCol * (16)) / 16;
    return finalCol * 3 - circle * 15;
}

technique Technique1
{
    pass BlackholePass
    {
        
        PixelShader = compile ps_3_0 Blackhole();
    }
}