using UnityEngine;

namespace NoiseGenerator.Filters
{
    public class Constant : IFilter
    {
        private FilterSettings.Constant settings;

        public Constant(FilterSettings.Constant settings)
        {
            this.settings = settings;
        }

        public float GetMin()
        {
            return settings.value;
        }

        public float GetMax()
        {
            return settings.value;
        }

        public float Evaluate(Vector2 point, float previousValue)
        {
            return settings.value;
        }
    }
}
