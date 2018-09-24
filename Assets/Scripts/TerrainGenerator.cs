using System.Collections.Generic;
using UnityEngine;
using NoiseGenerator;

public class TerrainGenerator : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 5f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public event System.Action OnInitialTerrainLoaded;

    public bool autoUpdate;
    public bool noiseSettingsFoldout;
    public NoiseGenerator.NoiseSettings noiseSettings;

    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureSettings textureSettings;

    public Transform viewer;
    public Material terrainMaterial;

    private GameObject chunkParent;

    private Vector2 viewerPos;
    private Vector2 viewerPosPrevChunkCheck;
    private bool addedFirstCollider;

    private float meshWorldSize;
    private int chunksVisibleInViewDst;

    private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    private int initialChunkLoadCount;

    public void GenerateTerrainEditorPreview()
    {
        ClearTerrainChunks();
        GenerateTerrain();
    }

    public void ClearTerrainChunks()
    {
        foreach (TerrainChunk chunk in terrainChunkDictionary.Values)
        {
            chunk.DestroyGameObject();
        }
        terrainChunkDictionary.Clear();
        visibleTerrainChunks.Clear();

        if (chunkParent != null)
        {
            Object.DestroyImmediate(chunkParent);
        }
    }

    public void OnNoiseSettingsUpdated()
    {
        if (!autoUpdate)
        {
            return;
        }
        GenerateTerrainEditorPreview();
    }

    private void Start()
    {
        GenerateTerrain(true);
    }

    private void Update()
    {
        viewerPos = new Vector2(viewer.position.x, viewer.position.z);

        if (viewerPos != viewerPosPrevChunkCheck || !addedFirstCollider)
        {
            UpdateVisibleChunkColliders();
        }

        if ((viewerPosPrevChunkCheck - viewerPos).sqrMagnitude > sqrViewerMoveThresholdForChunkUpdate)
        {
            viewerPosPrevChunkCheck = viewerPos;
            UpdateVisibleChunks();
        }
    }

    private void GenerateTerrain(bool initialLoad = false)
    {
        textureSettings.ApplyToMaterial(terrainMaterial);
        textureSettings.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);

        if (chunkParent == null)
        {
            chunkParent = new GameObject("Terrain Chunks");
            chunkParent.transform.parent = transform;
        }

        initialChunkLoadCount = 0;
        UpdateVisibleChunks(initialLoad);
        CheckIfInitialLoadComplete();
    }

    private void UpdateVisibleChunkColliders()
    {
        foreach (TerrainChunk chunk in visibleTerrainChunks)
        {
            chunk.UpdateCollisionMesh();
            if (!addedFirstCollider && chunk.hasSetCollider)
            {
                addedFirstCollider = true;
                CheckIfInitialLoadComplete();
            }
        }
    }

    private void UpdateVisibleChunks(bool initialLoad = false)
    {
        HashSet<Vector2> alreadyUpdatedChunkCoords = new HashSet<Vector2>();
        for (int i = visibleTerrainChunks.Count - 1; i >= 0; i--)
        {
            alreadyUpdatedChunkCoords.Add(visibleTerrainChunks[i].coord);
            visibleTerrainChunks[i].UpdateTerrainChunk();
        }

        int currentChunkCoordX = Mathf.RoundToInt(viewerPos.x / meshWorldSize);
        int currentChunkCoordY = Mathf.RoundToInt(viewerPos.y / meshWorldSize);

        for (int yOffset = -chunksVisibleInViewDst; yOffset <= chunksVisibleInViewDst; yOffset++)
        {
            for (int xOffset = -chunksVisibleInViewDst; xOffset <= chunksVisibleInViewDst; xOffset++)
            {
                Vector2 viewedChunkCoord = new Vector2(currentChunkCoordX + xOffset, currentChunkCoordY + yOffset);
                if (alreadyUpdatedChunkCoords.Contains(viewedChunkCoord))
                {
                    continue;
                }

                if (terrainChunkDictionary.ContainsKey(viewedChunkCoord))
                {
                    terrainChunkDictionary[viewedChunkCoord].UpdateTerrainChunk();
                }
                else
                {
                    TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, chunkParent.transform, viewer, terrainMaterial);
                    terrainChunkDictionary.Add(viewedChunkCoord, newChunk);
                    newChunk.OnVisibilityChanged += OnTerrainChunkVisibilityChanged;
                    if (initialLoad)
                    {
                        initialChunkLoadCount++;
                        newChunk.OnLodMeshUpdated += OnInitialMeshUpdated;
                    }
                    newChunk.Load();
                }
            }
        }
    }

    private void OnInitialMeshUpdated(TerrainChunk chunk)
    {
        chunk.OnLodMeshUpdated -= OnInitialMeshUpdated;
        initialChunkLoadCount--;
        CheckIfInitialLoadComplete();
    }

    private void CheckIfInitialLoadComplete()
    {
        if (addedFirstCollider && initialChunkLoadCount <= 0 && OnInitialTerrainLoaded != null)
        {
            OnInitialTerrainLoaded();
        }
    }

    private void OnTerrainChunkVisibilityChanged(TerrainChunk chunk, bool isVisible)
    {
        if (isVisible)
        {
            visibleTerrainChunks.Add(chunk);
        }
        else
        {
            visibleTerrainChunks.Remove(chunk);
        }
    }
}
