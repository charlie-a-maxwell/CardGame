float4x4 World;
float4x4 View;
float4x4 Projection;
float xOvercast;
float xTime;
float2 xDir;

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
 struct PNVertexToPixel
 {    
     float4 Position         : POSITION;
     float2 TextureCoords    : TEXCOORD0;
 };
 
 struct PNPixelToFrame
 {
     float4 Color : COLOR0;
 };
 
 PNPixelToFrame PerlinPS(PNVertexToPixel PSIn) : COLOR0
 {
     PNPixelToFrame Output = (PNPixelToFrame)0;    
     
     float2 move = xDir;
     float4 perlin = tex2D(TextureSampler, (PSIn.TextureCoords)+xTime*move)/2;
     perlin += tex2D(TextureSampler, (PSIn.TextureCoords)*2+xTime*move)/4;
     perlin += tex2D(TextureSampler, (PSIn.TextureCoords)*4+xTime*move)/8;
     perlin += tex2D(TextureSampler, (PSIn.TextureCoords)*8+xTime*move)/16;
     perlin += tex2D(TextureSampler, (PSIn.TextureCoords)*16+xTime*move)/32;
     perlin += tex2D(TextureSampler, (PSIn.TextureCoords)*32+xTime*move)/32;    
     
     Output.Color.rgb = 1;
     Output.Color.a =1.0f-pow(perlin.r, xOvercast)*2.0f;
 
     return Output;
 }
 
 technique PerlinNoise
 {
     pass Pass0
     {
         PixelShader = compile ps_2_0 PerlinPS();
     }
 }
