Shader "Recall/Greyscale"
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

				float mask = SAMPLE_TEXTURE2D(_MaskedObjects, sampler_LinearClamp, i.texcoord).r;

				float4 col = SAMPLE_TEXTURE2D(_BlitTexture, sampler_LinearClamp, i.texcoord);

				if (mask < 0.5f)
				{
					col.rgb = lerp(col.rgb, Luminance(col.rgb), _Strength);
				}

				return col;
            }
            ENDHLSL
        }
    }
}
