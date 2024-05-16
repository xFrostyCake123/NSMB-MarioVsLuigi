Shader "Custom/HeatwaveV2"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _Distortion ("Distortion", Range(0,0.1)) = 0.01
        _Speed ("Speed", Range(0,10)) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert

        sampler2D _MainTex;
        float _Distortion;
        float _Speed;

        struct Input
        {
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutput o)
        {
            float distortion = sin(_Time.y * _Speed + IN.uv_MainTex.x * 10) * _Distortion;
            float2 uv = IN.uv_MainTex + float2(distortion, 0);
            o.Albedo = tex2D(_MainTex, uv).rgb;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
