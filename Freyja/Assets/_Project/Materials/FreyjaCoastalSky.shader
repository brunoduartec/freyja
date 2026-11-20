Shader "Freyja/Coastal Sky"
{
    Properties
    {
        _Zenith ("Zenith", Color) = (0.23, 0.49, 0.75, 1)
        _Horizon ("Horizon", Color) = (0.78, 0.78, 0.73, 1)
        _CloudLight ("Cloud Light", Color) = (1, 0.93, 0.80, 1)
        _CloudShade ("Cloud Shade", Color) = (0.53, 0.61, 0.73, 1)
        _SolarTint ("Solar Tint", Color) = (1, 0.84, 0.60, 1)
        _SolarDirection ("Solar Direction", Vector) = (0.8, 0.58, -0.1, 0)
        _CloudDrift ("Cloud Drift", Float) = 0
    }
    SubShader
    {
        Tags { "Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox" }
        Cull Off
        ZWrite Off
        Fog { Mode Off }
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "UnityCG.cginc"

            float4 _Zenith, _Horizon, _CloudLight, _CloudShade, _SolarTint;
            float4 _SolarDirection;
            float _CloudDrift;

            struct SkyVertex
            {
                float4 position : SV_POSITION;
                float3 direction : TEXCOORD0;
            };

            SkyVertex vert(float4 vertex : POSITION)
            {
                SkyVertex output;
                output.position = UnityObjectToClipPos(vertex);
                output.direction = vertex.xyz;
                return output;
            }

            float Ellipse(float2 p, float2 center, float2 radii)
            {
                return 1.0 - length((p - center) / radii);
            }

            float2 CloudBank(float turns, float elevation, float count, float phase, float height)
            {
                float track = turns * count + phase;
                float cell = floor(track);
                float id = fmod(fmod(cell, count) + count, count);
                float seed = frac(sin(id * 91.73 + count * 4.17) * 4163.37);
                float shape = frac(sin(id * 32.51 + count * 8.31) * 2957.29);
                float2 p = float2(frac(track) - lerp(0.43, 0.57, seed),
                    (elevation - height) / lerp(0.055, 0.115, shape) - (seed - 0.5) * 1.8);
                p.x /= lerp(0.63, 1.02, shape);
                p.y -= p.x * (seed - 0.5) * 0.6;

                float baseLeft = Ellipse(p, float2(-0.13, -0.12), float2(0.23, 0.38));
                float baseRight = Ellipse(p, float2(0.13, -0.06), float2(0.24, 0.36));
                float left = Ellipse(p, float2(-0.18, 0.11 + seed * 0.31), float2(0.15, 0.43));
                float crest = Ellipse(p, float2(lerp(-0.10, 0.10, seed), 0.31 + shape * 0.26),
                    float2(lerp(0.13, 0.24, shape), lerp(0.48, 0.70, seed)));
                float right = Ellipse(p, float2(0.18, lerp(0.13, 0.41, shape)), float2(0.15, 0.44));
                float silhouette = max(max(baseLeft, baseRight), max(crest, max(left, right)));

                float2 underCenter = float2(0.045 + (seed - 0.5) * 0.2, -0.36);
                float underside = Ellipse(p, underCenter, float2(0.42, 0.45 + shape * 0.16));
                float lit = 0.3 + 0.33 * step(0.72 - silhouette * 0.2, 1 - underside)
                    + 0.37 * step(0.98, 1 - underside + p.y * 0.14);
                return float2(smoothstep(-0.018, 0.055, silhouette), lit);
            }

            fixed4 frag(SkyVertex input) : SV_Target
            {
                float3 direction = normalize(input.direction);
                float elevation = asin(clamp(direction.y, -1.0, 1.0));
                float turns = atan2(direction.x, direction.z) / (2.0 * UNITY_PI);
                float altitude = saturate(direction.y);
                float3 color = lerp(_Horizon.rgb, _Zenith.rgb, smoothstep(0.0, 0.82, pow(altitude, 0.65)));
                color *= lerp(0.57, 1.0, smoothstep(-0.37, 0.0, direction.y));

                float3 sun = normalize(_SolarDirection.xyz);
                float solarAlignment = dot(direction, sun);
                float solarVisibility = smoothstep(-0.06, 0.035, sun.y);
                float halo = pow(saturate(solarAlignment), 24.0) * solarVisibility;
                color = lerp(color, _SolarTint.rgb, halo * 0.32);
                float disk = smoothstep(0.99855, 0.99905, solarAlignment) * solarVisibility;
                disk *= smoothstep(-0.012, 0.005, direction.y);
                color = lerp(color, lerp(_SolarTint.rgb, float3(1, 1, 0.87), 0.45), disk);

                float2 highCloud = CloudBank(turns, elevation, 6.0, 0.45 + _CloudDrift * 0.6, 0.68);
                float2 lowCloud = CloudBank(turns, elevation, 10.0, 0.28 + _CloudDrift, 0.27);
                float3 upperTint = lerp(_CloudShade.rgb, _CloudLight.rgb, highCloud.y);
                float3 lowerTint = lerp(_CloudShade.rgb, _CloudLight.rgb, lowCloud.y);
                color = lerp(color, upperTint, highCloud.x * 0.82);
                color = lerp(color, lowerTint, lowCloud.x * 0.98);
                return fixed4(color, 1);
            }
            ENDCG
        }
    }
    Fallback Off
}
