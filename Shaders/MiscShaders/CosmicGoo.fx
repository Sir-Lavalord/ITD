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

float2 Rotate(float2 uv, float amount)
{
    float2 uv2 = uv;
    float s = sin(amount);
    float c = cos(amount);
    uv2.x = (uv.x * c) + (uv.y * -s);
    uv2.y = (uv.x * s) + (uv.y * c);

    return uv2;
    
}



float4 CosmicGoo(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float3 CyanLaserColor = float3(168. / 255, 241. / 255, 255. / 255);
    float3 WhiteLaserColor = float3(255. / 255., 242. / 255, 191. / 255);
    float3 purpleLaserColor = float3(57. / 255, 52. / 255, 87. / 255);
    
    // centered uv
    float2 NormalUV = coords * 2. - 1.;
    float d = saturate(length(NormalUV));
   
    float4 noise = tex2D(uImage1, NormalUV * 2 + uTime);
    float circle = saturate((0.5 - distance(float2(0.5 * 2.0 - 1.0, 0.5 * 2.0 - 1.0), NormalUV)));
    circle *=1.;
    noise.rgb *= uSecondaryColor;
    
    noise.a = noise.r;

    float4 noise2 = tex2D(uImage1, Rotate(NormalUV, 3.141592 / 4) + uTime);
    noise *= noise2.r;
    
    
    return noise * circle + float3(circle * uSecondaryColor).rgbr;
}

technique Technique1
{
    pass CosmicGooPass
    {
        
        PixelShader = compile ps_3_0 CosmicGoo();
    }
}