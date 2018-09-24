using UnityEngine;

namespace NoiseGenerator
{
    [CreateAssetMenu()]
    public class NoiseSettings : ScriptableObject
    {
        public NoiseLayer[] noiseLayers;
    }
}
