using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    /// <summary>
    /// Gaussian blur for 2D array of floats
    /// </summary>
    public class StandardDeviation
    {
        private static Dictionary<float, float[]> cache = new Dictionary<float, float[]>();

        public static float[] GetKernel(float deviation, bool normalize = true)
        {
            if (deviation <= 0)
            {
                throw new System.ArgumentException("Deviation must be greater than 0");
            }
            float[] map;
            if (cache.TryGetValue(deviation, out map))
            {
                return map;
            }

            int size = Mathf.CeilToInt(deviation * 3) * 2 + 1;
            map = new float[size];

            float sum = 0;
            float half = (size - 1) / 2;
            for (int i = 0; i < size; i++)
            {
                float value = 1 / (Mathf.Sqrt(2 * Mathf.PI) * deviation) * Mathf.Exp(-(i - half) * (i - half) / (2 * deviation * deviation));
                sum += value;
                map[i] = value;
            }

            return Normalize(map, sum);
        }

        private static float[] Normalize(float[] matrix, float? _sum = null)
        {
            int length = matrix.Length;
            float[] normalized = new float[length];
            float sum = _sum ?? Sum(matrix);
            if (sum <= 0)
            {
                return normalized;
            }
            for (int i = 0; i < length; i++)
            {
                normalized[i] = matrix[i] / sum;
            }
            return normalized;
        }

        private static float Sum(float[] matrix)
        {
            float sum = 0;
            for (int i = 0; i < matrix.Length; i++)
            {
                sum += matrix[i];
            }
            return sum;
        }
    }
}
