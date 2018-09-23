using System.Collections.Generic;
using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    const float viewerMoveThresholdForChunkUpdate = 5f;
    const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

    public event System.Action OnInitialTerrainLoaded;

    public int colliderLODIndex;
    public LODInfo[] detailLevels;

    public MeshSettings meshSettings;
    public HeightMapSettings heightMapSettings;
    public TextureSettings textureSettings;

    public Transform viewer;
    public Material terrainMaterial;

    private Vector2 viewerPos;
    private Vector2 viewerPosPrevChunkCheck;
    private bool addedFirstCollider;

    private float meshWorldSize;
    private int chunksVisibleInViewDst;

    private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
    private List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

    private int initialChunkLoadCount;

    private void Start()
    {
        textureSettings.ApplyToMaterial(terrainMaterial);
        textureSettings.UpdateMeshHeights(terrainMaterial, heightMapSettings.minHeight, heightMapSettings.maxHeight);

        float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        meshWorldSize = meshSettings.meshWorldSize;
        chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshWorldSize);

        initialChunkLoadCount = 0;
        UpdateVisibleChunks(true);
        CheckIfInitialLoadComplete();
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
                    TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, heightMapSettings, meshSettings, detailLevels, colliderLODIndex, transform, viewer, terrainMaterial);
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
