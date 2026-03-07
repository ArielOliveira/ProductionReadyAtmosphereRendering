using UnityEngine;
using Arielado.Lighting.Models;
using System;

namespace Arielado.Lighting {
    [CreateAssetMenu(fileName = "AtmosphereParameters", menuName = "Data/Lighting/AtmosphereParameters")]
    public class AtmosphereParametersSO : ScriptableObject {
        #if UNITY_EDITOR
        public event Action onDataChange;
        private void OnValidate() {
            onDataChange?.Invoke();

            UnityEditor.EditorUtility.SetDirty(this);
        }
        #endif

        [SerializeField] private AtmosphereParameters atmosphereParameters;
        public AtmosphereParameters Parameters => atmosphereParameters;
        public Vector3 RayleighScatteringBase => atmosphereParameters.RayleighScatteringBase;
        public Vector3 AtmosphereAbsorptionBase => atmosphereParameters.AtmosphereAbsorptionBase;
        public Vector3 GroundAlbedo => atmosphereParameters.GroundAlbedo;
        public float RayleighDensityFalloff => atmosphereParameters.rayleighDensityFalloff;
        public float MieScatteringBase => atmosphereParameters.MieScatteringBase;
        public float MieAbsorptionBase => atmosphereParameters.MieAbsorptionBase;
        public float MieDensityFalloff => atmosphereParameters.mieDensityFalloff;
        public float PlanetRadius => atmosphereParameters.planetRadius;
        public float AtmosphereRadius => atmosphereParameters.AtmosphereRadius;
    }
}
