Shader "Freyja/Lit Coastal Grass"
{
    Properties
    {
        _RootColor ("Root", Color) = (0.25, 0.31, 0.13, 1)
        _TipColor ("Tip", Color) = (0.42, 0.48, 0.22, 1)
        _WindBend ("Wind Bend (m)", Range(0, 0.12)) = 0.075
        _NormalUp ("Normal Softness", Range(0, 0.8)) = 0.38
        _Translucency ("Translucency", Range(0, 1)) = 0.38
        [HideInInspector] _FadeDistance ("Fade Range", Vector) = (28, 35, 0, 0)
        [HideInInspector] _GrassGeometry ("Width and Height", Vector) = (1, 1, 0, 0)
        [HideInInspector] _GrassLod ("LOD Range and Role", Vector) = (0, 1, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        Cull Off

        CGINCLUDE
        #include "UnityCG.cginc"

        float _WindBend;
        float4 _FadeDistance;
        float4 _GrassGeometry;
        float4 _GrassLod;
        #include "WindCore.hlsl"

        struct Attributes
        {
            float4 vertex : POSITION;
            float3 normal : NORMAL;
            UNITY_VERTEX_INPUT_INSTANCE_ID
        };

        void Deform(Attributes input, out float3 world, out float3 normal, out float height, out float variation)
        {
            const float bladeHeight = 0.31;
            float3 root = unity_ObjectToWorld._m03_m13_m23;
            variation = frac(sin(dot(root.xz, float2(47.21, 13.57))) * 43758.5453);
            float transition = lerp(_FadeDistance.x, _FadeDistance.y - 1.5, variation);
            float distanceToCamera = distance(root, _WorldSpaceCameraPos);
            float size = 1 - smoothstep(transition, transition + 1.5, distanceToCamera);
            float lod = smoothstep(_GrassLod.x, max(_GrassLod.y, _GrassLod.x + 0.001), distanceToCamera);
            size *= 1 - max(_GrassLod.z, 0) * (1 - lod) + min(_GrassLod.z, 0) * lod;
            float3 scale = max(_GrassGeometry.xyx, 0.01);
            height = saturate(input.vertex.y / bladeHeight);

            float2 wind = FreyjaWindOffset(root.xz, variation, min(_WindBend, 0.12));
            float3 tipOffset = float3(wind.x, 0, wind.y);
            world = root + size * (mul((float3x3)unity_ObjectToWorld, input.vertex.xyz * scale)
                + tipOffset * height * height);

            float3 restNormal = mul(input.normal / scale, (float3x3)unity_WorldToObject);
            // Transform the normal through the local shear created by the quadratic bend.
            float heightSlope = 2 * height * step(input.vertex.y, bladeHeight) / bladeHeight;
            float3 heightGradient = unity_WorldToObject._m10_m11_m12 * heightSlope / scale.y;
            normal = normalize(restNormal * (1 + dot(heightGradient, tipOffset))
                - heightGradient * dot(restNormal, tipOffset));
        }
        ENDCG

        Pass
        {
            Name "FORWARD"
            Tags { "LightMode" = "ForwardBase" }

            CGPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
            #pragma multi_compile_fog
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            half4 _RootColor;
            half4 _TipColor;
            half _Translucency;
            half _NormalUp;
            float4 _FreyjaGrassLightPositionRange[4];
            half4 _FreyjaGrassLightColor[4];
            int _FreyjaGrassLightCount;
            half4 _FreyjaGrassAmbientSky;
            half4 _FreyjaGrassAmbientEquator;
            half4 _FreyjaGrassAmbientGround;

            struct Varyings
            {
                float4 pos : SV_POSITION;
                float3 world : TEXCOORD0;
                half3 normal : TEXCOORD1;
                half3 albedo : COLOR0;
                UNITY_FOG_COORDS(2)
                SHADOW_COORDS(3)
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings Vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                Varyings output;
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                float3 normal;
                float height;
                float variation;
                Deform(input, output.world, normal, height, variation);
                output.normal = normal;
                output.pos = UnityWorldToClipPos(output.world);
                output.albedo = lerp(_RootColor.rgb, _TipColor.rgb, height) * lerp(0.87, 1.08, variation);
                UNITY_TRANSFER_FOG(output, output.pos);
                TRANSFER_SHADOW_WPOS(output, output.world);
                return output;
            }

            half BladeLighting(half3 normal, half3 lightDir, half3 viewDir)
            {
                half incidence = dot(normal, lightDir);
                half backlight = saturate(-dot(lightDir, viewDir));
                return saturate(incidence) + saturate(-incidence) * backlight * backlight * _Translucency;
            }

            half3 AmbientLighting(half3 normal)
            {
                if (_FreyjaGrassAmbientSky.w > 0)
                {
                    half3 pole = normal.y >= 0 ? _FreyjaGrassAmbientSky.rgb : _FreyjaGrassAmbientGround.rgb;
                    return lerp(_FreyjaGrassAmbientEquator.rgb, pole, abs(normal.y));
                }

                return max(ShadeSH9(half4(normal, 1)), 0);
            }

            half4 Frag(Varyings input, fixed facing : VFACE) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                half3 normal = normalize(input.normal) * (facing > 0 ? 1 : -1);
                normal = normalize(lerp(normal, half3(0, 1, 0), _NormalUp));
                half3 viewDir = normalize(_WorldSpaceCameraPos - input.world);
                half3 lighting = AmbientLighting(normal);

                // ForwardBase supplies the directional light. The draw's local lights are explicit.
                #if defined(SHADOWS_SCREEN)
                    half shadow = UnityComputeForwardShadows(0, input.world, input._ShadowCoord);
                #else
                    half shadow = 1;
                #endif
                half3 sunDirection = _WorldSpaceLightPos0.xyz;
                lighting += _LightColor0.rgb * (shadow * BladeLighting(normal, sunDirection, viewDir));

                for (int index = 0; index < min(_FreyjaGrassLightCount, 4); index++)
                {
                    float4 source = _FreyjaGrassLightPositionRange[index];
                    float3 toLight = source.xyz - input.world;
                    float distanceSquared = dot(toLight, toLight);
                    half rangeFade = saturate(1 - distanceSquared / max(source.w * source.w, 0.0001));
                    half attenuation = rangeFade * rangeFade / (1 + distanceSquared);
                    half3 lightDirection = toLight * rsqrt(max(distanceSquared, 0.0001));
                    lighting += _FreyjaGrassLightColor[index].rgb
                        * (attenuation * BladeLighting(normal, lightDirection, viewDir));
                }

                half4 color = half4(input.albedo * lighting, 1);
                UNITY_APPLY_FOG(input.fogCoord, color);
                return color;
            }
            ENDCG
        }

        Pass
        {
            Name "SHADOWCASTER"
            Tags { "LightMode" = "ShadowCaster" }
            ZWrite On

            CGPROGRAM
            #pragma target 3.5
            #pragma vertex VertShadow
            #pragma fragment FragShadow
            #pragma multi_compile_instancing
            #pragma multi_compile_shadowcaster

            struct ShadowVaryings
            {
                V2F_SHADOW_CASTER;
            };

            ShadowVaryings VertShadow(Attributes v)
            {
                UNITY_SETUP_INSTANCE_ID(v);
                float3 world;
                float3 normal;
                float height;
                float variation;
                Deform(v, world, normal, height, variation);
                v.vertex = mul(unity_WorldToObject, float4(world, 1));
                v.normal = mul(normal, (float3x3)unity_ObjectToWorld);
                ShadowVaryings output;
                TRANSFER_SHADOW_CASTER_NORMALOFFSET(output);
                return output;
            }

            half4 FragShadow(ShadowVaryings input) : SV_Target
            {
                SHADOW_CASTER_FRAGMENT(input)
            }
            ENDCG
        }
    }
}
