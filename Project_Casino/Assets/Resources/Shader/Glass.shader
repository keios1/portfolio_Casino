Shader "Custom/Glass"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Tint", Color) = (1,1,1,1)

        _RimColor ("Rim Color", Color) = (1,1,1,1)
        _RimPower ("Rim Power", Range(0.5,8)) = 3
        _RimIntensity ("Rim Intensity", Range(0,3)) = 1.2

        _GlassAlpha ("Glass Transparency", Range(0,1)) = 0.55
        _LineAlpha ("Line Opacity", Range(0,1)) = 1.0
        _BlackThreshold ("Black Threshold", Range(0,1)) = 0.2
        _LineSoftness ("Line Softness", Range(0.001,0.5)) = 0.08

        _Glossiness ("Smoothness", Range(0,1)) = 0.8
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        CGPROGRAM
        #pragma surface surf Standard alpha:fade fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _Color;

        fixed4 _RimColor;
        half _RimPower;
        half _RimIntensity;

        half _GlassAlpha;
        half _LineAlpha;
        half _BlackThreshold;
        half _LineSoftness;

        half _Glossiness;
        half _Metallic;

        struct Input
        {
            float2 uv_MainTex;
            float3 viewDir;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 tex = tex2D(_MainTex, IN.uv_MainTex) * _Color;

            o.Albedo = tex.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            // 텍스처 밝기 계산
            float luminance = dot(tex.rgb, float3(0.299, 0.587, 0.114));

            // 검은 부분 마스크
            // 검을수록 lineMask = 1, 밝을수록 0
            float lineMask = 1.0 - smoothstep(_BlackThreshold, _BlackThreshold + _LineSoftness, luminance);

            // Rim Light (라인에는 너무 세게 안 들어가게)
            float rim = 1.0 - saturate(dot(normalize(IN.viewDir), o.Normal));
            rim = pow(rim, _RimPower);

            float glassMask = 1.0 - lineMask;
            o.Emission = _RimColor.rgb * rim * _RimIntensity * glassMask;

            // 핵심: 검은 선은 불투명, 유리 부분은 투명
            o.Alpha = lerp(_GlassAlpha, _LineAlpha, lineMask);
        }
        ENDCG
    }

    FallBack "Transparent/Diffuse"
}
