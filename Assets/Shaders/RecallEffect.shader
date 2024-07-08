Shader "PostProcess/Recall"
{
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"RenderPipeline" = "UniversalPipeline"
		}

		Pass
		{
			Stencil
			{
				Ref 1
				Comp NotEqual
				Pass Keep
			}

			HLSLPROGRAM
			#pragma vertex Vert
			#pragma fragment frag

#if SHADER_API_D3D11
		#define STENCIL_CHANNEL g
#else
		#define STENCIL_CHANNEL r
#endif

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

			Texture2D<uint2> _DepthTexture;
			//TEXTURE2D(_DepthTexture);

			TEXTURE2D(_MaskedObjects);

			float _Strength;

            float4 frag (Varyings i) : SV_Target
            {
				//return SAMPLE_TEXTURE2D(_MaskedObjects, sampler_LinearClamp, i.texcoord);
				//uint depthStencilValue = LOAD_TEXTURE2D(_DepthTexture, int2(floor(i.texcoord * _ScreenSize.xy))).STENCIL_CHANNEL;

				//uint x = LOAD_TEXTURE2D(_DepthTexture, int2(floor(i.texcoord * _ScreenParams.xy))).g;
				//return float4(x, x, x, 1);

				//float4 depthValue = SAMPLE_TEXTURE2D(_DepthTexture, sampler_LinearClamp, i.texcoord);
				//return depthValue;

				// Calculate the mask and pick between regular and greyscale colors.
				float mask = SAMPLE_TEXTURE2D(_MaskedObjects, sampler_LinearClamp, i.texcoord).r;
				float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.texcoord);
				col.rgb = lerp(col.rgb, Luminance(col.rgb), _Strength * (1.0f - mask));

				// Perform outline detection step
				float2 leftUV = i.texcoord + float2(1.0f / -_ScreenParams.x, 0.0f);
				float2 rightUV = i.texcoord + float2(1.0f / _ScreenParams.x, 0.0f);
				float2 bottomUV = i.texcoord + float2(0.0f, 1.0f / -_ScreenParams.y);
				float2 topUV = i.texcoord + float2(0.0f, 1.0f / _ScreenParams.y);

				float3 col0 = SAMPLE_TEXTURE2D(_MaskedObjects, sampler_LinearClamp, leftUV).rgb;
				float3 col1 = SAMPLE_TEXTURE2D(_MaskedObjects, sampler_LinearClamp, rightUV).rgb;
				float3 col2 = SAMPLE_TEXTURE2D(_MaskedObjects, sampler_LinearClamp, bottomUV).rgb;
				float3 col3 = SAMPLE_TEXTURE2D(_MaskedObjects, sampler_LinearClamp, topUV).rgb;

				float3 c0 = col1 - col0;
				float3 c1 = col3 - col2;

				float edgeCol = sqrt(dot(c0, c0) + dot(c1, c1));
				edgeCol = step(0.1f, edgeCol);

				col.rgb = lerp(col.rgb, float3(1.0f, 1.0f, 0.0f), edgeCol);

				return col;
            }
            ENDHLSL
        }
    }
}
