Shader "Custom/ChipOutline_ObjectCenter"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _BaseOutlineColor ("Base Outline Color", Color) = (0.08,0.04,0.02,1)
        _HoverOutlineColor ("Hover Outline Color", Color) = (1,0.25,0.05,1)

        _OutlineWidth ("Outline Width", Range(0,1)) = 0.02
        _OutlinePower ("Outline Power", Range(0,5)) = 1.5

        _Hover ("Hover", Range(0,1)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Cull Front
            ZWrite On

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float _OutlineWidth;
            float _Hover;
            float _OutlinePower;

            fixed4 _BaseOutlineColor;
            fixed4 _HoverOutlineColor;

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;

                // 오브젝트 중심 (0,0,0) 기준 방향
                float3 dir = normalize(v.vertex.xyz);

                float4 expanded = v.vertex;
                expanded.xyz += dir * _OutlineWidth;

                o.pos = UnityObjectToClipPos(expanded);
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed4 col = lerp(_BaseOutlineColor, _HoverOutlineColor, _Hover);
                return col * _OutlinePower;
            }
            ENDCG
        }

        CGPROGRAM
        #pragma surface surf Standard

        sampler2D _MainTex;
        fixed4 _Color;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
        }
        ENDCG
    }
}