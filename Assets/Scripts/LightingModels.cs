using UnityEngine;

namespace Arielado.Lighting.Models { 
    public struct AtmosphereParametersGPUAligned {
        public Vector4 rayleighScatteringBase;
        public Vector4 atmosphereAbsorptionBase;
        public Vector4 groundAlbedo;
        public Vector2 atmosphereAbsorptionRange;
        public float mieScatteringBase;
        public float mieAbsorptionBase;
        public float rayleighDensityFalloff;
        public float mieDensityFalloff;
        public float planetRadius;
        public float atmosphereRadius;
    }

    [System.Serializable]
    public struct AtmosphereParameters {
        [SerializeField] private Color rayleighScatteringBase;
        [SerializeField] private Color atmosphereAbsorptionBase;
        [SerializeField] private Color groundAlbedo;
        [SerializeField] private Vector2 atmosphereAbsorptionRange;
        [SerializeField, Min(0)] private float mieScatteringBase;
        [SerializeField, Min(0)] private float mieAbsorptionBase;
        [SerializeField, Range(0.0f, 2f)] private float rayleighScatteringCoefficient;
        [SerializeField, Range(0.0002f, 2f)] private float mieScatteringCoefficient;
        [SerializeField, Range(0.0002f, 2f)] private float mieAbsorptionCoefficient;
        [SerializeField, Range(0.0001f, 1f)] private float atmosphereAbsorptionCoefficient;
        [Range(0.001f, 0.995f)] public float rayleighDensityFalloff;
        [Range(0.001f, 0.995f)] public float mieDensityFalloff;
        public float planetRadius;
        [SerializeField, Range(1, 300)] private float atmosphereThickness;

        public Vector3 RayleighScatteringBase => (Vector4)rayleighScatteringBase * rayleighScatteringCoefficient;
        public Vector3 AtmosphereAbsorptionBase => (Vector4)atmosphereAbsorptionBase * atmosphereAbsorptionCoefficient;
        public Vector3 GroundAlbedo => (Vector4)groundAlbedo;
        public float MieScatteringBase => mieScatteringBase * mieScatteringCoefficient;
        public float MieAbsorptionBase => mieAbsorptionBase * mieAbsorptionCoefficient;
        public float AtmosphereRadius => planetRadius + atmosphereThickness;

        public static AtmosphereParametersGPUAligned GetGPUAlignedData(AtmosphereParameters ap) => new AtmosphereParametersGPUAligned() {
            rayleighScatteringBase = ap.RayleighScatteringBase,
            atmosphereAbsorptionBase = ap.AtmosphereAbsorptionBase,
            groundAlbedo = ap.GroundAlbedo,
            atmosphereAbsorptionRange = ap.atmosphereAbsorptionRange,
            mieScatteringBase = ap.MieScatteringBase,
            mieAbsorptionBase = ap.MieAbsorptionBase,
            rayleighDensityFalloff = ap.rayleighDensityFalloff,
            mieDensityFalloff = ap.mieDensityFalloff,
            planetRadius = ap.planetRadius,
            atmosphereRadius = ap.AtmosphereRadius
        };
    }

    [System.Serializable]
    public struct AtmosphereLUTParameters {
        public Vector2Int sTLUTRes;
        public Vector2Int mSLUTRes;
        public Vector2Int skyViewLUTRes;

        public int hemisphereSamples;
        public int offlineDepthSamples;
        public int runtimeDepthSamples;
    }
}
