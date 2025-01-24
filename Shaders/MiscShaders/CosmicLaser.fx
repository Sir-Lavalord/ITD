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

float4 CosmicLaser(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    
    // use this uv to make the texture repeat in the X axis
    float2 NormalUV = coords * 2. - 1.;

    float2 RepeatedUV = float2(coords.x * (uShaderSpecificData.x / 256.), coords.y * 2. - 1.);
    // centered uv
    
    // thin start
    RepeatedUV.y *=  exp(1. /RepeatedUV.x / 10);
    
    float4 noise = tex2D(uImage1, RepeatedUV);
    noise.a = noise.r;
    

    
    // laser bloom
    float4 laser = 1 - clamp(abs(RepeatedUV.y) * 2, 0, 1);
    laser *= smoothstep(0.,1.,laser.y);
    laser.rgb *= uColor;

    //laser smoke 
    float4 smoke1 = tex2D(uImage1, (RepeatedUV * 2. - 1.) + float2(-uTime * 3, 0));
    smoke1.a = smoke1.r;
    smoke1.rgb *= uColor;
    smoke1 *= smoothstep(0., 1, 1 - abs(RepeatedUV.y * 2));

    // the magic, cool 3d stuff
    float4 smokeOverlay = saturate(tex2D(uImage1, RepeatedUV - float2(uTime *5, 0 )));
    
    smoke1 *= smokeOverlay.r;

    float4 finalColor = smoke1 + laser;

    return finalColor;
}

technique Technique1
{
    pass CosmicLaserPass
    {
        
        PixelShader = compile ps_2_0 CosmicLaser();
    }
}