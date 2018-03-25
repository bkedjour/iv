sampler blurred : register(s0);
float blurPOWER = 0.002;
texture textureToRender;
sampler blurred2 = sampler_state
{
	texture = <textureToRender>;
};
struct VertexShaderOutput
{
    float4 Tex : TEXCOORD0;
};

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{

float4 color = 0;
		for (int i=1 ; i<2 ; i++)
		{
		color += tex2D( blurred,float2(input.Tex.x+blurPOWER*i, input.Tex.y+blurPOWER*i));
		color += tex2D( blurred,float2(input.Tex.x-blurPOWER*i, input.Tex.y-blurPOWER*i));
		color += tex2D( blurred,float2(input.Tex.x+blurPOWER*i, input.Tex.y-blurPOWER*i));
		color += tex2D( blurred,float2(input.Tex.x-blurPOWER*i, input.Tex.y+blurPOWER*i));
		}
		color = color / (8);
		
float4 color2 = tex2D( blurred2, input.Tex);
return( (color*1.02f) + color2);
}

technique Technique1
{
    pass Pass1
    {
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }
}
