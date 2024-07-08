Shader "PostProcess/Recall"
{
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"RenderPipeline" = "UniversalPipeline"
		}

		HLSLINCLUDE

		inline float randomValue(float2 uv)
		{
			return frac(sin(dot(uv, float2(12.9898, 78.233)))*43758.5453);
		}

		inline float perlinLerp(float a, float b, float t)
		{
			return (1.0f - t) * a + (t * b);
		}

		inline float valueNoise(float2 uv)
		{
			float2 i = floor(uv);
			float2 f = frac(uv);
			f = f * f * (3.0 - 2.0 * f);

			uv = abs(frac(uv) - 0.5);
			float2 c0 = i + float2(0.0, 0.0);
			float2 c1 = i + float2(1.0, 0.0);
			float2 c2 = i + float2(0.0, 1.0);
			float2 c3 = i + float2(1.0, 1.0);
			float r0 = randomValue(c0);
			float r1 = randomValue(c1);
			float r2 = randomValue(c2);
			float r3 = randomValue(c3);

			float bottomOfGrid = perlinLerp(r0, r1, f.x);
			float topOfGrid = perlinLerp(r2, r3, f.x);
			float t = perlinLerp(bottomOfGrid, topOfGrid, f.y);
			return t;
		}

		float perlinNoise(float2 uv, float scale)
		{
			float t = 0.0;
			float2 scaledUV = uv * scale;

			float freq = pow(2.0, float(0));
			float amp = pow(0.5, float(3 - 0));
			t += valueNoise(float2(scaledUV.x / freq, scaledUV.y / freq))*amp;

			freq = pow(2.0, float(1));
			amp = pow(0.5, float(3 - 1));
			t += valueNoise(float2(scaledUV.x / freq, scaledUV.y / freq))*amp;

			freq = pow(2.0, float(2));
			amp = pow(0.5, float(3 - 2));
			t += valueNoise(float2(scaledUV.x / freq, scaledUV.y / freq))*amp;

			return t;
		}

		ENDHLSL

		Pass
		{
			/*
			Stencil
			{
				Ref 1
				Comp NotEqual
				Pass Keep
			}
			*/

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
			float2 _WipeOriginPoint;
			float _WipeSize;
			float _WipeThickness;
			float _NoiseScale;
			float _NoiseStrength;
			float3 _EdgeColor;

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

				// Perform screen wipe to decide between greyscale colors.
				float2 offset = i.texcoord - _WipeOriginPoint;
				offset.x *= _ScreenParams.x / _ScreenParams.y;
				float distance = length(offset) + perlinNoise(i.texcoord, _NoiseScale) * _NoiseStrength;
				float isInWipeRadius = saturate(1.0f - step(_WipeSize, distance) - mask);

				float wipeRadiusEdge = step(_WipeSize - _WipeThickness, distance) * isInWipeRadius;

				col.rgb = lerp(col.rgb, Luminance(col.rgb), _Strength * (1.0f - mask) * isInWipeRadius);
				col.rgb = lerp(col.rgb, _EdgeColor, wipeRadiusEdge);

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

				col.rgb = lerp(col.rgb, _EdgeColor, edgeCol);

				return col;
            }
            ENDHLSL
        }
    }
}
