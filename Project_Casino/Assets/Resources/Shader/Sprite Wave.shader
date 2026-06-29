Shader "Custom/UI_MaskedStrongWiggle"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _WiggleMask ("Wiggle Mask", 2D) = "black" {}

        _WiggleStrength ("Wiggle Strength", Range(0, 1.0)) = 0.01
        _WiggleSpeed ("Wiggle Speed", Range(0, 20)) = 4
        _WiggleFrequency ("Wiggle Frequency", Range(0, 50)) = 12

        _StencilComp ("Stencil Comparison", Float) = 8
        _Stencil ("Stencil ID", Float) = 0
        _StencilOp ("Stencil Operation", Float) = 0
        _StencilWriteMask ("Stencil Write Mask", Float) = 255
        _StencilReadMask ("Stencil Read Mask", Float) = 255
        _ColorMask ("Color Mask", Float) = 15
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "IgnoreProjector"="True"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
            "CanUseSpriteAtlas"="True"
        }

        Stencil
        {
            Ref [_Stencil]
            Comp [_StencilComp]
            Pass [_StencilOp]
            ReadMask [_StencilReadMask]
            WriteMask [_StencilWriteMask]
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest [unity_GUIZTestMode]
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask [_ColorMask]

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata_t
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            sampler2D _WiggleMask;
            fixed4 _Color;

            float _WiggleStrength;
            float _WiggleSpeed;
            float _WiggleFrequency;

            v2f vert(appdata_t IN)
            {
                v2f OUT;
                OUT.vertex = UnityObjectToClipPos(IN.vertex);
                OUT.uv = IN.texcoord;
                OUT.color = IN.color * _Color;
                return OUT;
            }

            fixed4 frag(v2f IN) : SV_Target
            {
                float2 uv = IN.uv;

                // 마스크 (0~1)
                float mask = tex2D(_WiggleMask, uv).r;

                // ===== 1차 웨이브 =====
                float wave1 = sin((uv.y * _WiggleFrequency) + (_Time.y * _WiggleSpeed));
                float wave2 = cos((uv.x * _WiggleFrequency) + (_Time.y * _WiggleSpeed * 0.8));

                // ===== 2차 큰 요동 =====
                float wave3 = sin((_Time.y * _WiggleSpeed * 0.5) + uv.x * 4);
                float wave4 = cos((_Time.y * _WiggleSpeed * 0.6) + uv.y * 3);

                // ===== 합성 =====
                float2 distortion = float2(
                    wave1 + wave3 * 0.7,
                    wave2 + wave4 * 0.5
                );

                // ===== 강도 적용 =====
                distortion *= _WiggleStrength * mask * 1.5;

                fixed4 col = tex2D(_MainTex, uv + distortion) * IN.color;

                return col;
            }
            ENDCG
        }
    }
}