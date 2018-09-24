using UnityEngine;

namespace NoiseGenerator
{
    [System.Serializable]
    public class NoiseLayer
    {
        public enum LayerOperator { Set, Add, Multiply }
        public LayerOperator layerOperator;

        public bool enabled = true;
        public FilterSettings settings;
        public IFilter filter;
    }
}
