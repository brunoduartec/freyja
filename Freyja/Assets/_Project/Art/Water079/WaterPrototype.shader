Shader "Freyja/Water Prototype"
{
    Properties
    {
        _Shallow ("Shallow", Color) = (0.34, 0.72, 0.65, 1)
        _Deep ("Deep", Color) = (0.05, 0.23, 0.31, 1)
        _DepthRange ("Depth range (m)", Range(0.1, 15)) = 5
        _WaveHeight ("Wave height (m)", Range(0, 0.2)) = 0.08
        _FoamWidth ("Contact foam width (m)", Range(0.05, 1)) = 0.35
        [HideInInspector] _PreviewTime ("Preview time", Float) = -1
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Pass
        {
            Tags { "LightMode"="ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            UNITY_DECLARE_DEPTH_TEXTURE(_CameraDepthTexture);
            sampler2D _CameraDepthNormalsTexture;
            float4 _CameraDepthTexture_TexelSize;
            fixed4 _Shallow, _Deep;
            float _DepthRange, _WaveHeight, _FoamWidth, _PreviewTime;
            float WaterTime() { return _PreviewTime >= 0 ? _PreviewTime : _Time.y; }
            float Hash(float2 p) { return frac(sin(dot(p, float2(127.1,311.7))) * 43758.5453); }
            float Noise(float2 p)
            {
                float2 cell = floor(p), f = frac(p);
                f = f*f*(3-2*f);
                return lerp(lerp(Hash(cell), Hash(cell+float2(1,0)), f.x),
                            lerp(Hash(cell+float2(0,1)), Hash(cell+1), f.x), f.y);
            }
            float NeighborContact(float2 uv, float eye, float worldY)
            {
                float separation = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv)) - eye;
                float3 viewNormal = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, uv));
                float3 normal = normalize(mul((float3x3)UNITY_MATRIX_I_V, viewNormal));
                float vertical = 1-smoothstep(.45,.8,abs(normal.y));
                float height = -separation * ((_WorldSpaceCameraPos.y-worldY)/max(eye,.001));
                return smoothstep(-1.2,-.7,separation) * (1-smoothstep(-.08,.03,separation)) * vertical * (1-smoothstep(.1,.45,height));
            }
            struct v2f { float4 pos : SV_POSITION; float4 screen : TEXCOORD0; float3 world : TEXCOORD1; float eye : TEXCOORD2; float3 normal : TEXCOORD3; };
            v2f vert(appdata_base v)
            {
                v2f o;
                float3 world = mul(unity_ObjectToWorld, v.vertex).xyz;
                float a = dot(world.xz, float2(.8,.3)) - WaterTime()*.8;
                float b = dot(world.xz, float2(-.35,.65)) - WaterTime()*.55;
                world.y += _WaveHeight * (.7*sin(a)+.3*sin(b));
                float2 slope = _WaveHeight * (.7*cos(a)*float2(.8,.3)+.3*cos(b)*float2(-.35,.65));
                o.normal = normalize(float3(-slope.x,1,-slope.y));
                o.world = world;
                o.pos = UnityWorldToClipPos(world);
                o.screen = ComputeScreenPos(o.pos);
                o.eye = -mul(UNITY_MATRIX_V, float4(world,1)).z;
                return o;
            }
            fixed4 frag(v2f i) : SV_Target
            {
                float raw = SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.screen));
                float separation = LinearEyeDepth(raw) - i.eye;
                float thickness = max(0, separation);
                float depth = saturate(thickness / _DepthRange);
                float3 light = UNITY_LIGHTMODEL_AMBIENT.rgb + _LightColor0.rgb * saturate(dot(normalize(i.normal), _WorldSpaceLightPos0.xyz));
                float2 p = i.world.xz;
                float noiseA = Noise(p*.38 + float2(.12,.07)*WaterTime());
                float noiseB = Noise(p*.67 + float2(-.08,.11)*WaterTime());
                float phaseA = dot(p, float2(.85,.36)) - WaterTime()*.8 + (noiseA-.5)*1.8;
                float phaseB = dot(p, float2(.36,.72)) - WaterTime()*.55 + (noiseB-.5)*1.6;
                float ripple = .65*sin(phaseA) + .35*sin(phaseB);
                float3 color = lerp(_Shallow.rgb, _Deep.rgb, depth) * (light*.7 + .25) * (1 + ripple * .20);
                float noise = noiseA*2-1;
                float2 uv = i.screen.xy / i.screen.w;
                float3 sceneViewNormal = DecodeViewNormalStereo(tex2D(_CameraDepthNormalsTexture, uv));
                float3 sceneWorldNormal = normalize(mul((float3x3)UNITY_MATRIX_I_V, sceneViewNormal));
                // Normal-dependent foam width follows Roystan's Toon Water technique (Unlicense).
                float alignment = saturate(dot(sceneWorldNormal, normalize(i.normal)));
                float width = lerp(_FoamWidth, _FoamWidth*.08, alignment) * (1 + .25*noise);
                float foamVariation = lerp(.75, .9, noiseB);
                float contactOrientation = 1-smoothstep(.75,.95,alignment);
                float direct = (1-smoothstep(width*.1,width,thickness)) * contactOrientation;
                float2 d = _CameraDepthTexture_TexelSize.xy * 2;
                float edge = max(max(NeighborContact(uv+float2(d.x,0),i.eye,i.world.y), NeighborContact(uv-float2(d.x,0),i.eye,i.world.y)),
                                 max(NeighborContact(uv+float2(0,d.y),i.eye,i.world.y), NeighborContact(uv-float2(0,d.y),i.eye,i.world.y)));
                float foam = max(direct,edge) * step(0,separation) * foamVariation;
                color = lerp(color, float3(.9,.95,.88), foam*.8);
                float glint = smoothstep(.25, .9, ripple) * .12;
                float alpha = saturate(lerp(.22,.97,smoothstep(0,1,depth)) + glint);
                // Blend into the analytic wash of the showcase beach before the water mesh meets it.
                float shore = i.world.z - (3 + 2*sin(i.world.x*.19));
                float oceanFade = 1-smoothstep(-1.1,0,shore);
                return fixed4(color, max(alpha * oceanFade, foam * .8));
            }
            ENDCG
        }
    }
}
