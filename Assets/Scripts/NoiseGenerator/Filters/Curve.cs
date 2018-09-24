using UnityEngine;

namespace NoiseGenerator.Filters
{
    public class Curve : IFilter
    {
        private AnimationCurve threadSafeCurve;
        private MinMaxFloat minMax;
        private float globalMin;
        private float globalMax;

        public Curve(FilterSettings.Curve settings)
        {
            threadSafeCurve = new AnimationCurve(settings.curve.keys);
            minMax = new MinMaxFloat();
            for (int i = 0; i < threadSafeCurve.length; i++)
            {
                float val = threadSafeCurve[i].value;
                minMax.AddValue(val);
            }
        }

        public void Setup(float globalMin, float globalMax)
        {
            this.globalMin = globalMin;
            this.globalMax = globalMax;
        }

        public float GetMin()
        {
            return minMax.Min;
        }

        public float GetMax()
        {
            return minMax.Max;
        }

        public float Evaluate(Vector2 point, float previousValue)
        {
            float t = Mathf.InverseLerp(globalMin, globalMax, previousValue);
            float newT = threadSafeCurve.Evaluate(t);
            float clamped = Mathf.Clamp01(newT);
            return Mathf.Lerp(globalMin, globalMax, newT);
        }
    }
}
