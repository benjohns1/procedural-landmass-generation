using UnityEngine;
using ThreadedJobSystem;

public class LODMesh
{
    public Mesh mesh;
    public bool hasRequestedMesh;
    public bool hasMesh;
    private readonly int lod;
    private readonly int lodIndex;
    public event System.Action<int> OnMeshDataUpdated;

    public LODMesh(int lodIndex, int lod)
    {
        this.lodIndex = lodIndex;
        this.lod = lod;
    }

    public void RequestMesh(HeightMap heightMap, MeshSettings meshSettings)
    {
        hasRequestedMesh = true;
        JobQueue.Run(() => MeshGenerator.GenerateTerrainMesh(heightMap.values, meshSettings, lod), this.OnMeshDataReceived);
    }

    private void OnMeshDataReceived(object data)
    {
        mesh = ((MeshData)data).CreateMesh();
        hasMesh = true;
        OnMeshDataUpdated(lodIndex);
    }
}