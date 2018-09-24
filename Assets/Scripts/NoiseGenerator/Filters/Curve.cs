using UnityEngine;

namespace NoiseGenerator.Filters
{
    public class Curve : IFilter
    {
        private AnimationCurve threadSafeCurve;
        private MinMaxFloat minMax;

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
            return threadSafeCurve.Evaluate(previousValue);
        }
    }
}
