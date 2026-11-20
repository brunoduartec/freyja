Shader "Freyja/PS1 Path Edge" {
 Properties { _MainTex("Texture",2D)="white"{} _Color("Color",Color)=(1,1,1,1) _Extent("Extent",Vector)=(1,1,0,0) _EdgeWidth("Edge width",Float)=.6 }
 SubShader {
 Tags {"RenderType"="TransparentCutout" "Queue"="AlphaTest"} Cull Off
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows addshadow
 #pragma target 3.0
 sampler2D _MainTex;fixed4 _Color;float4 _Extent;float _EdgeWidth;
 struct Input {float2 uv_MainTex;float2 uv2_MainTex;float3 worldPos;};
 void surf(Input IN,inout SurfaceOutputStandard o){
 float2 q=IN.uv2_MainTex;
 float distanceToEdge=min(min(q.x,_Extent.x-q.x),min(q.y,_Extent.y-q.y));
 float coverage=saturate(distanceToEdge/max(.01,_EdgeWidth));
 float2 cell=floor(IN.worldPos.xz*16);
 float noise=frac(sin(dot(cell,float2(12.9898,78.233)))*43758.5453);
 clip(coverage-noise-.001);
 fixed4 c=tex2D(_MainTex,IN.uv_MainTex)*_Color;o.Albedo=c.rgb;o.Metallic=0;o.Smoothness=0;o.Alpha=1;
 }
 ENDCG
 } FallBack "Diffuse"
}
