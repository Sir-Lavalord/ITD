﻿sampler uImage0 : register(s0);
sampler uImage1 : register(s1);
sampler uImage2 : register(s2);
sampler uImage3 : register(s3);
float3 uColor;
float3 uSecondaryColor;
float2 uScreenResolution;
float2 uScreenPosition;
float2 uTargetPosition;
float2 uDirection;
float uOpacity;
float uTime;
float uIntensity;
float uProgress;
float2 uImageSize1;
float2 uImageSize2;
float2 uImageSize3;
float2 uImageOffset;
float uSaturation;
float4 uSourceRect;
float2 uZoom;

float4 BlackMold(float2 coords : TEXCOORD0) : COLOR0
{
    float waveFrequency = 20.0;
    float waveAmplitude = 0.08;
    float waveSpeedMult = 2.0;
    
	float2 middle = {0.5f, 0.5f};
	float toMiddle = distance(coords, middle);
	
    coords.y += sin(toMiddle * waveFrequency + uTime * waveSpeedMult) * toMiddle * waveAmplitude * uOpacity;

    float4 color = tex2D(uImage0, coords);

    return color;
}

technique Technique1
{
    pass BlackMoldPass
    {
        PixelShader = compile ps_2_0 BlackMold();
    }
}