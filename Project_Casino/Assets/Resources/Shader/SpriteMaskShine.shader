Shader "Custom/SpriteMaskSparkleBlinkShadow"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _SparkleMask ("Sparkle Mask", 2D) = "black" {}
        _ShadowMask ("Shadow Mask", 2D) = "black" {}

        [HDR] _SparkleColor ("Sparkle Color", Color) = (1,0.9,0.45,1)
        _SparklePower ("Sparkle Power", Range(0,8)) = 2
        _SparkleSpeed ("Sparkle Speed", Range(0,10)) = 3

        _ShadowStrength ("Shadow Strength", Range(0,1)) = 0.5
        _ShadowBlinkMin ("Shadow Blink Min", Range(0,1)) = 0.2
        _ShadowBlinkMax ("Shadow Blink Max", Range(0,1)) = 1.0
        _ShadowBlinkSpeed ("Shadow Blink Speed", Range(0,10)) = 3

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
            sampler2D _SparkleMask;
            sampler2D _ShadowMask;

            fixed4 _Color;
            fixed4 _SparkleColor;

            float _SparklePower;
            float _SparkleSpeed;

            float _ShadowStrength;
            float _ShadowBlinkMin;
            float _ShadowBlinkMax;
            float _ShadowBlinkSpeed;

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
                fixed4 col = tex2D(_MainTex, IN.uv) * IN.color;
                float alpha = col.a;

                float sparkleMask = tex2D(_SparkleMask, IN.uv).r;
                float shadowMask = tex2D(_ShadowMask, IN.uv).r;

                float sparkleBlink = sin(_Time.y * _SparkleSpeed) * 0.5 + 0.5;
                sparkleBlink = pow(sparkleBlink, 4.0);

                float shadowBlink = sin(_Time.y * _ShadowBlinkSpeed) * 0.5 + 0.5;
                shadowBlink = pow(shadowBlink, 4.0);
                shadowBlink = lerp(_ShadowBlinkMin, _ShadowBlinkMax, shadowBlink);

                float sparkle = sparkleMask * sparkleBlink * _SparklePower * alpha;
                float shadow = shadowMask * _ShadowStrength * shadowBlink * alpha;

                col.rgb *= (1.0 - shadow);
                col.rgb += _SparkleColor.rgb * sparkle;

                return col;
            }
            ENDCG
        }
    }
}