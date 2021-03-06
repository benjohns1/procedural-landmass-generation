﻿using System.Collections.Generic;
using UnityEngine;

namespace WorldGenerator
{
    public class TerrainGenerator : MonoBehaviour
    {
        const string chunkParentName = "Terrain Chunks";
        const string chunkParentNameEditor = "Terrain Chunks (editor)";
        const float viewerMoveThresholdForChunkUpdate = 5f;
        const float sqrViewerMoveThresholdForChunkUpdate = viewerMoveThresholdForChunkUpdate * viewerMoveThresholdForChunkUpdate;

        public event System.Action OnInitialTerrainLoaded;

        public int colliderLODIndex;
        public LODInfo[] detailLevels;

        public WorldSettings worldSettings;
        public Transform viewer;

        private Transform chunkParent;

        private Vector2 viewerPos;
        private Vector2 viewerPosPrevChunkCheck;
        private bool addedFirstCollider;

        private float meshSizeInWorld;
        private int chunksVisibleInViewDst;

        private Dictionary<Vector2, TerrainChunk> terrainChunkDictionary = new Dictionary<Vector2, TerrainChunk>();
        private List<TerrainChunk> visibleTerrainChunks = new List<TerrainChunk>();

        private int initialChunkLoadCount;

#if UNITY_EDITOR
        [HideInInspector]
        public bool worldFoldout;
#endif

        public void GenerateTerrain(bool initialLoad = false)
        {
            ClearTerrainChunks();

            worldSettings.Initialize();

            float maxViewDst = detailLevels[detailLevels.Length - 1].visibleDstThreshold;
            meshSizeInWorld = worldSettings.meshSettings.meshSizeInWorld;
            chunksVisibleInViewDst = Mathf.RoundToInt(maxViewDst / meshSizeInWorld);

            if (chunkParent == null)
            {
                chunkParent = new GameObject(Application.isPlaying ? chunkParentName : chunkParentNameEditor).transform;
                chunkParent.parent = transform;
            }

            initialChunkLoadCount = 0;
            UpdateVisibleChunks(initialLoad);
            CheckIfInitialLoadComplete();
        }

        public void ClearTerrainChunks()
        {
            foreach (TerrainChunk chunk in terrainChunkDictionary.Values)
            {
                chunk.DestroyGameObject();
            }
            terrainChunkDictionary.Clear();
            visibleTerrainChunks.Clear();

            if (chunkParent == null)
            {
                if (Application.isPlaying)
                {
                    chunkParent = transform.Find(chunkParentName);

                    // Deactivate any chunks generated from within editor
                    Transform editorChunkParent = transform.Find(chunkParentNameEditor);
                    if (editorChunkParent != null)
                    {
                        editorChunkParent.gameObject.SetActive(false);
                    }
                }
                else
                {
                    chunkParent = transform.Find(chunkParentNameEditor);
                }
            }

            if (chunkParent != null)
            {
                Object.DestroyImmediate(chunkParent.gameObject);
            }
        }

        private void Awake()
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

            int currentChunkCoordX = Mathf.RoundToInt(viewerPos.x / meshSizeInWorld);
            int currentChunkCoordY = Mathf.RoundToInt(viewerPos.y / meshSizeInWorld);

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
                        TerrainChunk newChunk = new TerrainChunk(viewedChunkCoord, worldSettings, detailLevels, colliderLODIndex, chunkParent.transform, viewer);
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
}