using System;
using System.Collections.Generic;
using UnityEngine;

namespace Utilities
{
    /// <summary>
    /// Matrix convolution for 2D array of floats
    /// </summary>
    public class Matrix
    {
        /// <summary>
        /// Returns a new matrix containing the return values of the provided expression for each element
        /// Expression is passed the current value, x coordinate, and y coordinate in that order
        /// </summary>
        /// <param name="matrix"></param>
        /// <param name="kernel"></param>
        /// <returns></returns>
        public static T[,] ValueFilter<T>(float[,] matrix, Func<float, int, int, T> expression)
        {
            int width = matrix.GetLength(0);
            int height = matrix.GetLength(1);

            T[,] newMatrix = new T[width, height];
            for (int x = 0; x < width; x++)
            {
                for (int y = 0; y < height; y++)
                {
                    newMatrix[x, y] = expression(matrix[x, y], x, y);
                }
            }
            return newMatrix;
        }
    }
}
