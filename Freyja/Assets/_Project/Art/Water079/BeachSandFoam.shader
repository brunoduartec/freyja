Shader "Freyja/Beach Sand Foam 079"
{
    Properties
    {
        _FoamColor ("Foam color", Color) = (0.9, 0.97, 0.9, 0.9)
        _WaterLevel ("Water level", Float) = 0
        _Runup ("Shore runup (m)", Range(0.2, 4)) = 2.8
        _BandWidth ("Foam width (m)", Range(0.05, 1.5)) = 0.65
        _Period ("Wave period (s)", Range(2, 12)) = 4.76
        _Opacity ("Opacity", Range(0, 1)) = 0.55
        [HideInInspector] _PreviewTime ("Preview time", Float) = -1
    }
    SubShader
    {
        Tags { "Queue"="Transparent+1" "RenderType"="Transparent" "IgnoreProjector"="True" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        Cull Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            fixed4 _FoamColor;
            float _WaterLevel, _Runup, _BandWidth, _Period, _Opacity, _PreviewTime;

            struct appdata
            {
                float4 vertex : POSITION;
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float4 position : SV_POSITION;
                float3 world : TEXCOORD0;
                fixed mask : TEXCOORD1;
            };

            float WaterTime()
            {
                return _PreviewTime >= 0 ? _PreviewTime : _Time.y;
            }

            v2f vert(appdata v)
            {
                v2f o;
                float4 world = mul(unity_ObjectToWorld, v.vertex);
                o.position = UnityWorldToClipPos(world.xyz);
                o.world = world.xyz;
                o.mask = v.color.a;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float time = WaterTime();
                float spatialOffset = .12 * sin(dot(i.world.xz, float2(.07, .043)));
                float phase = frac(time / max(_Period, 0.01) + spatialOffset);
                float travel = phase < .5 ? phase * 2 : (1 - phase) * 2;
                float crest = lerp(.08, _Runup, travel);
                float breakup = .13 * sin(dot(i.world.xz, float2(1.7, 2.3)) + time * .23);
                float distance = abs((i.world.y - _WaterLevel) - crest + breakup);
                float band = 1 - smoothstep(.04, max(_BandWidth, .05), distance);
                float active = smoothstep(.025, .13, phase) * (1 - smoothstep(.87, .98, phase));
                float alpha = band * active * i.mask * _Opacity * _FoamColor.a;
                return fixed4(_FoamColor.rgb, saturate(alpha));
            }
            ENDCG
        }
    }
}
