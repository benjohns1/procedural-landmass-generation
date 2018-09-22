using UnityEngine;

[CreateAssetMenu()]
public class TerrainData : UpdatableData
{
    public float uniformScale = 2f;

    public bool useFalloff;
    public bool useFlatShading;

    public float meshheightMultiplier;
    public AnimationCurve meshHeightCurve;
}
