// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SeaShader"
{
	Properties
	{
		_PanSpeed("PanSpeed", Range( 0 , 1)) = 0.3
		_Texture("Texture", 2D) = "white" {}
		_TexScale("TexScale", Float) = 0
		_OpacityMultiplier("OpacityMultiplier", Range( 0 , 1)) = 0
		[HideInInspector] _texcoord( "", 2D ) = "white" {}
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Transparent"  "Queue" = "Transparent+0" "IgnoreProjector" = "True" "ForceNoShadowCasting" = "True" }
		Cull Back
		CGPROGRAM
		#include "UnityShaderVariables.cginc"
		#pragma target 4.6
		#pragma surface surf Standard alpha:fade keepalpha noshadow noambient novertexlights nolightmap  nodynlightmap nodirlightmap nofog nometa noforwardadd 
		struct Input
		{
			float2 uv_texcoord;
		};

		uniform sampler2D _Texture;
		uniform float _PanSpeed;
		uniform float _TexScale;
		uniform float _OpacityMultiplier;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float mulTime2 = _Time.y * _PanSpeed;
			float4 tex2DNode17 = tex2D( _Texture, ( ( i.uv_texcoord + mulTime2 ) * _TexScale ) );
			o.Albedo = ( tex2DNode17 * 2.5 ).rgb;
			o.Smoothness = 0.0;
			o.Alpha = ( tex2DNode17.r * _OpacityMultiplier );
		}

		ENDCG
	}
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18900
613;322;1025;676;836.9904;441.5948;1.388524;True;False
Node;AmplifyShaderEditor.RangedFloatNode;6;-1816.5,-117.5;Inherit;False;Property;_PanSpeed;PanSpeed;0;0;Create;True;0;0;0;False;0;False;0.3;0.017;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleTimeNode;2;-1060.5,-115.5;Inherit;False;1;0;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;18;-874.5275,-302.0356;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;22;-606.1132,-164.1228;Inherit;False;Property;_TexScale;TexScale;2;0;Create;True;0;0;0;False;0;False;0;5;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;20;-590.877,-266.7554;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;21;-453.8162,-227.4374;Inherit;False;2;2;0;FLOAT2;0,0;False;1;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.SamplerNode;17;-272.4722,-249.0691;Inherit;True;Property;_Texture;Texture;1;0;Create;True;0;0;0;False;0;False;-1;None;3da97c7446531544da09b8663a7e466d;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;26;-30.25795,27.72635;Inherit;False;Constant;_Float0;Float 0;3;0;Create;True;0;0;0;False;0;False;2.5;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;28;86.37811,320.7048;Inherit;False;Property;_OpacityMultiplier;OpacityMultiplier;3;0;Create;True;0;0;0;False;0;False;0;0.85;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.WorldPosInputsNode;3;-1302.9,-312.5361;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.DynamicAppendNode;4;-1094.9,-281.5361;Inherit;False;FLOAT4;4;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT4;0
Node;AmplifyShaderEditor.RangedFloatNode;7;-299.4337,116.7511;Inherit;False;Constant;_Smoothness;Smoothness;0;0;Create;True;0;0;0;False;0;False;0;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;104.5804,-303.6671;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;1;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;27;198.8486,229.0623;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;357.7019,-32.45769;Float;False;True;-1;6;ASEMaterialInspector;0;0;Standard;SeaShader;False;False;False;False;True;True;True;True;True;True;True;True;False;False;True;True;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Transparent;0.5;True;False;0;False;Transparent;;Transparent;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;45.4;10;25;False;0.5;False;2;5;False;-1;10;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;2;0;6;0
WireConnection;20;0;18;0
WireConnection;20;1;2;0
WireConnection;21;0;20;0
WireConnection;21;1;22;0
WireConnection;17;1;21;0
WireConnection;4;0;3;1
WireConnection;4;1;3;3
WireConnection;25;0;17;0
WireConnection;25;1;26;0
WireConnection;27;0;17;1
WireConnection;27;1;28;0
WireConnection;0;0;25;0
WireConnection;0;4;7;0
WireConnection;0;9;27;0
ASEEND*/
//CHKSM=8EFAA1238194903407B1CDD2C61A8FA80EA278E3