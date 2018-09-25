using UnityEngine;

namespace NoiseGenerator
{
    public interface IFilter
    {
        void Setup(float globalMin, float globalMax);
        void StartNewRegion(int width, int height);
        float Evaluate(Vector2 point);
        float GetMin();
        float GetMax();
    }
}