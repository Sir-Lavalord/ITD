
#pragma warning (disable : 4717) 
sampler2D image0 : register(s0);
sampler2D image1 : register(s1);
sampler2D image2 : register(s2);
sampler2D image3 : register(s3);
float4x4 viewWorldProjection;
float time;
float4 shaderData;
float3 color;
float outlineThickness;
float2 screenPosition;
float3 outlineColor;
float2 screenResolution;
float2 positions[50];

struct VertexShaderInput
{
    float4 pos : POSITION0;
    float4 col : COLOR0;
    float2 texCoord : TEXCOORD0;
};

struct VertexShaderOutput
{
    float4 pos : SV_POSITION;
    float4 col : COLOR0;
    float2 texCoord : TEXCOORD0;
};

VertexShaderOutput ShaderVS(VertexShaderInput input)
{
    
    VertexShaderOutput output = (VertexShaderOutput) 0;

    output.pos = mul(input.pos, viewWorldProjection);
    output.col = input.col;
    output.texCoord = input.texCoord;
    
    return output;

}


float4 ShaderPS(float4 vertexColor : COLOR0, float2 texCoords : TEXCOORD0) : COLOR0
{
    
    //apply an outline effect
    
    const float TwoPI = 3.141592 * 2.;
    const float steps = 128.;
    float2 UV = texCoords;
    
    float4 col1 = float4(0, 0., 0., 0.);
    for (float i = 0.; i < TwoPI; i += TwoPI / steps)
    {
        float2 offset = float2(sin(i), cos(i)) * outlineThickness;
        col1 += tex2D(image0, UV + offset * 0.0001) * float4(outlineColor.rgb, 1);
        

    }
    return (tex2D(image0, UV) * float4(color.rgb, 1) * 30) + col1;
}

technique t0
{
    pass FakeMetaballsPass
    {
        PixelShader = compile ps_3_0 ShaderPS();
    }
}