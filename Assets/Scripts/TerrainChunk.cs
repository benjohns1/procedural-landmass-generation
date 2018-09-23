using UnityEngine;
using ThreadedJobSystem;

public class TerrainChunk
{
    const float colliderGenerationDistanceThreshold = 200f;
    const float sqrColliderGenerationDistanceThreshold = colliderGenerationDistanceThreshold * colliderGenerationDistanceThreshold;

    public event System.Action<TerrainChunk> OnLodMeshUpdated;
    public event System.Action<TerrainChunk, bool> OnVisibilityChanged;
    public Vector2 coord;

    public bool hasSetCollider { get; private set; }

    private GameObject meshObject;
    private Vector2 sampleCenter;

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
    private readonly float sqrMaxViewDst;

    private bool initiallyLoaded;

    private readonly HeightMapSettings heightMapSettings;
    private readonly MeshSettings meshSettings;
    private readonly Transform viewer;

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

        meshObject = new GameObject("Terrain Chunk");
        meshRenderer = meshObject.AddComponent<MeshRenderer>();
        meshRenderer.material = material;
        meshFilter = meshObject.AddComponent<MeshFilter>();
        meshCollider = meshObject.AddComponent<MeshCollider>();

        meshObject.transform.position = new Vector3(position.x, 0, position.y);
        meshObject.transform.parent = parent;
        SetVisible(false);

        lodMeshes = new LODMesh[detailLevels.Length];
        for (int lodIndex = 0; lodIndex < detailLevels.Length; lodIndex++)
        {
            lodMeshes[lodIndex] = new LODMesh(lodIndex, detailLevels[lodIndex].lod);
            lodMeshes[lodIndex].OnMeshDataUpdated += SetLodMesh;
            if (lodIndex == colliderLODIndex)
            {
                lodMeshes[lodIndex].OnMeshDataUpdated += SetCollisionMesh;
            }
        }

        maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
        sqrMaxViewDst = (maxViewDst * maxViewDst);
    }

    public void Load()
    {
        JobQueue.Run(() => HeightMapGenerator.GenerateHeightMap(meshSettings.numVertsPerLine, meshSettings.numVertsPerLine, heightMapSettings, sampleCenter), this.OnHeightMapReceived);
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

    public void UpdateTerrainChunk()
    {
        if (!heightMapReceived)
        {
            return;
        }

        float sqrViewerDstFromCenter = (viewerPosition - sampleCenter).sqrMagnitude;
        bool wasVisible = IsVisible();
        bool visible = sqrViewerDstFromCenter <= sqrMaxViewDst;
        if (visible)
        {
            int lodIndex = 0;
            for (int i = 0; i < detailLevels.Length - 1; i++)
            {
                if (detailLevels[i].sqrVisibleDstThreshold > sqrViewerDstFromCenter)
                {
                    break;
                }
                lodIndex = i + 1;
            }

            if (!lodMeshes[lodIndex].hasRequestedMesh)
            {
                lodMeshes[lodIndex].RequestMesh(mapData, meshSettings);
            }
            else
            {
                SetLodMesh(lodIndex);
            }
        }
        else if (!initiallyLoaded)
        {
            initiallyLoaded = true;
            if (OnLodMeshUpdated != null)
            {
                OnLodMeshUpdated(this);
            }
        }

        if (wasVisible != visible)
        {
            SetVisible(visible);
            if (OnVisibilityChanged != null)
            {
                OnVisibilityChanged(this, visible);
            }
        }
    }

    public void UpdateCollisionMesh()
    {
        if (hasSetCollider)
        {
            return;
        }

        float sqrViewerDstFromCenter = (viewerPosition - sampleCenter).sqrMagnitude;
        if (sqrViewerDstFromCenter > sqrColliderGenerationDistanceThreshold)
        {
            return;
        }

        if (!lodMeshes[colliderLODIndex].hasRequestedMesh)
        {
            lodMeshes[colliderLODIndex].RequestMesh(mapData, meshSettings);
        }
        else
        {
            SetCollisionMesh(colliderLODIndex);
        }
    }

    private void SetLodMesh(int lodIndex)
    {
        if (lodIndex == previousLODIndex)
        {
            return;
        }

        LODMesh lodMesh = lodMeshes[lodIndex];
        if (!lodMesh.hasMesh)
        {
            return;
        }

        meshFilter.mesh = lodMesh.mesh;
        previousLODIndex = lodIndex;
        if (OnLodMeshUpdated != null)
        {
            OnLodMeshUpdated(this);
        }
    }

    private void SetCollisionMesh(int lod)
    {
        if (lod != colliderLODIndex)
        {
            return;
        }

        if (!lodMeshes[colliderLODIndex].hasMesh)
        {
            return;
        }

        float sqrViewerDstFromCenter = (viewerPosition - sampleCenter).sqrMagnitude;
        if (sqrViewerDstFromCenter > sqrColliderGenerationDistanceThreshold)
        {
            return;
        }

        meshCollider.sharedMesh = lodMeshes[colliderLODIndex].mesh;
        hasSetCollider = true;
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