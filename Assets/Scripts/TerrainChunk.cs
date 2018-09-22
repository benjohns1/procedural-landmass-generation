using UnityEngine;

public class TerrainChunk
{
    const float colliderGenerationDistanceThreshold = 20f;
    const float sqrColliderGenerationDistanceThreshold = colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold;

    public event System.Action<TerrainChunk, bool> onVisibilityChanged;
    public Vector2 coord;

    public bool hasSetCollider { get; private set; }

    private GameObject meshObject;
    private Vector2 sampleCenter;
    private Bounds bounds;

    private MeshRenderer meshRenderer;
    private MeshFilter meshFilter;
    private MeshCollider meshCollider;

    private LODInfo[] detailLevels;
    private readonly LODMesh[] lodMeshes;
    private readonly int colliderLODIndex;

    private HeightMap mapData;
    private bool heightMapReceived;
    private int previousLODIndex = -1;
    private readonly float maxViewDst;

    HeightMapSettings heightMapSettings;
    MeshSettings meshSettings;
    Transform viewer;

    public TerrainChunk(Vector2 coord, HeightMapSettings heightMapSettings, MeshSettings meshSettings, LODInfo[] detailLevels, int colliderLODIndex, Transform parent, Transform viewer, Material material)
    {
        this.coord = coord;
        this.detailLevels = detailLevels;
        this.colliderLODIndex = colliderLODIndex;
        this.heightMapSettings = heightMapSettings;
        this.meshSettings = meshSettings;
        this.viewer = viewer;

        sampleCenter = coord * meshSettings.meshWorldSize / meshSettings.meshScale;
        Vector2 position = coord * meshSettings.meshWorldSize;
        bounds = new Bounds(position, Vector2.one * meshSettings.meshWorldSize);

        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
        SetVisible(false);

        lodMeshes = new LODMesh[detailLevels.Length];
        for (int i = 0; i < detailLevels.Length; i++)
        {
            lodMeshes[i] = new LODMesh(detailLevels[i].lod);
            lodMeshes[i].updateCallback += UpdateTerrainChunk;
            if (i == colliderLODIndex)
            {
                lodMeshes[i].updateCallback += UpdateCollisionMesh;
            }
        }

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
    }

    public void Load()
    {
        ThreadedDataRequester.RequestData(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, sampleCenter), OnHeightMapReceived);
    }

    private void OnHeightMapReceived(object data)
    {
        HeightMap mapData = (HeightMap)data;
        this.mapData = mapData;
        heightMapReceived = true;

        UpdateTerrainChunk();
    }

    private Vector2 viewerPosition
    {
        get
        {
            return new Vector2(viewer.position.x, viewer.position.z);
        }
    }

    private void OnMeshDataReceived(MeshData meshData)
    {
        meshFilter.mesh = meshData.CreateMesh();
    }

    public void UpdateTerrainChunk()
    {
        if (!heightMapReceived)
        {
            return;
        }


        float viewerDstFromNearestEdge = Mathf.Sqrt(bounds.SqrDistance(viewerPosition));
        bool wasVisible = IsVisible();
        bool visible = viewerDstFromNearestEdge <= maxViewDst;
        if (visible)
        {
            int lodIndex = 0;
            for (int i = 0; i < detailLevels.Length - 1; i++)
            {
                if (viewerDstFromNearestEdge > detailLevels[i].visibleDstThreshold)
                {
                    lodIndex = i + 1;
                }
                else
                {
                    break;
                }
            }

            if (lodIndex != previousLODIndex)
            {
                LODMesh lodMesh = lodMeshes[lodIndex];
                if (lodMesh.hasMesh)
                {
                    meshFilter.mesh = lodMesh.mesh;
                    previousLODIndex = lodIndex;
                }
                else if (!lodMesh.hasRequestedMesh)
                {
                    lodMesh.RequestMesh(mapData, meshSettings);
                }
            }
        }

        if (wasVisible != visible)
        {
            SetVisible(visible);
            if (onVisibilityChanged != null)
            {
                onVisibilityChanged(this, visible);
            }
        }
    }

    public void UpdateCollisionMesh()
    {
        if (hasSetCollider)
        {
            return;
        }

        float sqrDstFromViewerToEdge = bounds.SqrDistance(viewerPosition);

        if (sqrDstFromViewerToEdge < detailLevels[colliderLODIndex].sqrVisibleDstThreshold)
        {
            if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
            {
                Debug.Log("collider requested for " + coord);
                lodMeshes[colliderLODIndex].RequestMesh(mapData, meshSettings);
            }
        }

        if (sqrDstFromViewerToEdge < sqrColliderGenerationDistanceThreshold)
        {
            if (lodMeshes[colliderLODIndex].hasMesh)
            {
                Debug.Log("collider set for " + coord);
                meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
                hasSetCollider = true;
            }
        }
    }

    public void SetVisible(bool visible)
    {
        meshObject.SetActive(visible);
    }

    public bool IsVisible()
    {
        return meshObject.activeSelf;
    }

}