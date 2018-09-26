using UnityEngine;

public abstract class Filter
{
    public virtual void Setup(float previousGlobalMin, float previousGlobalMax) { }

    public virtual float[,] GenerateRegion(int width, int height, Vector2 startPoint)
    {
        float[,] region = new float[width, height];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                region[x, y] = Evaluate(x + startPoint.x, y - startPoint.y);
            }
        }
        return region;
    }

    public abstract float GetGlobalMax();

    public abstract float GetGlobalMin();

    protected virtual float Evaluate(float x, float y)
    {
        return 0;
    }
}
