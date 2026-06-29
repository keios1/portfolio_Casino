Shader "Custom/Test"
{
    Properties
    {
        _Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}

        _EmissionMask ("Emission Mask", 2D) = "black" {}
        _EmissionColor ("Emission Color", Color) = (1,1,1,1)
        _EmissionPower ("Emission Power", Range(0,5)) = 1

        _FlickerSpeed ("Flicker Speed", Range(0,20)) = 4
        _FlickerStrength ("Flicker Strength", Range(0,1)) = 0.4
        _FlickerMin ("Flicker Min", Range(0,1)) = 0.6

        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Cutoff ("Alpha Cutoff", Range(0,1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="AlphaTest" }

        LOD 200
        Cull Off
        ZWrite On

        CGPROGRAM
        #pragma surface surf Standard alphatest:_Cutoff addshadow
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _EmissionMask;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_EmissionMask;
            float facing : VFACE;
        };

        half _Glossiness;
        half _Metallic;
        half _EmissionPower;
        half _FlickerSpeed;
        half _FlickerStrength;
        half _FlickerMin;

        fixed4 _Color;
        fixed4 _EmissionColor;

        UNITY_INSTANCING_BUFFER_START(Props)
        UNITY_INSTANCING_BUFFER_END(Props)

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            fixed mask = tex2D(_EmissionMask, IN.uv_EmissionMask).r;

            // 0~1로 반복되는 깜빡임 값
            half flicker = (sin(_Time.y * _FlickerSpeed) * 0.5 + 0.5);

            // 너무 완전히 꺼지지 않게 최소 밝기 보장
            flicker = lerp(_FlickerMin, 1.0, flicker);

            // 깜빡임 강도 조절
            half flickerFinal = lerp(1.0, flicker, _FlickerStrength);

            o.Albedo = c.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            o.Emission = mask * _EmissionColor.rgb * _EmissionPower * flickerFinal;

            o.Normal = IN.facing < 0 ? -o.Normal : o.Normal;
            o.Alpha = c.a;
        }
        ENDCG
    }

    FallBack "Diffuse"
}