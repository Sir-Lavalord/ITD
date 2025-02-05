float3 uColor;
float3 uSecondaryColor;
float uSaturation;
float uOpacity;
sampler2D uImage0 : register(s0);
sampler2D uImage1 : register(s1);
sampler2D uImage2 : register(s2);
float4 uShaderSpecificData;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float2 uImageSize0;
float2 uImageSize1;
float2 uImageSize2;



float4 CosmicBall(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    
    
    float2 waveyUV = float2(coords.x + cos(coords.y * 2 + uTime / 3) * sin(uTime / 3 + coords.y * 2) * 0.4, coords.y + uTime / 25) * 5;
    float2 waveyUV2 = float2(coords.x + (cos(coords.y + uTime) * 0.1) * (sin(uTime + coords.y) * 0.1), coords.
    y);

    float4 screen = tex2D(uImage0, coords);

    
    
    return (tex2D(uImage2, frac(waveyUV * 2) + float2(0, uTime)) + tex2D(uImage1, waveyUV) * uColor.rgbr) * screen;

}

technique Technique1
{
    pass CosmicBallPass
    {
        
        PixelShader = compile ps_3_0 CosmicBall();
    }
}