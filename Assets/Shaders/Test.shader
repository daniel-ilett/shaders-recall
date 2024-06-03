Shader "Recall/Test"
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
				Comp Equal
				Pass Replace
				Fail Replace
			}

			//ColorMask 0
			//ZWrite Off
        }
    }
}
