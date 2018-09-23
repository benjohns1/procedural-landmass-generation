using UnityEngine;

public class FreezeUntilInitialLoad : MonoBehaviour
{
    private TerrainGenerator terrainGenerator;
    private RigidbodyConstraints initialConstraints;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        initialConstraints = rb.constraints;
        rb.constraints = RigidbodyConstraints.FreezeAll;
        terrainGenerator = Object.FindObjectOfType<TerrainGenerator>();
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
