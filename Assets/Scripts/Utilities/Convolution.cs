using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    /// <summary>
    /// Matrix convolution for 2D array of floats
    /// </summary>
    public class Convolution
    {
        /// <summary>
        /// Apply a 1D convolution kernel symmetrically to both dimensions of a 2D array
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="kernel"></param>
        /// <returns></returns>
        public static float[,] Symmetric(float[,] matrix, float[] kernel)
        {
            if (kernel.Length % 2 == 0)
            {
                throw new System.ArgumentException("Kernel must have an odd length of elements");
            }
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);
            int halfIndex = kernel.Length / 2;

            // Horizontal pass
            float[,] horizontalBlurred = new float[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    horizontalBlurred[x, y] = processPoint(matrix, width, height, x, y, kernel, halfIndex, false);
                }
            }

            // Vertical pass
            float[,] bothBlurred = new float[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    bothBlurred[x, y] = processPoint(horizontalBlurred, width, height, x, y, kernel, halfIndex, true);
                }
            }

            return bothBlurred;
        }

        private static float processPoint(float[,] matrix, int width, int height, int x, int y, float[] kernel, int halfIndex, bool vertical)
        {
            float blurredValue = 0;
            for (int i = 0; i < kernel.Length; i++)
            {
                int cx = !vertical ? x + i - halfIndex : x;
                int cy = vertical ? y + i - halfIndex : y;
                if (cx >= 0 && cx < width && cy >= 0 && cy < height)
                {
                    blurredValue += matrix[cx, cy] * kernel[i];
                }
            }
            return blurredValue;
        }
    }
}
