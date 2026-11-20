Shader "Freyja/Sheltered Sea"
{
    Properties
    {
        _WaterColor ("Water", Color) = (0.17, 0.48, 0.52, 1)
        _Coast ("Shore and sand distances", 2D) = "white" {}
        [NoScaleOffset] _PierOcclusion ("Pier and moored hull occlusion", 2D) = "black" {}
        _WaveHeight ("Maximum wave height (m)", Range(0, 0.15)) = 0.065
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase
            #include "UnityCG.cginc"
            #include "AutoLight.cginc"
            #include "Lighting.cginc"
            sampler2D _Coast, _PierOcclusion;
            fixed4 _WaterColor;
            float _WaveHeight;

            float2 phases(float2 p)
            {
                return float2(dot(p, float2(.43, .20)), dot(p, float2(-.19, .36)))
                     - _Time.y * float2(1.04, .76);
            }

            struct Surface
            {
                float4 pos : SV_POSITION;
                float2 meters : TEXCOORD0;
                float depth : TEXCOORD1;
                SHADOW_COORDS(2)
            };

            Surface vert(appdata_base v)
            {
                Surface o;
                float3 world = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.meters = world.xz;
                o.depth = -mul(UNITY_MATRIX_V, float4(world,1)).z;
                world.y += _WaveHeight * dot(sin(phases(world.xz)), float2(.72,.28));
                o.pos = UnityWorldToClipPos(world);
                TRANSFER_SHADOW(o);
                return o;
            }

            fixed4 frag(Surface i) : SV_Target
            {
                float2 phase = phases(i.meters);
                float slope = dot(cos(phase), float2(.72,.28));
                float nearRipples = 1 - smoothstep(10, 48, i.depth);
                float farWaves = 1 - smoothstep(35, 120, i.depth);
                float ripple = sin(dot(i.meters, float2(1.83,.86)) - _Time.y * 1.52
                                 + .9 * sin(phase.y) + .45 * sin(phase.x));
                float patches = smoothstep(-.3,.8,sin(phase.y + sin(phase.x)) * cos(phase.x*.7));
                float glint = smoothstep(.55,1,ripple) * patches;
                float3 color = _WaterColor.rgb * (1 + .10*slope*farWaves);
                color += float3(.023,.037,.036) * glint * nearRipples;
                float2 shore = tex2D(_Coast, (i.meters + 104.0) / 208.0).rg * 24.0;
                float shallows = 1 - smoothstep(.5, 12.0, shore.x);
                color = lerp(color, color * float3(1.05, 1.18, 1.08), shallows);
                // Surveyed 32 m square around the two fixed berths.
                float2 pierUV = (i.meters - float2(-46, -70)) / 32;
                float shelter = tex2D(_PierOcclusion, saturate(pierUV)).r;
                color *= 1 - shelter * float3(.40, .34, .28);
                float pulse = sin(_Time.y * 1.32 + dot(i.meters, float2(.47,.66)));
                float contact = 1 - smoothstep(.14, .60 + .13 * pulse, shore.x);
                float age = frac(_Time.y * .21 + .12*sin(dot(i.meters,float2(.07,.043))));
                float crest = lerp(2.8,.08,smoothstep(0,.81,age));
                float wash = (1-smoothstep(.10,.52,abs(shore.y-crest))) *
                             smoothstep(.07,.25,age)*(1-smoothstep(.71,.98,age));
                float foam = max(contact*.55,wash*.66)*smoothstep(-.45,.55,sin(phase.y)+.55*ripple);
                color = lerp(color,float3(.66,.84,.80),foam*(1-smoothstep(30,90,i.depth)));
                color *= saturate(.32*unity_AmbientSky.rgb + .85*_LightColor0.rgb + .09);
                color *= lerp(.64, 1, SHADOW_ATTENUATION(i));
                return fixed4(color,1);
            }
            ENDCG
        }
    }
}
