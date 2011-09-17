float4x4 World;
float4x4 View;
float4x4 Projection;
float xOvercast;
float xTime;
float2 xDir;
float2 centerCoord;        // 0.5,0.5 is the screen center


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

sampler2D texSample;

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
     
   // PSIn.TextureCoords.x = clamp(PSIn.TextureCoords.x, 0, 1);
   // PSIn.TextureCoords.y = clamp(PSIn.TextureCoords.y, 0, 1);

    float2 distance = PSIn.TextureCoords - centerCoord;  
    float wave = atan2(distance.y, distance.x)+ 3.1415;
    float len = length(distance);
    float sine = sin(wave);
    float cosine = cos(wave);
    
    float x = len % 0.7847;    
      
     float2 rotate = float2(cosine - x*sine, sine + x*cosine)/10;
     float2 move = (xDir) * xTime ;
     float4 perlin = tex2D(TextureSampler, (PSIn.TextureCoords+rotate)+move)/2;
     perlin += tex2D(TextureSampler, (PSIn.TextureCoords+rotate)*2+move)/4;
     perlin += tex2D(TextureSampler, (PSIn.TextureCoords+rotate)*4+move)/8;
     perlin += tex2D(TextureSampler, (PSIn.TextureCoords+rotate)*8+move)/16;
     perlin += tex2D(TextureSampler, (PSIn.TextureCoords+rotate)*16+move)/32;
     perlin += tex2D(TextureSampler, (PSIn.TextureCoords+rotate)*32+move)/32;

     Output.Color.rgb = 1;
     Output.Color.a =1.0f-pow(perlin.r, xOvercast)*2.0f;
 
     return Output;
 }
 
 PNPixelToFrame CloudBlendPS(PNVertexToPixel PSIn)
 {
     PNPixelToFrame Output = (PNPixelToFrame)0;   
     
     float4 c = tex2D(texSample, PSIn.TextureCoords);

     Output.Color.rgb = c;
    if (c.r == 1.0f && c.b == 1.0f && c.g == 0.0f)
		 Output.Color.a = 0;
	else 
		Output.Color.a = 1-c.a-0.2f;
 
     return Output;
 }
 
 PNPixelToFrame CloudColorBlendPS(PNVertexToPixel PSIn)
 {
     PNPixelToFrame Output = (PNPixelToFrame)0;   
     
     float4 c = tex2D(texSample, PSIn.TextureCoords);

     Output.Color.rgba = c;
     if (c.r == 1.0f && c.b == 1.0f && c.g == 0.0f)
		 Output.Color.a = 0;
	else if (c.a == 0)
		Output.Color.a = 0.1;
 
     return Output;
 }
 
 technique PerlinNoise
 {
     pass Pass0
     {
         PixelShader = compile ps_3_0 PerlinPS();
     }
 }
 
 technique CloudBlend
 {
	pass Pass0
	{
		AlphaBlendEnable = True;
		SrcBlend = Zero;
		DestBlend = One;
		StencilEnable = True;
		StencilFunc = Always;
		StencilPass = Incr;
		StencilFail = Incr;
		StencilRef = 1;
		
		PixelShader = compile ps_2_0 CloudBlendPS();
	}
 }
 
  
 technique CloudColorBlend
 {
	pass Pass0
	{
		AlphaBlendEnable = True;
		SrcBlend = One;
		DestBlend = Zero;
		StencilEnable = True;
		StencilFunc = Greater;
		StencilFail = Decr;
		StencilPass = Keep;
		StencilRef = 1;
		
		PixelShader = compile ps_2_0 CloudColorBlendPS();
	}
 }
