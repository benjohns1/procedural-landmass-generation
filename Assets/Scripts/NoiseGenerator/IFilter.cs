using UnityEngine;

namespace NoiseGenerator
{
    public interface IFilter
    {
        float Evaluate(Vector2 point, float previousValue);
        float GetMin();
        float GetMax();
    }
}