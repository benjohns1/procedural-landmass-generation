using UnityEngine;

namespace NoiseGenerator
{
    public struct Region
    {
        public readonly float[,] values;
        public readonly float minValue;
        public readonly float maxValue;

        public Region(float[,] values, float minValue, float maxValue)
        {
            this.values = values;
            this.minValue = minValue;
            this.maxValue = maxValue;
        }
    }
}