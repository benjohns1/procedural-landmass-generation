using UnityEngine;

namespace NoiseGenerator
{
    public interface IFilter
    {
        void Setup(float globalMin, float globalMax);
        float Evaluate(Vector2 point, float previousValue);
        float GetMin();
        float GetMax();
    }
}