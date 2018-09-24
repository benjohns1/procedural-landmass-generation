using UnityEngine;

namespace NoiseGenerator
{
    [System.Serializable]
    public class NoiseLayer
    {
        public enum LayerType { Set, Add, Multiply }

        public bool enabled = true;
        public LayerType layerType;
        public FilterSettings settings;
    }
}
