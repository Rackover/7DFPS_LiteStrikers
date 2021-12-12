//Radial blur shader by puppet_master
//2017.2.17
Shader "ApcShader/PostEffect/RadialBlurShader2"
{
	Properties
	{
		_MainTex("Base (RGB)", 2D) = "white" {}
	}

		CGINCLUDE
		uniform sampler2D _MainTex;
	uniform float _BlurFactor;//Blur strength (0-0.05)
	uniform float4 _BlurCenter;//Blur center point xy value (0-1) screen space
#include "UnityCG.cginc"
#define SAMPLE_COUNT 6//Number of iterations

	fixed4 frag(v2f_img i) : SV_Target
	{
		//The blur direction is the point where the midpoint of the blur points to the edge (the current pixel), and the greater the value along the edge, the more blurry
		float2 dir = i.uv - _BlurCenter.xy;
		float4 outColor = 0;
		//sample SAMPLE_COUNT times
		for (int j = 0; j < SAMPLE_COUNT; ++j)
		{
			//Calculate the sampled uv value: normal uv value + sampling distance gradually increasing from the middle to the edge
			float2 uv = i.uv + _BlurFactor * dir * j;
			outColor += tex2D(_MainTex, uv);
		}
		//take the average
		outColor /= SAMPLE_COUNT;
		return outColor;
	}
		ENDCG

		SubShader
	{
		Pass
		{
			ZTest Always
			Cull Off
			ZWrite Off
			Fog {Mode off}

			//Call CG function	
			CGPROGRAM
			//Make the compiled macro more efficient
			#pragma fragmentoption ARB_precision_hint_fastest 
			//vert_img is defined in UnityCG.cginc, when the post-processing vert stage calculation routine, you can directly use the built-in vert_img
			#pragma vertex vert_img
			#pragma fragment frag 
			ENDCG
		}
	}
	Fallback off
}