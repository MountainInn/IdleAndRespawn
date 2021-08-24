Shader "Unlit/DoubleTexture"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SecondTex ("Texture", 2D) = "white" {}
        _Progress ("Progress", Range(0.0,1.0)) = 0.0
    }
    SubShader
    {
        Tags { "Queue"="Transparent" }
        ZTest Off
        Blend SrcAlpha OneMinusSrcAlpha
        LOD 100

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
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _SecondTex;
            float4 _MainTex_ST;
            float _Progress;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                UNITY_TRANSFER_FOG(o,o.vertex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col1 = tex2D(_MainTex, i.uv);
                fixed4 col2 = tex2D(_SecondTex, i.uv);

                float higher = i.uv.y > _Progress;
                float lower = i.uv.y < _Progress;

                // apply fog
                UNITY_APPLY_FOG(i.fogCoord, col);

                return col1 * higher + col2 * lower;
            }
            ENDCG
        }
    }
}
