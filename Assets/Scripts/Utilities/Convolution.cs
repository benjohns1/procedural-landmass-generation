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
        /// Apply a 2D convolution kernel to a 2D array
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="kernel"></param>
        /// <returns></returns>
        public static float[,] Convolve(float[,] matrix, float[,] kernel, int xStart = 0, int? _xEnd = null, int yStart = 0, int? _yEnd = null)
        {
            int kernelWidth = kernel.GetLength(0);
            int kernelHeight = kernel.GetLength(1);
            if (kernelWidth % 2 == 0 && kernelHeight % 2 == 0)
            {
                throw new System.ArgumentException("Kernel must have an odd number of elements in both dimensions");
            }
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);
            int xEnd = _xEnd ?? width - 1;
            int yEnd = _yEnd ?? height - 1;

            float[,] convolved = new float[width, height];
            for (int y = yStart; y <= yEnd; y++)
            {
                for (int x = xStart; x <= xEnd; x++)
                {
                    convolved[x, y] = processPoint(matrix, width, height, x, y, kernel, kernelWidth, kernelHeight);
                }
            }

            return convolved;
        }

        private static float processPoint(float[,] matrix, int width, int height, int x, int y, float[,] kernel, int kernelWidth, int kernelHeight)
        {
            float convolvedValue = 0;
            for (int j = 0; j < kernelHeight; j++)
            {
                int cy = y + j - (kernelHeight / 2);
                if (cy < 0 || cy >= height)
                {
                    continue;
                }
                for (int i = 0; i < kernelWidth; i++)
                {
                    int cx = x + i - (kernelWidth / 2);
                    if (cx < 0 || cx >= width)
                    {
                        continue;
                    }
                    convolvedValue += matrix[cx, cy] * kernel[i, j];
                }
            }
            return convolvedValue;
        }

        /// <summary>
        /// Apply a 1D convolution kernel symmetrically to both dimensions of a 2D array in 2 passes (slightly faster than using a 2D kernel, but not exact)
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="kernel"></param>
        /// <returns></returns>
        public static float[,] Symmetric(float[,] matrix, float[] kernel, int xStart = 0, int? _xEnd = null, int yStart = 0, int? _yEnd = null)
        {
            if (kernel.Length % 2 == 0)
            {
                throw new System.ArgumentException("Kernel must have an odd number of elements");
            }
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);
            int halfIndex = kernel.Length / 2;
            int xEnd = _xEnd ?? width - 1;
            int yEnd = _yEnd ?? height - 1;

            // Horizontal pass
            float[,] horizontalConvolved = new float[width, height];
            for (int y = yStart; y <= yEnd; y++)
            {
                for (int x = xStart; x <= xEnd; x++)
                {
                    horizontalConvolved[x, y] = processPointSymmetric(matrix, width, height, x, y, kernel, halfIndex, false);
                }
            }

            // Vertical pass
            float[,] convolved = new float[width, height];
            for (int y = yStart; y <= yEnd; y++)
            {
                for (int x = xStart; x <= xEnd; x++)
                {
                    convolved[x, y] = processPointSymmetric(horizontalConvolved, width, height, x, y, kernel, halfIndex, true);
                }
            }

            return convolved;
        }

        private static float processPointSymmetric(float[,] matrix, int width, int height, int x, int y, float[] kernel, int halfIndex, bool vertical)
        {
            float convolvedValue = 0;
            for (int i = 0; i < kernel.Length; i++)
            {
                int cx = !vertical ? x + i - halfIndex : x;
                int cy = vertical ? y + i - halfIndex : y;
                if (cx >= 0 && cx < width && cy >= 0 && cy < height)
                {
                    convolvedValue += matrix[cx, cy] * kernel[i];
                }
            }
            return convolvedValue;
        }
    }
}
