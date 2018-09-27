using NUnit.Framework;
using System;

namespace Utilities
{
    public class Convolution_Tests
    {
        private const float floatPrecision = 0.0001f;

        [Test]
        public void Symmetric_5x1SampleBlurringIsCorrect()
        {
            float[,] map = new float[,]
            {
                {0,0,1,0,0}
            };
            float[] kernel = StandardDeviation.GetKernel(0.5f);
            float[,] blurredMap = Convolution.Symmetric(map, kernel);
            Assert.AreEqual(0.0002075485f, blurredMap[0,0], floatPrecision);
            Assert.AreEqual(0.08373106f, blurredMap[0, 1], floatPrecision);
            Assert.AreEqual(0.6186935f, blurredMap[0, 2], floatPrecision);
            Assert.AreEqual(0.08373106f, blurredMap[0, 3], floatPrecision);
            Assert.AreEqual(0.0002075485f, blurredMap[0, 4], floatPrecision);
        }

        [Test]
        public void Symmetric_1x5SampleBlurringIsCorrect()
        {
            float[,] map = new float[,]
            {
                { 0 },
                { 0 },
                { 1 },
                { 0 },
                { 0 }
            };
            float[] kernel = StandardDeviation.GetKernel(0.5f);
            float[,] blurredMap = Convolution.Symmetric(map, kernel);
            Assert.AreEqual(0.0002075485f, blurredMap[0, 0], floatPrecision);
            Assert.AreEqual(0.08373106f, blurredMap[1, 0], floatPrecision);
            Assert.AreEqual(0.6186935f, blurredMap[2, 0], floatPrecision);
            Assert.AreEqual(0.08373106f, blurredMap[3, 0], floatPrecision);
            Assert.AreEqual(0.0002075485f, blurredMap[4, 0], floatPrecision);
        }

        [Test]
        public void Symmetric_EvenKernelLengthThrowsException()
        {
            float[] kernel = new float[] { 0, 0 };
            Assert.Throws<ArgumentException>(() => Convolution.Symmetric(new float[0, 0], kernel));
        }

        [Test]
        public void Symmetric_ReturnsSameSizeArray()
        {
            int width = 5;
            int height = 200;
            float[,] map = new float[width, height];
            float[,] blurredMap = Convolution.Symmetric(map, new float[1]);
            Assert.AreEqual(map.GetLength(0), blurredMap.GetLength(0));
            Assert.AreEqual(map.GetLength(1), blurredMap.GetLength(1));
        }


    }
}