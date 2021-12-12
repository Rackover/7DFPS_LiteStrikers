// Made with Amplify Shader Editor
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "LevelShader"
{
	Properties
	{
		_Color("Color", Color) = (1,1,1,1)
		_Spread("Spread", Float) = 0.03
		_Offset("Offset", Float) = 0
		[HideInInspector] __dirty( "", Int ) = 1
	}

	SubShader
	{
		Tags{ "RenderType" = "Opaque"  "Queue" = "Geometry+0" "IgnoreProjector" = "True" }
		Cull Back
		CGPROGRAM
		#pragma target 3.0
		#pragma surface surf Standard keepalpha nodirlightmap 
		struct Input
		{
			float3 worldPos;
		};

		uniform float4 _Color;
		uniform float _Offset;
		uniform float _Spread;

		void surf( Input i , inout SurfaceOutputStandard o )
		{
			float3 ase_worldPos = i.worldPos;
			float clampResult19 = clamp( ( ( ase_worldPos.y + _Offset ) * _Spread ) , -100.0 , 1.0 );
			float4 lerpResult21 = lerp( float4( 0,0,0,0 ) , _Color , clampResult19);
			o.Albedo = lerpResult21.rgb;
			o.Alpha = 1;
		}

		ENDCG
	}
	Fallback "Diffuse"
	CustomEditor "ASEMaterialInspector"
}
/*ASEBEGIN
Version=18900
547;279;1025;676;1125.283;243.55;1.719529;True;False
Node;AmplifyShaderEditor.WorldPosInputsNode;20;-895.5166,190.8675;Inherit;False;0;4;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3
Node;AmplifyShaderEditor.RangedFloatNode;26;-709.1569,423.6271;Inherit;False;Property;_Offset;Offset;2;0;Create;True;0;0;0;False;0;False;0;542.34;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;24;-549.2396,482.0913;Inherit;False;Property;_Spread;Spread;1;0;Create;True;0;0;0;False;0;False;0.03;0.001;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;25;-661.0101,325.614;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;23;-430.7396,309.7664;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0.03;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;16;-363.9047,-188.9136;Inherit;False;Property;_Color;Color;0;0;Create;True;0;0;0;False;0;False;1,1,1,1;0.9258771,0.740566,0.4252958,1;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ClampOpNode;19;-259.0618,35.3303;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;-100;False;2;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;21;109.4156,-70.09582;Inherit;False;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.StandardSurfaceOutputNode;0;474.3639,5.587233;Float;False;True;-1;2;ASEMaterialInspector;0;0;Standard;LevelShader;False;False;False;False;False;False;False;False;True;False;False;False;False;False;True;False;False;False;False;False;False;Back;0;False;-1;0;False;-1;False;0;False;-1;0;False;-1;False;0;Opaque;0.5;True;False;0;False;Opaque;;Geometry;All;14;all;True;True;True;True;0;False;-1;False;0;False;-1;255;False;-1;255;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;-1;False;2;45.4;10;25;False;0.5;True;0;0;False;-1;0;False;-1;0;0;False;-1;0;False;-1;0;False;-1;0;False;-1;0;False;0;0,0,0,0;VertexOffset;True;False;Cylindrical;False;Relative;0;;-1;-1;-1;-1;0;False;0;0;False;-1;-1;0;False;-1;0;0;0;False;0.1;False;-1;0;False;-1;False;16;0;FLOAT3;0,0,0;False;1;FLOAT3;0,0,0;False;2;FLOAT3;0,0,0;False;3;FLOAT;0;False;4;FLOAT;0;False;5;FLOAT;0;False;6;FLOAT3;0,0,0;False;7;FLOAT3;0,0,0;False;8;FLOAT;0;False;9;FLOAT;0;False;10;FLOAT;0;False;13;FLOAT3;0,0,0;False;11;FLOAT3;0,0,0;False;12;FLOAT3;0,0,0;False;14;FLOAT4;0,0,0,0;False;15;FLOAT3;0,0,0;False;0
WireConnection;25;0;20;2
WireConnection;25;1;26;0
WireConnection;23;0;25;0
WireConnection;23;1;24;0
WireConnection;19;0;23;0
WireConnection;21;1;16;0
WireConnection;21;2;19;0
WireConnection;0;0;21;0
ASEEND*/
//CHKSM=2082D00B7C7FC6A87458AC0A99BA8CB957708945