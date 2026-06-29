Shader "Custom/CardMaskBlinkGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Card Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _GlowMask ("Glow Mask", 2D) = "black" {}
        [HDR] _GlowColor ("Glow Color", Color) = (0.2, 1, 0.2, 1)
        _GlowPower ("Glow Power", Range(0, 8)) = 2

        _BlinkSpeed ("Blink Speed", Range(0, 10)) = 2
        _BlinkMin ("Blink Min", Range(0, 1)) = 0.2
        _BlinkMax ("Blink Max", Range(0, 2)) = 1
        
        // 🌟 C# 스크립트에서 조종할 켜짐/꺼짐 스위치 추가!
        _Hover ("Hover Intensity", Range(0, 1)) = 0

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
            sampler2D _GlowMask;

            fixed4 _Color;
            fixed4 _GlowColor;
            float _GlowPower;
            float _BlinkSpeed;
            float _BlinkMin;
            float _BlinkMax;
            float _Hover; // 스위치 변수 등록

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
                fixed4 card = tex2D(_MainTex, IN.uv) * IN.color;
                float mask = tex2D(_GlowMask, IN.uv).r;

                float blink01 = sin(_Time.y * _BlinkSpeed) * 0.5 + 0.5;
                float blink = lerp(_BlinkMin, _BlinkMax, blink01);

                // 🌟 최종 빛나는 수치(glow)에 _Hover를 곱해줍니다! 
                // _Hover가 0(사용 불가)이면 빛도 0이 되어 완전히 꺼집니다.
                float glow = mask * _GlowPower * blink * _Hover;

                fixed4 finalColor = card;
                finalColor.rgb += _GlowColor.rgb * glow;
                
                // 🌟 알파값(투명도)에도 _Hover를 곱해서 빛의 잔여물까지 완벽하게 지워줍니다.
                finalColor.a = max(card.a, mask * blink * _GlowColor.a * _Hover);

                return finalColor;
            }
            ENDCG
        }
    }
}