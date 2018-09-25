using UnityEngine;

public abstract class Filter
{
    public enum GenerationMode { EvaluatePoints, FullRegion }
    public GenerationMode generationMode = GenerationMode.EvaluatePoints;

    public virtual void Setup(float previousGlobalMin, float previousGlobalMax) { }
    public virtual float[,] GenerateRegion(int width, int height, Vector2 startPoint) { throw new System.NotImplementedException("Evaluate method must be overridden for GenerationMode.EvaluatePoints"); }
    public virtual float Evaluate(Vector2 point) { throw new System.NotImplementedException("Evaluate method must be overridden for GenerationMode.EvaluatePoints"); }
    public abstract float GetGlobalMax();
    public abstract float GetGlobalMin();
}
