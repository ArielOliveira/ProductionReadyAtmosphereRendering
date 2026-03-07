#ifndef ATMOSPHERE_INPUTS
#define ATMOSPHERE_INPUTS

#ifndef UNIVERSAL_PIPELINE_CORE_INCLUDED
    #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
#endif

CBUFFER_START(AtmosphereParameters) 
    float4 _RayleighScatteringBase;
    float4 _AtmosphereAbsorptionBase;
    float4 _GroundAlbedo;
    float2 _AtmosphereAbsorptionRange;
    
    float _MieScatteringBase;
    float _MieAbsorptionBase;
    float _RayleighDensityFalloff;
    float _MieDensityFalloff;
    float _PlanetRadius;
    float _AtmosphereRadius;
CBUFFER_END

CBUFFER_START(AtmosphereLUTParameters)
    int2 _SunTLUTRes;
    int2 _MSLUTRes;
    int2 _SkyViewLUTRes;

    int _HemisphereSamples;
    int _OfflineDepthSamples;
    int _RuntimeDepthSamples;
CBUFFER_END

#endif