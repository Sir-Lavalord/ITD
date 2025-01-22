#pragma warning (disable : 4717) 
sampler2D image1 : register(s1);
sampler2D image2 : register(s2);
sampler2D image3 : register(s3);
float4x4 viewWorldProjection;
float time;
float4 shaderData;
float3 color;

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

struct MetaballAttributes
{
    float color;
    float2 position;
    float raduis;
    float attraction;
    
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
    float4 color = tex2D(image1,texCoords);
    float circle = saturate((0.5 - distance(float2(0.5 * 2.0 - 1.0, 0.5 * 2.0 - 1.0), NormalUV)));

    return color * circle;
}


technique t0
{
    pass MetaballTestPass
    {
        VertexShader = compile vs_2_0 ShaderVS();
        PixelShader = compile ps_2_0 ShaderPS();
    }
}