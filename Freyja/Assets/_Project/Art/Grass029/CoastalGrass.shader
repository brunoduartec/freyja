Shader "Freyja/Coastal Grass"
{
    Properties
    {
        _RootColor ("Root", Color) = (0.25, 0.31, 0.13, 1)
        _TipColor ("Tip", Color) = (0.42, 0.48, 0.22, 1)
        _WindBend ("Wind Bend (m)", Range(0, 0.12)) = 0.075
        [HideInInspector] _FadeDistance ("Fade Range", Vector) = (28, 35, 0, 0)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }
        Cull Off

        Pass
        {
            CGPROGRAM
            #pragma target 3.5
            #pragma vertex Vert
            #pragma fragment Frag
            #pragma multi_compile_instancing
            #pragma instancing_options assumeuniformscaling
            #pragma multi_compile_fog
            #include "UnityCG.cginc"

            float4 _RootColor;
            float4 _TipColor;
            float _WindBend;
            float4 _FadeDistance;

            struct Attributes
            {
                float4 vertex : POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 position : SV_POSITION;
                half3 color : COLOR0;
                UNITY_FOG_COORDS(0)
            };

            Varyings Vert(Attributes input)
            {
                UNITY_SETUP_INSTANCE_ID(input);
                Varyings output;
                float3 root = unity_ObjectToWorld._m03_m13_m23;
                float variation = frac(sin(dot(root.xz, float2(47.21, 13.57))) * 43758.5453);
                float distanceToCamera = distance(root, _WorldSpaceCameraPos);
                float transition = lerp(_FadeDistance.x, _FadeDistance.y - 1.5, variation);
                float size = 1 - smoothstep(transition, transition + 1.5, distanceToCamera);
                float height = saturate(input.vertex.y / 0.31);
                float3 world = root + mul((float3x3)unity_ObjectToWorld, input.vertex.xyz) * size;

                const float2 wind = float2(0.8, 0.6);
                float gust = 0.63 + 0.28 * sin(dot(root.xz, wind) * 0.64 - _Time.y * 2.3);
                gust += 0.09 * sin(_Time.y * 4.0 + variation * 6.28);
                world.xz += wind * (height * height * gust * _WindBend * size);

                output.position = UnityWorldToClipPos(world);
                output.color = lerp(_RootColor.rgb, _TipColor.rgb, height) * lerp(0.87, 1.08, variation);
                UNITY_TRANSFER_FOG(output, output.position);
                return output;
            }

            fixed4 Frag(Varyings input) : SV_Target
            {
                fixed4 color = fixed4(input.color, 1);
                UNITY_APPLY_FOG(input.fogCoord, color);
                return color;
            }
            ENDCG
        }
    }
}
