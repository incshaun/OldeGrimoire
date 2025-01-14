Shader "Unlit/StereoTexture"
{
    Properties
    {
        LeftTex ("Texture", 2D) = "white" {}
        RightTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 100
        Cull Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
    
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
                float4 objvertex : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID 
                UNITY_VERTEX_OUTPUT_STEREO
            };

            sampler2D LeftTex;
            sampler2D RightTex;
            float4 LeftTex_ST;
            float4 RightTex_ST;
            
            v2f vert (appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_OUTPUT(v2f, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o)
                o.objvertex = v.vertex;
                o.vertex = UnityObjectToClipPos(v.vertex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                float horizontalOffset = 0.25; // between 0 and 1
                float horizontalFlip = -1;     // +1 or -1
                
                fixed2 uv;
                float xz = sqrt(i.objvertex.x * i.objvertex.x + i.objvertex.z * i.objvertex.z);
                float latitude = atan2(i.objvertex.y, xz);
                float longitude = atan2(i.objvertex.z, horizontalFlip * i.objvertex.x);
                uv.y = 0.5 + latitude / 3.14159;
                uv.x = horizontalOffset + longitude / (2 * 3.14159);
                
                // sample the texture
                fixed4 col;
                if (unity_StereoEyeIndex == 0)
                {
                    col = tex2D(LeftTex, uv);
                }
                else
                {
                    col = tex2D(RightTex, uv);
                }
                return col;
                
            }
            ENDCG
        }
    }
}
