
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
float2 positions [50];
int amount;

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

    float total = 0.;
    float3 col = float3(0,0,0);
    for (int i = 0; i < amount; i++)
    {   
        
        float dst = 0.1 * outlineThickness / length(texCoords - positions[i]);
        float4 SDF = float4(color * dst, dst);
        
        total += SDF.a;
        col += SDF.rgb;
    }
    
    
    col *= step(4.5,total/amount);
    
    return float4(col, col.r + col.g + col.b);
    
}

technique t0
{
    pass MetaballsPass
    {
        PixelShader = compile ps_3_0 ShaderPS();
    }
}