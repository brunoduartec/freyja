Shader "Freyja/Coastal Water"
{
    Properties
    {
        _Shallows ("Shallows", Color) = (0.19, 0.68, 0.64, 1)
        _OpenWater ("Open Water", Color) = (0.08, 0.39, 0.49, 1)
        _FoamColor ("Foam", Color) = (0.74, 0.89, 0.83, 1)
        _HorizonColor ("Horizon", Color) = (0.53, 0.70, 0.79, 1)
        _Coast ("Coast Distance", 2D) = "white" {}
        _CoastMapping ("Origin XZ, inverse size, max distance", Vector) = (-80, -80, 0.00625, 24)
        _Shelter ("Sheltered cove center and radii", Vector) = (-37, -61, 23, 18)
        _WaveHeight ("Geometric wave height (m)", Range(0, 0.6)) = 0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry" }
        Pass
        {
            CGPROGRAM
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            sampler2D _Coast;
            float4 _CoastMapping, _Shelter;
            fixed4 _Shallows, _OpenWater, _FoamColor, _HorizonColor;
            float _WaveHeight;

            struct Surface
            {
                float4 position : SV_POSITION;
                float2 meters : TEXCOORD0;
                float depth : TEXCOORD1;
            };

            float3 WavePhases(float2 meters)
            {
                return float3(dot(meters, float2(0.36, 0.12)),
                              dot(meters, float2(0.08, 0.44)),
                              dot(meters, float2(-0.04, 0.18))) - _Time.y * float3(1.15, 0.83, 0.67);
            }

            float Shelter(float2 meters)
            {
                float coveDistance = length((meters - _Shelter.xy) / _Shelter.zw);
                return 1.0 - smoothstep(0.45, 1.55, coveDistance);
            }

            float CrestPhase(float3 phases)
            {
                return 0.64 * phases.x + 0.20 * sin(phases.z);
            }

            float BeachFoam(float2 meters, float distanceToSand, float breakup)
            {
                float rhythm = 0.12 * sin(dot(meters, float2(0.070, 0.043)));
                float age = frac(_Time.y * 0.21 + rhythm);
                float arrival = smoothstep(0.0, 0.81, age);
                float crest = lerp(4.25, 0.08, arrival);
                crest += 0.20 * sin(dot(meters, float2(0.46, 0.30))) * (1.0 - arrival);
                float width = lerp(0.23, 0.68, arrival);
                float foamBand = 1.0 - smoothstep(width * 0.22, width, abs(distanceToSand - crest));
                float energy = smoothstep(0.07, 0.25, age) * (1.0 - smoothstep(0.71, 0.98, age));
                float raggedEdge = lerp(0.58, 1.0, smoothstep(-0.45, 0.44, breakup));

                float backwash = (1.0 - smoothstep(0.12, 1.15, distanceToSand)) *
                                 smoothstep(0.51, 0.72, age) * (1.0 - smoothstep(0.79, 1.0, age));
                return saturate((foamBand * energy + backwash * 0.52) * raggedEdge);
            }

            Surface Vert(float4 vertex : POSITION)
            {
                Surface output;
                float3 world = mul(unity_ObjectToWorld, vertex).xyz;
                output.depth = -mul(UNITY_MATRIX_V, float4(world, 1)).z;
                if (_WaveHeight > 0)
                {
                    float3 phases = WavePhases(world.xz);
                    float swell = sin(CrestPhase(phases));
                    float amplitude = _WaveHeight * lerp(1.0, 0.22, Shelter(world.xz));
                    amplitude *= 1.0 - smoothstep(70.0, 135.0, output.depth);
                    world.y += amplitude * (0.86 * swell + 0.14 * sin(phases.z));
                }
                output.position = UnityWorldToClipPos(world);
                output.meters = world.xz;
                return output;
            }

            fixed4 Frag(Surface input) : SV_Target
            {
                float2 meters = input.meters;
                float shelter = Shelter(meters);
                float2 coastUV = (meters - _CoastMapping.xy) * _CoastMapping.z;
                float2 distances = tex2D(_Coast, coastUV).rg * _CoastMapping.w;
                float metersFromCoast = distances.x;
                float farFade = smoothstep(108.0, 147.0, input.depth);

                float3 phases = WavePhases(meters);
                float crestPhase = CrestPhase(phases);
                float swellsVisible = 1.0 - smoothstep(32.0, 94.0, input.depth);
                float swell = sin(crestPhase);
                float finePhase = dot(meters, float2(1.15, 0.42)) - _Time.y * 1.45;
                float ripple = sin(finePhase + sin(phases.y));
                float rippleFade = (1.0 - smoothstep(14.0, 50.0, input.depth)) *
                                   (1.0 - smoothstep(4.0, 14.0, metersFromCoast));

                float shallowness = 1.0 - smoothstep(0.4, 15.0, metersFromCoast);
                fixed3 color = lerp(_OpenWater.rgb, _Shallows.rgb, shallowness * 0.84);
                color *= 1.0 + (swell * swellsVisible * 0.035 + ripple * rippleFade * 0.018) *
                                   lerp(1.0, 0.42, shelter);
                if (_WaveHeight > 0)
                {
                    // The face tint follows the slope of the main geometric swell.
                    float faceLight = cos(crestPhase);
                    float slopeContrast = _WaveHeight * lerp(1.0, 0.22, shelter) * swellsVisible;
                    color *= 1.0 + faceLight * slopeContrast * 0.10;
                }

                float breakup = sin(dot(meters, float2(1.04, 0.57)) + sin(phases.y)) * 0.5 +
                                sin(dot(meters, float2(-0.44, 2.05)) - _Time.y * 0.59) * 0.5;
                float pulse = sin(_Time.y * 1.65 + dot(meters, float2(0.47, 0.66)));
                float pulseAmount = pulse * 0.5 + 0.5;
                float coastWidth = lerp(0.24, 1.17, pulseAmount);
                float pierWidth = lerp(0.34, 0.74, pulseAmount);
                float foamWidth = lerp(coastWidth, pierWidth, shelter);
                float contact = 1.0 - smoothstep(foamWidth - 0.20, foamWidth, metersFromCoast);
                float brokenCoast = smoothstep(-0.17, 0.48, breakup + 0.31 * pulse);
                float brokenPier = smoothstep(-0.34, 0.39, breakup + 0.28 * pulse);
                float foam = contact * lerp(brokenCoast, brokenPier, shelter);
                foam *= 1.0 - smoothstep(27.0, 79.0, input.depth);
                float shorelineFoam = foam * lerp(0.66, 0.48, shelter);
                float beachFoam = BeachFoam(meters, distances.y, breakup);
                beachFoam *= 1.0 - smoothstep(42.0, 96.0, input.depth);
                color = lerp(color, _FoamColor.rgb, max(shorelineFoam, beachFoam * 0.86));

                return fixed4(lerp(color, _HorizonColor.rgb, farFade), 1);
            }
            ENDCG
        }
    }
}
