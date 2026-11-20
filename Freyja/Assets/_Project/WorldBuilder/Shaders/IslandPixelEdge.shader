Shader "Freyja/Island Pixel Edge" {
Properties { _MainTex("Texture",2D)="white"{} _Color("Color",Color)=(1,1,1,1) }
SubShader { Tags { "Queue"="AlphaTest" "RenderType"="TransparentCutout" } Cull Off
CGPROGRAM
#pragma surface surf Standard fullforwardshadows addshadow
#pragma target 3.0
sampler2D _MainTex; fixed4 _Color;
struct Input {float2 uv_MainTex;float3 worldPos;float4 color:COLOR;};
void surf(Input IN,inout SurfaceOutputStandard o){float2 cell=floor(IN.worldPos.xz*12);float n=frac(sin(dot(cell,float2(12.9898,78.233)))*43758.5453);clip(IN.color.a-n-.001);o.Albedo=tex2D(_MainTex,IN.uv_MainTex).rgb*_Color.rgb;o.Alpha=1;o.Smoothness=0;}
ENDCG
} FallBack "Diffuse"
}
