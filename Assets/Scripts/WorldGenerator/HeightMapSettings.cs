using UnityEngine;
using Utilities;

namespace WorldGenerator
{
    [CreateAssetMenu()]
    public class HeightMapSettings : UpdatableSettings
    {
        public NoiseGenerator.NoiseSettings noiseSettings;

#if UNITY_EDITOR

        protected override void OnValidate()
        {
            if (noiseSettings != null)
            {
                noiseSettings.OnValuesUpdated -= OnChildValuesUpdated;
                noiseSettings.OnValuesUpdated += OnChildValuesUpdated;
                noiseSettings.Validate();
            }
            base.OnValidate();
        }

        private void OnChildValuesUpdated()
        {
            NotifyOfUpdatedValues();
        }

#endif

    }
}