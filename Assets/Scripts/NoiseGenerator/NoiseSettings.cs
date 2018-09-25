using UnityEngine;
using Utilities;

namespace NoiseGenerator
{
    [CreateAssetMenu()]
    public class NoiseSettings : UpdatableSettings
    {
        public NoiseLayer[] noiseLayers;

        public void Validate()
        {
            foreach (NoiseLayer layer in noiseLayers)
            {
                layer.settings.Validate();
            }
        }
    }
}
