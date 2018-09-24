using UnityEngine;

namespace NoiseGenerator
{
    [CreateAssetMenu()]
    public class NoiseSettings : UpdatableData
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
