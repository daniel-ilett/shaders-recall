Shader "Recall/MaskObject"
{
	SubShader
	{
		Tags
		{
			"RenderType" = "Opaque"
			"Queue" = "Geometry"
			"RenderPipeline" = "UniversalPipeline"
		}

		Pass
		{
			ZTest LEqual
			ZWrite On

			// Blending means that we can render black and not worry about needing to check if we
			// already have white in the mask.
			Blend SrcColor OneMinusSrcColor

			Tags
			{
				"LightMode" = "UniversalForward"
			}

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			sampler2D _CameraDepthTexture;

            struct appdata
            {
                float4 positionOS : Position;
            };

            struct v2f
            {
				float depth : DEPTH;
            };

            v2f vert (appdata v, out float4 positionCS : SV_Position)
            {
                v2f o;
                positionCS = TransformObjectToHClip(v.positionOS.xyz);
				// From: https://gamedev.stackexchange.com/questions/157922/depth-intersection-shader
				o.depth = -mul(UNITY_MATRIX_MV, v.positionOS).z * _ProjectionParams.w;
                return o;
            }

			// Thanks to this for the note on using VPOS: https://gamedev.stackexchange.com/questions/157922/depth-intersection-shader
            float4 frag (v2f i, float4 positionSS : VPOS) : SV_Target
            {
				float2 screenUV = positionSS.xy / _ScreenParams.xy;
				float screenDepth = Linear01Depth(tex2D(_CameraDepthTexture, screenUV).r, _ZBufferParams);

				return step(i.depth - 0.0001f, screenDepth);
            }
            ENDHLSL
        }
    }
}
