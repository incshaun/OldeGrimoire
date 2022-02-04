// Generate a periodic pattern projected from the light position.
Shader "Custom/StructuredLightShader"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Frequency ("Frequency", float) = 1.0
        _Phase ("Phase", float) = 3.14159
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        //#pragma surface surf Standard fullforwardshadows
        #pragma surface surf SimpleSpecular vertex:vert

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0

        sampler2D _MainTex;

        struct Input
        {
            float2 uv_MainTex;
            float3 PatternPos;
        };

        struct SurfaceOutputCustom {
            fixed3 Albedo;
            fixed3 Normal;
            fixed3 Emission;
            half Metallic;
            half Smoothness;
            half Specular;
            fixed Alpha;
            float3 worldPos;
        };
        
        half _Glossiness;
        half _Metallic;
        fixed4 _Color;
        float _Frequency;
        float _Phase;

        void vert (inout appdata_full v, out Input o) {
            UNITY_INITIALIZE_OUTPUT(Input,o);
            o.PatternPos = mul (unity_ObjectToWorld, v.vertex);
        }
        
        half4 LightingSimpleSpecular (SurfaceOutputCustom s, half3 lightDir, half3 viewDir, half atten) {
            half3 h = normalize (lightDir + viewDir);

            half diff = max (0, dot (s.Normal, lightDir));

            float nh = max (0, dot (s.Normal, h));
            float spec = pow (nh, 48.0);

            half4 c;
            
    #if defined (SPOT)
            float4 lightCoord = mul(unity_WorldToLight, float4 (s.worldPos, 1));
            float u = lightCoord.x / lightCoord.z; // avoid projection from spotlight's angle using .w
            atten = atten * 0.5 * (1.0 + sin (_Frequency * u + _Phase));
            
            // See if we can get a zero phase line.
//             if (abs (u) < 0.001)
//             {
//                 atten = 0;
//             }
//             else
//             {
//                 atten = 1;
//             }
    #endif 

            c.rgb = (s.Albedo * _LightColor0.rgb * diff + _LightColor0.rgb * spec) * atten;
            c.a = s.Alpha;
            return c;
        }
        
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputCustom o)
        {
            // Albedo comes from a texture tinted by color
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            // Metallic and smoothness come from slider variables
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;
            o.Alpha = c.a;
            o.worldPos = IN.PatternPos;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
