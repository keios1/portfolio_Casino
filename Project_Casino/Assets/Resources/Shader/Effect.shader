Shader "Custom/Particle_EffectSprite_MaskedSparkle"
{
    Properties
    {
        _MainTex ("Main Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _DistortionMask ("Distortion Mask", 2D) = "black" {}
        _DissolveMask ("Dissolve Mask", 2D) = "white" {}
        _SparkleMask ("Sparkle Mask", 2D) = "black" {}

        _DistortionStrength ("Distortion Strength", Range(0,0.05)) = 0.005
        _DistortionSpeed ("Distortion Speed", Range(0,20)) = 4
        _DistortionFrequency ("Distortion Frequency", Range(0,50)) = 16

        _DissolveAmount ("Dissolve Amount", Range(0,1)) = 0
        _DissolveSoftness ("Dissolve Softness", Range(0.001,0.2)) = 0.04

        [HDR] _EdgeColor ("Burn Edge Color", Color) = (1,0.45,0.1,1)
        _EdgePower ("Edge Power", Range(0,8)) = 2
        _EdgeWidth ("Edge Width", Range(0.001,0.2)) = 0.05

        [HDR] _SparkleColor ("Sparkle Color", Color) = (1,0.9,0.45,1)
        _SparklePower ("Sparkle Power", Range(0,8)) = 1.5
        _SparkleSpeed ("Sparkle Speed", Range(0,20)) = 6
        _SparkleSharpness ("Sparkle Sharpness", Range(1,20)) = 8
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_particles
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            sampler2D _DistortionMask;
            sampler2D _DissolveMask;
            sampler2D _SparkleMask;

            fixed4 _Color;
            fixed4 _EdgeColor;
            fixed4 _SparkleColor;

            float _DistortionStrength;
            float _DistortionSpeed;
            float _DistortionFrequency;

            float _DissolveAmount;
            float _DissolveSoftness;
            float _EdgePower;
            float _EdgeWidth;

            float _SparklePower;
            float _SparkleSpeed;
            float _SparkleSharpness;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
                float2 uv : TEXCOORD0;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.color = v.color * _Color;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = i.uv;

                // 마스크 기반 일렁임
                float dMask = tex2D(_DistortionMask, uv).r;

                float waveX = sin(uv.y * _DistortionFrequency + _Time.y * _DistortionSpeed);
                float waveY = cos(uv.x * _DistortionFrequency + _Time.y * _DistortionSpeed * 0.8);

                float2 distortion = float2(waveX, waveY) * _DistortionStrength * dMask;
                float2 finalUV = uv + distortion;

                fixed4 col = tex2D(_MainTex, finalUV) * i.color;

                // 디졸브 / 타들어감
                float dissolve = tex2D(_DissolveMask, finalUV).r;

                float visible = smoothstep(
                    _DissolveAmount,
                    _DissolveAmount + _DissolveSoftness,
                    dissolve
                );

                float edgeOuter = smoothstep(
                    _DissolveAmount,
                    _DissolveAmount + _EdgeWidth,
                    dissolve
                );

                float edge = saturate(edgeOuter - visible);

                col.rgb += _EdgeColor.rgb * edge * _EdgePower;
                col.a *= visible;

                // 마스크 기반 반짝임 - 전체가 동시에 깜박임
float sparkleMask = tex2D(_SparkleMask, uv).r;

// 시간만 사용해서 전체가 같이 깜박임
float blink = sin(_Time.y * _SparkleSpeed) * 0.5 + 0.5;

// 반짝임 곡선 조절
blink = pow(blink, _SparkleSharpness);

// 최종 반짝임
float sparkle = sparkleMask * blink * _SparklePower;

col.rgb += _SparkleColor.rgb * sparkle;

                return col;
            }
            ENDCG
        }
    }
}