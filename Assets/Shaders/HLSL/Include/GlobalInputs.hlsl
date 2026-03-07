#ifndef GLOBAL_INPUTS
#define GLOBAL_INPUTS

#if defined(TEXTURE2D)
    TEXTURE2D_HALF(_SKY_VIEW_LUT);          SAMPLER(sampler_SKY_VIEW_LUT);
    TEXTURE2D_HALF(_DITHER);                SAMPLER(sampler_DITHER);
#endif

uniform float3 _SUN_DIR;
uniform float _PLANET_RADIUS;

#endif