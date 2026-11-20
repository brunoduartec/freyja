Shader "Freyja/ExperimentoRampa"
{
    Properties
    {
        _Color ("Tint", Color) = (1,1,1,1)
        _MainTex ("Albedo", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200
        CGPROGRAM
        #pragma surface surf Ramp fullforwardshadows addshadow
        sampler2D _MainTex;
        fixed4 _Color;
        struct Input { float2 uv_MainTex; };
        half4 LightingRamp(SurfaceOutput s, half3 lightDir, half atten)
        {
            half ndl = saturate(dot(s.Normal, lightDir));
            half band = smoothstep(0.12, 0.32, ndl) * 0.45 + smoothstep(0.48, 0.75, ndl) * 0.55;
            half3 tint = lerp(half3(0.64,0.59,0.82), half3(1.0,0.94,0.80), band);
            half4 c;
            c.rgb = s.Albedo * _LightColor0.rgb * tint * (0.20 + 0.80 * band) * atten;
            c.a = s.Alpha;
            return c;
        }
        void surf(Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }
    Fallback "Diffuse"
}
