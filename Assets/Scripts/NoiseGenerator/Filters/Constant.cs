using UnityEngine;

namespace NoiseGenerator.Filters
{
    public class Constant : global::Filter
    {

        private FilterSettings.Constant settings;

        public override float GetGlobalMin()
        {
            return settings.value;
        }

        public override float GetGlobalMax()
        {
            return settings.value;
        }

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

        public override float Evaluate(Vector2 point)
        {
            return settings.value;
        }
    }
}
