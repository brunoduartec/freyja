#ifndef FREYJA_WIND_CORE_INCLUDED
#define FREYJA_WIND_CORE_INCLUDED

float4 _FreyjaWind;
float4 _FreyjaWindGust;

// Returns horizontal displacement at full height before species-specific falloff.
float2 FreyjaWindOffset(float2 worldRoot, float variation, float maxBend)
{
    float2 direction = _FreyjaWind.xy;
    direction *= rsqrt(max(dot(direction, direction), 0.0001));
    float phase = dot(worldRoot, direction) * max(_FreyjaWindGust.x, 0) - _FreyjaWind.w;
    float gust = sin(phase) * saturate(_FreyjaWindGust.y);
    float turbulence = sin(phase * 2 + variation * 6.2831853) * saturate(_FreyjaWindGust.z);
    float amplitude = saturate(_FreyjaWind.z * (0.63 + gust + turbulence));
    return direction * (max(maxBend, 0) * amplitude);
}

#endif
