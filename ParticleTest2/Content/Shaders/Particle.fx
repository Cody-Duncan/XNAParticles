float4x4 view;
float4x4 proj;
float4x4 world;

float sizeModifier : PARTICLE_SIZE = 3.5f;
texture textureMap : DiffuseMap;  // texture for scene rendering
sampler textureSampler= 
sampler_state
{
    Texture = <textureMap>;
    AddressU  = CLAMP;        
    AddressV  = CLAMP;
    MIPFILTER = LINEAR;
    MINFILTER = LINEAR;
    MAGFILTER = LINEAR;
};

texture positionMap;
sampler positionSampler = sampler_state
{
    Texture   = <positionMap>;
    MipFilter = None;
    MinFilter = Point;
    MagFilter = Point;
    AddressU  = Clamp;
    AddressV  = Clamp;
};

struct VS_INPUT 
{
    float4 vertexData	: POSITION;
    float4 color		: COLOR0;
};

struct VS_OUTPUT
{
	float4 position  : POSITION;
	float4 color	 : COLOR0;
	float Size		 : PSIZE0;
};

struct PS_INPUT
{
    #ifdef XBOX
        float4 textureCoordinate : SPRITETEXCOORD;
    #else
        float2 textureCoordinate : TEXCOORD0;
    #endif
    float4 Color : COLOR0;
};




float screenHeight = 600;
 
VS_OUTPUT Transform(VS_INPUT In)
{
    VS_OUTPUT Out = (VS_OUTPUT)0;
    float4x4 worldView= mul(world, view);
    float4x4 WorldViewProj=mul(worldView, proj);

    // Transform the position from object space to homogeneous projection space
    float4 realPosition = tex2Dlod ( positionSampler, float4(In.vertexData.x, In.vertexData.y,0,0));
    
    Out.color = In.color;
    realPosition.w = 1;          //we need to set this to 1, because the value read from the texture is the life of the particle, which doesn't interest us here.
    Out.position = mul( realPosition , WorldViewProj);
	Out.Size = 3.0;
    return Out;
    
}

//the pixel shader draws the particle with the specified color. 
float4 ApplyTexture(PS_INPUT input) : COLOR
{
	float2 textureCoordinate;
	     #ifdef XBOX
            textureCoordinate = abs(input.textureCoordinate.zw);
        #else
            textureCoordinate = input.textureCoordinate.xy;
        #endif
    
	float4 col=tex2D(textureSampler, textureCoordinate) * input.Color;

	col.b *= 3;
	col.r *= 1.4;
	
	return col;
}
    
technique TransformAndTexture
{
    pass P0
    {
        vertexShader = compile vs_3_0 Transform();
        pixelShader  = compile ps_3_0 ApplyTexture();
    }
}