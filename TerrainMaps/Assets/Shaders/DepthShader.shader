Shader "Custom/DepthShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
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
        #pragma target 3.0

        sampler2D _MainTex;

		struct appdata
		{
			float4 vertex : POSITION;
			float3 normal : NORMAL;
			float2 uv : TEXCOORD0;
			float4 tangent : TANGENT;
		};

        struct Input
        {
            float2 uv_MainTex;
			float height;
			float3 normal : NORMAL;
        };

        half _Glossiness;
        half _Metallic;
        fixed4 _Color;

        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

		void vert(inout appdata v, out Input o)
		{
			float3 hh = tex2Dlod(_MainTex, float4 (v.uv, 0, 0));
			float delta = 0.01;
			float3 hhxp = tex2Dlod(_MainTex, float4 (v.uv + float2 (1, 0) * delta, 0, 0));
			float3 hhxn = tex2Dlod(_MainTex, float4 (v.uv + float2 (-1, 0) * delta, 0, 0));
			float3 hhyp = tex2Dlod(_MainTex, float4 (v.uv + float2 (0, 1) * delta, 0, 0));
			float3 hhyn = tex2Dlod(_MainTex, float4 (v.uv + float2 (0, -1) * delta, 0, 0));
			o.height = (hh.r * 256.0 + hh.g + hh.b / 256.0) - 128.0;
			float hhhxp = (hhxp.r * 256.0 + hhxp.g + hhxp.b / 256.0) - 128.0;
			float hhhxn = (hhxn.r * 256.0 + hhxn.g + hhxn.b / 256.0) - 128.0;
			float hhhyp = (hhyp.r * 256.0 + hhyp.g + hhyp.b / 256.0) - 128.0;
			float hhhyn = (hhyn.r * 256.0 + hhyn.g + hhyn.b / 256.0) - 128.0;
			if (o.height < 0)
			{
				o.height = 0;
				hhhxp = 0;
				hhhxn = 0;
				hhhyp = 0;
				hhhyn = 0;
			}
			v.vertex.y = -0.01 * o.height;
			o.normal = normalize (float3 (hhhyp - hhhyn, hhhxp - hhhxn, 0.0001));
			o.uv_MainTex = v.uv;
		}

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
			c = float4 (0.5, 0.5, 0.5, 0) + 0.5 * float4 (0, pow (abs (sin(IN.height * 10.0)), 10), 0, 1);
            o.Albedo = c.rgb;
			o.Normal = IN.normal;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
