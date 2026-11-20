Shader "Hidden/Freyja/ColliderWireframe"
{
    Properties
    {
        _Color ("Color", Color) = (0, 1, 1, 0.65)
        _DepthTest ("Depth test", Float) = 8
    }
    SubShader
    {
        Pass
        {
            Cull Off
            ZWrite Off
            ZTest [_DepthTest]
            Blend SrcAlpha OneMinusSrcAlpha
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #include "UnityCG.cginc"
            fixed4 _Color;

            float4 Vert(float4 position : POSITION) : SV_POSITION
            {
                return UnityObjectToClipPos(position);
            }

            fixed4 Frag() : SV_Target
            {
                return _Color;
            }
            ENDCG
        }
    }
}
