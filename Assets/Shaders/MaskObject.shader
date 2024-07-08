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

			// This blend step is a hack because without it, overlapping objects do not set the correct mask value
			// for some goddamn reason.
			Blend SrcAlpha OneMinusSrcAlpha

			Tags
			{
				"LightMode" = "UniversalForward"
			}

			HLSLPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#pragma target 3.0

			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

			sampler2D _CameraDepthTexture;

            struct appdata
            {
                float4 positionOS : Position;
            };

            struct v2f
            {
                //float4 positionCS : SV_Position;
				float4 screenUV : TEXCOORD0;
				float depth : DEPTH;
            };

            v2f vert (appdata v, out float4 positionCS : SV_Position)
            {
                v2f o;
                positionCS = TransformObjectToHClip(v.positionOS.xyz);
				o.screenUV = ComputeScreenPos(positionCS);
				// From: https://gamedev.stackexchange.com/questions/157922/depth-intersection-shader
				o.depth = -mul(UNITY_MATRIX_MV, v.positionOS).z * _ProjectionParams.w;
                return o;
            }

			// Thanks to this for the note on using VPOS: https://gamedev.stackexchange.com/questions/157922/depth-intersection-shader
            float4 frag (v2f i, float4 positionSS : VPOS) : SV_Target
            {
				float2 screenUV = positionSS.xy / _ScreenParams.xy;
				float screenDepth = Linear01Depth(tex2D(_CameraDepthTexture, screenUV).r, _ZBufferParams);

				return step(i.depth, screenDepth);

				/*
				if (i.depth <= screenDepth)
				{
					return 1.0f;
				}

				return 0.0f;
				*/
            }
            ENDHLSL
        }
    }
}
