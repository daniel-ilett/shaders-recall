// Take a 2D seed value and output a random 2D vector.
inline float2 hash2D2D_float (float2 s)
{
	return frac(sin(fmod(float2(dot(s, float2(127.1,311.7)), dot(s, float2(269.5,183.3))), 3.14159))*43758.5453);
}

inline half2 hash2D2D_half (half2 s)
{
	return frac(sin(fmod(half2(dot(s, half2(127.1,311.7)), dot(s, half2(269.5,183.3))), 3.14159))*43758.5453);
}

// Take a 2D seed value and output a random 2D vector - this version works with custom function node.
void randomValue2D_float(float2 Seed, out float2 Rand)
{
    Rand = hash2D2D_float(Seed);
}

void randomValue2D_half(half2 Seed, out half2 Rand)
{
    Rand = hash2D2D_half(Seed);
}

void TextureTilingOffset_float(UnityTexture2D Texture, out float2 Tiling, out float2 Offset)
{
    Tiling = Texture.scaleTranslate.xy;
    Offset = Texture.scaleTranslate.zx;
}

void TextureTilingOffset_half(UnityTexture2D Texture, out half2 Tiling, out half2 Offset)
{
    Tiling = Texture.scaleTranslate.xy;
    Offset = Texture.scaleTranslate.zx;
}

// Sample a Texture2D and supply DX, DY gradients for choosing mipmap levels.
void SampleTexture2DGrad_float(UnityTexture2D Texture, UnitySamplerState Sampler, float2 UV, float DX, float DY, out float4 Color)
{
    Color = SAMPLE_TEXTURE2D_GRAD(Texture, Sampler, UV, DX, DY);
}

void SampleTexture2DGrad_half(UnityTexture2D Texture, UnitySamplerState Sampler, half2 UV, half DX, half DY, out half4 Color)
{
    Color = SAMPLE_TEXTURE2D_GRAD(Texture, Sampler, UV, DX, DY);
}

// Based on https://www.reddit.com/r/Unity3D/comments/dhr5g2/i_made_a_stochastic_texture_sampling_shader/
void ProceduralTexturing_float(UnityTexture2D Texture, UnitySamplerState Sampler, float2 UV, out float4 Color)
{
    float4x3 BW_vx;

    float2 skewUV = mul(float2x2(1.0f, 0.0f, -0.57735027f, 1.15470054f), UV * 3.464f);

    float2 vxID = float2 (floor(skewUV));
	float3 barycentric = float3 (frac(skewUV), 0);
	barycentric.z = 1.0 - barycentric.x - barycentric.y;

    BW_vx = ((barycentric.z > 0) ? 
		float4x3(float3(vxID, 0), float3(vxID + float2(0, 1), 0), float3(vxID + float2(1, 0), 0), barycentric.zyx) :
		float4x3(float3(vxID + float2 (1, 1), 0), float3(vxID + float2 (1, 0), 0), float3(vxID + float2 (0, 1), 0), float3(-barycentric.z, 1.0-barycentric.y, 1.0-barycentric.x)));
 
	//calculate derivatives to avoid triangular grid artifacts
	float2 dx = ddx(UV);
	float2 dy = ddy(UV);
 
	//blend samples with calculated weights
	Color = mul(SAMPLE_TEXTURE2D_GRAD(Texture, Sampler, UV + hash2D2D_float(BW_vx[0].xy), dx, dy), BW_vx[3].x) + 
			mul(SAMPLE_TEXTURE2D_GRAD(Texture, Sampler, UV + hash2D2D_float(BW_vx[1].xy), dx, dy), BW_vx[3].y) + 
			mul(SAMPLE_TEXTURE2D_GRAD(Texture, Sampler, UV + hash2D2D_float(BW_vx[2].xy), dx, dy), BW_vx[3].z);
}

void ProceduralTexturing_half(UnityTexture2D Texture, UnitySamplerState Sampler, half2 UV, out half4 Color)
{
    half4x3 BW_vx;

    half2 skewUV = mul(half2x2(1.0, 0.0, -0.57735027, 1.15470054), UV * 3.464f);

    half2 vxID = half2 (floor(skewUV));
	half3 barycentric = half3 (frac(skewUV), 0);
	barycentric.z = 1.0 - barycentric.x - barycentric.y;

    BW_vx = ((barycentric.z > 0) ? 
		half4x3(half3(vxID, 0), half3(vxID + half2(0, 1), 0), half3(vxID + half2(1, 0), 0), barycentric.zyx) :
		half4x3(half3(vxID + half2 (1, 1), 0), half3(vxID + half2 (1, 0), 0), half3(vxID + half2 (0, 1), 0), half3(-barycentric.z, 1.0-barycentric.y, 1.0-barycentric.x)));
 
	//calculate derivatives to avoid triangular grid artifacts
	half2 dx = ddx(UV);
	half2 dy = ddy(UV);
 
	//blend samples with calculated weights
	Color = mul(SAMPLE_TEXTURE2D_GRAD(Texture, Sampler, UV + hash2D2D_half(BW_vx[0].xy), dx, dy), BW_vx[3].x) + 
			mul(SAMPLE_TEXTURE2D_GRAD(Texture, Sampler, UV + hash2D2D_half(BW_vx[1].xy), dx, dy), BW_vx[3].y) + 
			mul(SAMPLE_TEXTURE2D_GRAD(Texture, Sampler, UV + hash2D2D_half(BW_vx[2].xy), dx, dy), BW_vx[3].z);
}
