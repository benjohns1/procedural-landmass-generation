using UnityEngine;

namespace NoiseGenerator.Filters
{
    public class Perlin : IFilter
    {
        private FilterSettings.Perlin settings;

        public Perlin(FilterSettings.Perlin settings)
        {
            this.settings = settings;
        }

        public float Evaluate(Vector3 point)
        {
            throw new System.NotImplementedException();
        }
    }
}
