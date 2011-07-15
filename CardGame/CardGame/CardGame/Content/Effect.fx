float4x4 World;
float4x4 View;
float4x4 Projection;
float xOvercast;
float xTime;

Texture xTexture;

sampler TextureSampler = sampler_state 
{ 
	texture = <xTexture> ;
	magfilter = LINEAR; 
	minfilter = LINEAR; 
	mipfilter=LINEAR; 
	AddressU = mirror; 
	AddressV = mirror;
};

//------- Technique: PerlinNoise -------- 
 struct PNPixelToFrame
 {
     float4 Color : COLOR0;
 };
 
 PNPixelToFrame PerlinPS(float4 inPos : POSITION, float2 inTexCoords: TEXCOORD)
 {
     PNPixelToFrame Output = (PNPixelToFrame)0;    
     
     float2 move = float2(0,1);
     float4 perlin = tex2D(TextureSampler, (inTexCoords)+xTime*move)/2;
     perlin += tex2D(TextureSampler, (inTexCoords)*2+xTime*move)/4;
     perlin += tex2D(TextureSampler, (inTexCoords)*4+xTime*move)/8;
     perlin += tex2D(TextureSampler, (inTexCoords)*8+xTime*move)/16;
     perlin += tex2D(TextureSampler, (inTexCoords)*16+xTime*move)/32;
     perlin += tex2D(TextureSampler, (inTexCoords)*32+xTime*move)/32;    
     
     Output.Color.rgb = 1.0f-pow(perlin.r, xOvercast)*2.0f;
     Output.Color.a =1;
 
     return Output;
 }
 
 technique PerlinNoise
 {
     pass Pass0
     {
         PixelShader = compile ps_2_0 PerlinPS();
     }
 }
