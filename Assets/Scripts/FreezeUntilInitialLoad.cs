using UnityEngine;
using WorldGenerator;

public class FreezeUntilInitialLoad : MonoBehaviour
{
    public TerrainGenerator terrainGenerator;
    private RigidbodyConstraints initialConstraints;
    private Rigidbody rb;

    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialConstraints = rb.constraints;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        terrainGenerator.OnInitialTerrainLoaded += TerrainGenerator_OnInitialTerrainLoaded;
        Debug.Log("Loading terrain...");
    }

    private void TerrainGenerator_OnInitialTerrainLoaded()
    {
        Debug.Log("Terrain loaded");
        rb.constraints = initialConstraints;
        terrainGenerator.OnInitialTerrainLoaded -= TerrainGenerator_OnInitialTerrainLoaded;
    }
}
