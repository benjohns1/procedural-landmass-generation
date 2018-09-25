using UnityEngine;

namespace NoiseGenerator.Filters
{
    public class Cellular : Filter
    {
        private const int randPrecision = 10000;

        private FilterSettings.Cellular settings;
        private int[,] cellMap;
        private readonly int cellTypeCount;

        public override float GetGlobalMin()
        {
            return 0;
        }

        public override float GetGlobalMax()
        {
            return cellTypeCount - 1;
        }

        public Cellular(FilterSettings.Cellular settings)
        {
            generationMode = GenerationMode.FullRegion;
            settings.Validate();
            this.settings = settings;
            cellTypeCount = settings.cellTypes.colorKeys.Length;
        }

        public override float[,] GenerateRegion(int width, int height, Vector2 startPoint)
        {
            System.Random rand = new System.Random(settings.seed);

            int[,] cellMap = new int[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // @TODO: use startpoint as offset to make this infinitely tileable
                    float t = (float)rand.Next(randPrecision) / randPrecision;
                    float key = settings.cellTypes.Evaluate(t).grayscale;
                    cellMap[x, y] = Mathf.RoundToInt(Mathf.Lerp(0, cellTypeCount - 1, key));
                }
            }

            float[,] valueMap = new float[width, height];
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    valueMap[x, y] = cellMap[x, y];
                }
            }
            return valueMap;
        }

        public static int GetCellCoord(float coord, int dimension)
        {
            return Mathf.Abs(Mathf.RoundToInt(coord) % dimension);
        }
    }
}
