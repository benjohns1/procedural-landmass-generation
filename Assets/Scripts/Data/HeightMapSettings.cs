using UnityEngine;

[CreateAssetMenu()]
public class HeightMapSettings : UpdatableData
{
    public NoiseGenerator.NoiseSettings noiseSettings;

    public bool useFalloff;

    public float heightMultiplier;
    public AnimationCurve heightCurve;

    public float minHeight
    {
        get
        {
            return heightMultiplier * heightCurve.Evaluate(0);
        }
    }

    public float maxHeight
    {
        get
        {
            return heightMultiplier * heightCurve.Evaluate(1);
        }
    }

#if UNITY_EDITOR

    protected override void OnValidate()
    {
        if (noiseSettings != null)
        {
            noiseSettings.OnValuesUpdated -= OnChildValuesUpdated;
            noiseSettings.OnValuesUpdated += OnChildValuesUpdated;
            noiseSettings.Validate();
        }
        base.OnValidate();
    }

    private void OnChildValuesUpdated()
    {
        NotifyOfUpdatedValues();
    }

#endif

}
