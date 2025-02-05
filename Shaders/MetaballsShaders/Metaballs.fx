
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
float sizes[50];
int amount = 4;
float2 pixelization = float2(32,32);

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
    texCoords = round(texCoords * pixelization.x) / pixelization.y;

    float total = 0.;
    float4 col = float4(0,0,0,0);
    
    float aspect = screenResolution.x / screenResolution.y;
    
    float totalPower = 0.;
    float normal = 2;

    for (int i = 0; i < amount; i++)
    {   
        float2 pos = (texCoords.xy * screenResolution.xy / screenResolution.y - float2(positions[i].x * aspect, positions[i].y));
        pos = float2(abs(pos.x), abs(pos.y));
        float normalDist = pow(pos.x, normal) + pow(pos.y, normal);
        float power = pow(sizes[i], normal) / normalDist;
        
        totalPower += power;
        
        col += (color.rgb * power,1);
    }
    
    if (totalPower < 2.5 || totalPower > 2.5 + pow(1000, normal))
    {
        col = float4(0,0,0,0);
    }
    
    return tex2D(image0, texCoords) + col;
    
}

technique t0
{
    pass MetaballsPass
    {
        PixelShader = compile ps_3_0 ShaderPS();
    }
}