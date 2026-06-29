Shader "Custom/DiceSlotBlinkGlow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        [HDR] _GlowColor ("Glow Color", Color) = (0.1, 0.5, 1.0, 1) // 빛나는 색상
        _GlowPower ("Glow Power", Range(0, 8)) = 3

        _BlinkSpeed ("Blink Speed", Range(0, 10)) = 4
        _BlinkMin ("Blink Min", Range(0, 1)) = 0.2
        _BlinkMax ("Blink Max", Range(0, 2)) = 1
        
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

            fixed4 _Color;
            fixed4 _GlowColor;
            float _GlowPower;
            float _BlinkSpeed;
            float _BlinkMin;
            float _BlinkMax;
            float _Hover; 

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
                // 🌟 핵심: Source Image에 넣은 테두리 이미지를 그대로 가져옵니다.
                fixed4 boxColor = tex2D(_MainTex, IN.uv) * IN.color;

                // 🌟 테두리 모양(Alpha 투명도)과 밝기(Red)를 계산해서 마스크를 자동 생성!
                // 이렇게 하면 가운데 투명한 곳이나 까만 곳은 빛나지 않고 선만 예쁘게 빛납니다.
                float mask = boxColor.a * boxColor.r;

                float blink01 = sin(_Time.y * _BlinkSpeed) * 0.5 + 0.5;
                float blink = lerp(_BlinkMin, _BlinkMax, blink01);

                // 마스크 모양대로만 빛을 쏩니다.
                float glow = mask * _GlowPower * blink * _Hover;

                fixed4 finalColor = boxColor;
                finalColor.rgb += _GlowColor.rgb * glow;
                
                // 알파값(투명도)도 테두리 모양을 벗어나지 않게 잡아줍니다.
                finalColor.a = max(boxColor.a, mask * blink * _GlowColor.a * _Hover);

                return finalColor;
            }
            ENDCG
        }
    }
}