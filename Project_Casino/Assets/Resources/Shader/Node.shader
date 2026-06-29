Shader "Custom/Node"
{
    Properties
    {
        _MainTex ("Base Texture", 2D) = "white" {}
        _Color ("Base Color", Color) = (1,1,1,1)

        _EmissionMask ("Emission Mask", 2D) = "black" {}
        _EmissionColor ("Emission Color", Color) = (1,0.85,0.45,1)
        _EmissionPower ("Emission Power", Range(0,5)) = 1.2

        _PulseSpeed ("Pulse Speed", Range(0,10)) = 2.0
        _PulseMin ("Pulse Min", Range(0,5)) = 0.8
        _PulseMax ("Pulse Max", Range(0,5)) = 1.4

        _Glossiness ("Smoothness", Range(0,1)) = 0.3
        _Metallic ("Metallic", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        Cull Back

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _EmissionMask;

        fixed4 _Color;
        fixed4 _EmissionColor;
        half _EmissionPower;
        half _PulseSpeed;
        half _PulseMin;
        half _PulseMax;

        half _Glossiness;
        half _Metallic;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_EmissionMask;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            fixed4 baseTex = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            fixed mask = tex2D(_EmissionMask, IN.uv_EmissionMask).r;

            // 기본 표면
            o.Albedo = baseTex.rgb;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness;

            // 천천히 숨쉬듯 반짝이는 값
            float pulse = lerp(_PulseMin, _PulseMax, (sin(_Time.y * _PulseSpeed) * 0.5 + 0.5));

            // 마스크 부분만 발광
            o.Emission = mask * _EmissionColor.rgb * _EmissionPower * pulse;

            o.Alpha = baseTex.a;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
