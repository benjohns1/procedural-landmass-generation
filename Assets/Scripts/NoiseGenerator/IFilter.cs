using UnityEngine;

namespace NoiseGenerator
{
    public interface IFilter
    {
        float Evaluate(Vector3 point);
    }
}