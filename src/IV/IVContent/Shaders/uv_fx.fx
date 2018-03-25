float4x4 World;
float4x4 View;
float4x4 Projection;
float3 CameraPosition;
// MULTI
//#define NUMLIGHTS 3
//float3 LightPosition[NUMLIGHTS];
//float3 LightColor[NUMLIGHTS];
//float LightAttenuation[NUMLIGHTS];
//------
float3 AmbientLightColor = float3(.15, .15, .15);
//float3 AmbientLightColor = float3(.15, .15, .15);
float3 DiffuseColor = float3(0.58,0.58,0.58);
float3 LightPosition;
float3 LightColor = float3(1, 1, 1);
float LightAttenuation = 30;
float SpecularPower = 32;
float3 SpecularColor = float3(1, 1, 1);


texture tex;

sampler BasicTextureSampler = sampler_state { 
	texture = <tex>; 
};

bool TextureEnabled = true;

struct VertexShaderInput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float3 Normal : NORMAL0;
};

struct VertexShaderOutput
{
    float4 Position : POSITION0;
    float2 UV : TEXCOORD0;
    float3 Normal : TEXCOORD1;
    float4 WorldPosition : TEXCOORD2;
	//
	float3 ViewDirection : TEXCOORD3;
};

VertexShaderOutput VertexShaderFunction(VertexShaderInput input)
{
    VertexShaderOutput output;

    float4 worldPosition = mul(input.Position, World);
    float4 viewPosition = mul(worldPosition, View);
    output.Position = mul(viewPosition, Projection);

    output.WorldPosition = worldPosition;

	output.UV = input.UV;

	output.Normal = mul(input.Normal, World);
	output.ViewDirection = worldPosition - CameraPosition;

    return output;
}

float4 PixelShaderFunction(VertexShaderOutput input) : COLOR0
{
	float3 diffuseColor = DiffuseColor;

	if (TextureEnabled)
		diffuseColor *= tex2D(BasicTextureSampler, input.UV).rgb;
	
	float3 totalLight = float3(0.1, 0.1, 0.1);
	
	totalLight += AmbientLightColor;
	// MULTI LIGHTING :
	//for (int i = 0; i < NUMLIGHTS; i++)
	//{
		//float3 lightDir = normalize(LightPosition[i] - input.WorldPosition);
		//float diffuse = saturate(dot(normalize(input.Normal), lightDir));
	//
		//float d = distance(LightPosition[i], input.WorldPosition);
		//float att = 1 - pow(clamp(d / LightAttenuation[i], 0, 1), 2); 
//
		//totalLight += diffuse * att * LightColor[i];
	//}
						//
						float3 normal = normalize(input.Normal);
						float3 view = normalize(input.ViewDirection);
						//
	
		float3 lightDir = normalize(LightPosition - input.WorldPosition);
		float diffuse = saturate(dot(normalize(input.Normal), lightDir));
	
		float d = distance(LightPosition, input.WorldPosition);
		float att = 1 - pow(clamp(d / LightAttenuation, 0, 1), 2); 
				//
				// Add lambertian lighting
				totalLight += (saturate(dot(lightDir, normal)) * LightColor)/2;

				float3 refl = reflect(lightDir, normal);
	
				// Add specular highlights
				totalLight += pow(saturate(dot(refl, view)), SpecularPower) * SpecularColor;
				//
		totalLight += diffuse * att * LightColor*2;
    return float4(diffuseColor * totalLight, 1);
}

technique Technique1
{
    pass Pass1
    {
        // TODO: set renderstates here.

        VertexShader = compile vs_2_0 VertexShaderFunction();
        PixelShader = compile ps_2_0 PixelShaderFunction();
    }

}
