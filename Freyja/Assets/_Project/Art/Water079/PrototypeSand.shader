Shader "Freyja/Water Prototype Sand"
{
    Properties
    {
        _Color ("Sand", Color) = (0.64, 0.53, 0.37, 1)
        _CausticStrength ("Caustics", Range(0, 1)) = 0.35
        _ShoreReach ("Shore reach (m)", Range(0, 2)) = 1.1
        [HideInInspector] _PreviewTime ("Preview time", Float) = -1
        [HideInInspector] _PreviewDepth ("Preview caustic depth", Float) = -1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        CGPROGRAM
        #pragma surface surf Lambert addshadow
        #include "UnityCG.cginc"
        #include "Lighting.cginc"
        fixed4 _Color;
        float _CausticStrength, _ShoreReach, _PreviewTime, _PreviewDepth;
        struct Input { float3 worldPos; };
        float Hash(float2 p) { return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453); }
        float Noise(float2 p)
        {
            float2 cell = floor(p), f = frac(p);
            f = f*f*(3-2*f);
            return lerp(lerp(Hash(cell), Hash(cell+float2(1,0)),f.x),
                        lerp(Hash(cell+float2(0,1)), Hash(cell+1),f.x),f.y);
        }
        void surf(Input IN, inout SurfaceOutput o)
        {
            float2 p = IN.worldPos.xz * 1.2;
            float2 cell = floor(p), f = frac(p);
            f = f * f * (3 - 2 * f);
            float grain = lerp(lerp(Hash(cell), Hash(cell + float2(1, 0)), f.x),
                               lerp(Hash(cell + float2(0, 1)), Hash(cell + 1), f.x), f.y);
            float variation = lerp(.84, 1.16, grain);
            float time = _PreviewTime >= 0 ? _PreviewTime : _Time.y;
            // Matches only the analytic beach used by WaterShowcase.
            float shore = IN.worldPos.z - (3 + 2*sin(IN.worldPos.x*.19));
            float phase = time*.9 + .2*sin(IN.worldPos.x*.25);
            float reach = _ShoreReach * (.12 + .88*(.5+.5*sin(phase)));
            float drySide = smoothstep(-.35,-.08,shore);
            float edgeNoise = Noise(float2(IN.worldPos.x*.9,time*.12));
            float earlierReach = _ShoreReach * (.12 + .88*(.5+.5*sin(phase-.8)));
            float wetReach = max(reach, earlierReach);
            float wet = drySide * (1-smoothstep(wetReach,wetReach+.35,shore));
            float crestCenter = reach + (edgeNoise-.5)*.42;
            float crest = drySide * (1-smoothstep(.04,.30,abs(shore-crestCenter))) * smoothstep(.22,.58,edgeNoise);
            float3 sand = _Color.rgb * variation * (1 - wet*.30);
            float wash = smoothstep(-1.4,-.3,shore) * (1-smoothstep(reach-.25,reach+.12,shore));
            float3 wetWater = sand * float3(.65,.97,.96) + float3(.025,.10,.10);
            sand = lerp(sand, wetWater, wash*.65);
            o.Albedo = lerp(sand, float3(.94,.95,.86), crest*.88);

            float2 q = IN.worldPos.xz;
            float2 bend = float2(Noise(q*.3+time*.03),Noise(q*.3+float2(4.2,-time*.04)))-.5;
            float ca = Noise(q*1.2 + bend + float2(time*.10,time*.07));
            float cb = Noise(q*1.2 - bend + float2(3.7-time*.08,1.9+time*.09));
            float breakup = smoothstep(.25,.75,Noise(q*.46 + float2(9.1,time*.04)));
            float pattern = (1-smoothstep(.012,.07,abs(ca-cb))) * breakup * .15;
            float submerged = 1-smoothstep(-.08,.02,IN.worldPos.y);
            float causticDepth = _PreviewDepth >= 0 ? _PreviewDepth : -IN.worldPos.y;
            float depthFade = 1-smoothstep(.25,2.5,causticDepth);
            float sunlight = saturate(_WorldSpaceLightPos0.y*3) * saturate(max(_LightColor0.r,max(_LightColor0.g,_LightColor0.b)));
            o.Emission = float3(.7,.9,.75) * pattern * submerged * depthFade * sunlight * _CausticStrength;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
