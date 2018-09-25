using UnityEngine;
using Utilities;

namespace NoiseGenerator
{
    [CreateAssetMenu()]
    public class NoiseSettings : UpdatableSettings
    {
        public NoiseLayer[] noiseLayers;

#if UNITY_EDITOR

        protected override void OnValidate()
        {
            foreach (NoiseLayer layer in noiseLayers)
            {
                layer.settings.Validate();
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
