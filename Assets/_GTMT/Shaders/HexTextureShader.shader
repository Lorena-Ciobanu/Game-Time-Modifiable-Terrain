﻿//https://docs.unity3d.com/Manual/SL-BuiltinMacros.html

Shader "Hex Grid/HexTextureShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Terrain Texture Array", 2DArray) = "white" {}
		_GridTex("Grid Texture", 2D) = "white" {}
		//_OutlineTex("Outline Texture", 2D) = "white" {}
		_HexRadius("Hex Dimension", float) = 1
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.5

		#pragma multi_compile _ GRID_ON

		UNITY_DECLARE_TEX2DARRAY(_MainTex);

        struct Input
        {
			float4 color: COLOR;
			float3 worldPos;
			float3 terrain;
			bool outline;
        };

		void vert(inout appdata_full v, out Input data) {
			UNITY_INITIALIZE_OUTPUT(Input, data);
			data.terrain = v.texcoord2.xyz;
		}

		float4 GetTerrainColor(Input IN, int index) {
			float3 uvw = float3(IN.worldPos.xz * 0.2, IN.terrain[index]);
			float4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTex, uvw);
			return c * IN.color[index];
		}

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
		sampler2D _GridTex;
	//	sampler2D _OutlineTex;
		float _HexRadius;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
			//float2 uv = IN.worldPos.xz * 0.04;					// Affect tiling (0.02)
            //fixed4 c = UNITY_SAMPLE_TEX2DARRAY(_MainTex, float3(uv, 0));

			fixed4 c = GetTerrainColor(IN, 0) + GetTerrainColor(IN, 1) + GetTerrainColor(IN, 2);
			

			fixed4 grid = 1;
			
			#if defined(GRID_ON)
				float2 gridUV = IN.worldPos.xz;
				gridUV.x *= 1 / (4 * _HexRadius * 0.866025404);
				gridUV.y *= 1 / (2 * _HexRadius * 1.50);
				grid = tex2D(_GridTex, gridUV);
			#endif



            o.Albedo = c.rgb * grid * _Color;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
