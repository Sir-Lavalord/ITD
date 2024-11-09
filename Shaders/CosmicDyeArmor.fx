sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
float3 uColor;
float3 uSecondaryColor;
float uOpacity;
float uSaturation;
float uRotation;
float uTime;
float4 uSourceRect;
float2 uWorldPosition;
float uDirection;
float3 uLightSource;
float2 uImageSize0;
float2 uImageSize1;
float2 uTargetPosition;
float4 uLegacyArmorSourceRect;
float2 uLegacyArmorSheetSize;

float4 CosmicDye(float4 sampleColor : COLOR0, float2 coords : TEXCOORD0) : COLOR0
{
    float4 color = tex2D(uImage0, coords);

    // current frame offset
    float frameOffsetY = (uSourceRect.y / uImageSize0.y);

    // pan based on world pos
    float2 overlayCoords = coords + (uWorldPosition / 2560.0f);
    overlayCoords.y -= frameOffsetY;
    
    // aspect ratio so it don't stretch
    overlayCoords.x *= uImageSize0.x / uImageSize1.x;
    overlayCoords.y *= uImageSize0.y / uImageSize1.y;

    // loop
    overlayCoords = frac(overlayCoords);

    // actually get the sample
    float4 overlayColor = tex2D(uImage1, overlayCoords);

    // used as base for blending. this is the exact same color as the base for spacemist
    float3 darkViolet = float3(0.22352941176f, 0.20392156862f, 0.34117647058f);
    
    float4 blended = color * float4(darkViolet, 1.0f);

    // ik you're not supposed to do if statements in shaders but i mean it work
    if (color.a > 0)
    {
        float4 result = lerp(blended, overlayColor, overlayColor.a);
        return result * sampleColor;
    }

    return blended * sampleColor;
}

 
technique Technique1
{
    pass CosmicDyePass
    {
        PixelShader = compile ps_2_0 CosmicDye();
    }
}