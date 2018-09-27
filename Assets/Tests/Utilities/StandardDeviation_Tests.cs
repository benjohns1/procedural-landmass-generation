using NUnit.Framework;
using System;
using UnityEngine;

namespace Utilities
{
    public class StandardDeviation_Tests
    {
        [Test]
        public void GetKernel_NegativeOne()
        {
            Assert.Throws<ArgumentException>(() => StandardDeviation.GetKernel(-1));
        }

        [Test]
        public void GetKernel_OneNormalized()
        {
            float[] matrix = StandardDeviation.GetKernel(1);
            Assert.AreEqual(0.3990503f, matrix[3]);
            Assert.AreEqual(7, matrix.Length);
            ValidateNormalizedMatrixForm(matrix);
        }

        [Test]
        public void GetKernel_TwoNormalized()
        {
            float[] matrix = StandardDeviation.GetKernel(2);
            Assert.AreEqual(0.0648251921f, matrix[3]);
            Assert.AreEqual(13, matrix.Length);
            ValidateNormalizedMatrixForm(matrix);
        }

        [Test]
        public void GetKernel_Zero()
        {
            Assert.Throws<ArgumentException>(() => StandardDeviation.GetKernel(0));
        }

        [Test]
        public void GetKernel_ZeroPointFiveNormalized()
        {
            float[] matrix = StandardDeviation.GetKernel(0.5f);
            Assert.AreEqual(0.7865707f, matrix[2]);
            Assert.AreEqual(5, matrix.Length);
            ValidateNormalizedMatrixForm(matrix);
        }

        [Test]
        public void GetKernel_ZeroPointTwoNormalized()
        {
            float[] matrix = StandardDeviation.GetKernel(0.2f);
            Assert.AreEqual(0.9999926f, matrix[1]);
            Assert.AreEqual(3, matrix.Length);
            ValidateNormalizedMatrixForm(matrix);
        }

        private void ValidateNormalizedMatrixForm(float[] matrix)
        {
            int length = matrix.Length;
            Assert.AreEqual(1, length % 2);
            int middleIndex = length / 2;
            int lastIndex = length - 1;
            float sum = 0;
            for (int i = 0; i < lastIndex; i++)
            {
                Assert.LessOrEqual(0, matrix[i]);
                if (i < middleIndex)
                {
                    Assert.Less(matrix[i], matrix[i + 1]);
                    Assert.AreEqual(matrix[i], matrix[lastIndex - i]);
                }
                else
                {
                    Assert.Greater(matrix[i], matrix[i + 1]);
                }
                sum += matrix[i];
            }
            Assert.AreEqual(1f, sum + matrix[lastIndex], 0.000001f);
        }
    }
}