Shader "Custom/SpriteCardHoverGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _GlowColor ("Glow Color", Color) = (1,0.25,0.1,1)
        _GlowSize ("Glow Size", Range(0,6)) = 2
        _GlowPower ("Glow Power", Range(0,5)) = 1.5
        _Hover ("Hover", Range(0,1)) = 0
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        Lighting Off
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_TexelSize;

            fixed4 _Color;
            fixed4 _GlowColor;

            float _GlowSize;
            float _GlowPower;
            float _Hover;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            v2f vert(appdata v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            float GetGlow(float2 uv)
            {
                float centerAlpha = tex2D(_MainTex, uv).a;
                float2 offset = _MainTex_TexelSize.xy * _GlowSize;

                float glow = 0;

                glow += tex2D(_MainTex, uv + float2( offset.x, 0)).a;
                glow += tex2D(_MainTex, uv + float2(-offset.x, 0)).a;
                glow += tex2D(_MainTex, uv + float2(0,  offset.y)).a;
                glow += tex2D(_MainTex, uv + float2(0, -offset.y)).a;

                glow += tex2D(_MainTex, uv + float2( offset.x,  offset.y)).a;
                glow += tex2D(_MainTex, uv + float2(-offset.x, -offset.y)).a;
                glow += tex2D(_MainTex, uv + float2( offset.x, -offset.y)).a;
                glow += tex2D(_MainTex, uv + float2(-offset.x,  offset.y)).a;

                glow = saturate(glow);

                // 내부는 제외하고 테두리 주변만 남김
                glow = saturate(glow - centerAlpha);

                return glow;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;

                float glow = GetGlow(i.uv);
                glow *= _Hover * _GlowPower;

                col.rgb += glow * _GlowColor.rgb;
                col.a = max(col.a, glow * _GlowColor.a * _Hover);

                return col;
            }
            ENDCG
        }
    }
}