﻿using UnityEngine;

namespace WorldGenerator
{
    public class MeshData
    {
        private Vector3[] vertices;
        private int[] triangles;
        private Vector2[] uvs;
        private Vector3[] bakedNormals;

        private readonly Vector3[] outOfMeshVertices;
        private int[] outOfMeshTriangles;

        private int triangleIndex;
        private int outOfMeshTriangleIndex;

        private readonly bool useFlatShading;

        public MeshData(int numVertsPerLine, int skipIncrement, bool useFlatShading)
        {
            this.useFlatShading = useFlatShading;

            int numMeshEdgeVertices = (numVertsPerLine - 2) * 4 - 4;
            int numEdgeConnectionVertices = (skipIncrement - 1) * (numVertsPerLine - 5) / skipIncrement * 4;
            int numMainVerticesPerLine = (numVertsPerLine - 5) / skipIncrement + 1;
            int numMainVertices = numMainVerticesPerLine * numMainVerticesPerLine;

            vertices = new Vector3[numMeshEdgeVertices + numEdgeConnectionVertices + numMainVertices];
            uvs = new Vector2[vertices.Length];

            int numMeshEdgeTriangles = (numVertsPerLine - 4) * 8;
            int numMainTriangles = (numMainVerticesPerLine - 1) * (numMainVerticesPerLine - 1) * 2;
            triangles = new int[(numMeshEdgeTriangles + numMainTriangles) * 3];

            outOfMeshVertices = new Vector3[numVertsPerLine * 4 - 4];
            outOfMeshTriangles = new int[(numVertsPerLine - 2) * 24];
        }

        public Mesh CreateMesh()
        {
            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uvs;
            if (useFlatShading)
            {
                mesh.RecalculateNormals();
            }
            else
            {
                mesh.normals = bakedNormals;
            }
            return mesh;
        }

        public void AddVertex(Vector3 vertexPosition, Vector2 uv, int vertexIndex)
        {
            if (vertexIndex < 0)
            {
                outOfMeshVertices[-vertexIndex - 1] = vertexPosition;
            }
            else
            {
                vertices[vertexIndex] = vertexPosition;
                uvs[vertexIndex] = uv;
            }
        }

        public void AddTriangle(int a, int b, int c)
        {
            if (a < 0 || b < 0 || c < 0)
            {
                outOfMeshTriangles[outOfMeshTriangleIndex] = a;
                outOfMeshTriangles[outOfMeshTriangleIndex + 1] = b;
                outOfMeshTriangles[outOfMeshTriangleIndex + 2] = c;
                outOfMeshTriangleIndex += 3;
            }
            else
            {
                triangles[triangleIndex] = a;
                triangles[triangleIndex + 1] = b;
                triangles[triangleIndex + 2] = c;
                triangleIndex += 3;
            }
        }

        private Vector3[] CalculateNormals()
        {
            Vector3[] vertexNormals = new Vector3[vertices.Length];
            int triangleCount = triangles.Length / 3;
            for (int i = 0; i < triangleCount; i++)
            {
                int normalTriangleIndex = i * 3;
                int vertexIndexA = triangles[normalTriangleIndex];
                int vertexIndexB = triangles[normalTriangleIndex + 1];
                int vertexIndexC = triangles[normalTriangleIndex + 2];

                Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
                vertexNormals[vertexIndexA] += triangleNormal;
                vertexNormals[vertexIndexB] += triangleNormal;
                vertexNormals[vertexIndexC] += triangleNormal;
            }

            int borderTriangleCount = outOfMeshTriangles.Length / 3;
            for (int i = 0; i < borderTriangleCount; i++)
            {
                int normalTriangleIndex = i * 3;
                int vertexIndexA = outOfMeshTriangles[normalTriangleIndex];
                int vertexIndexB = outOfMeshTriangles[normalTriangleIndex + 1];
                int vertexIndexC = outOfMeshTriangles[normalTriangleIndex + 2];

                Vector3 triangleNormal = SurfaceNormalFromIndices(vertexIndexA, vertexIndexB, vertexIndexC);
                if (vertexIndexA >= 0)
                {
                    vertexNormals[vertexIndexA] += triangleNormal;
                }
                if (vertexIndexB >= 0)
                {
                    vertexNormals[vertexIndexB] += triangleNormal;
                }
                if (vertexIndexC >= 0)
                {
                    vertexNormals[vertexIndexC] += triangleNormal;
                }
            }

            for (int i = 0; i < vertexNormals.Length; i++)
            {
                vertexNormals[i].Normalize();
            }
            return vertexNormals;
        }

        private Vector3 SurfaceNormalFromIndices(int indexA, int indexB, int indexC)
        {
            Vector3 pointA = indexA < 0 ? outOfMeshVertices[-indexA - 1] : vertices[indexA];
            Vector3 pointB = indexB < 0 ? outOfMeshVertices[-indexB - 1] : vertices[indexB];
            Vector3 pointC = indexC < 0 ? outOfMeshVertices[-indexC - 1] : vertices[indexC];

            Vector3 sideAB = pointB - pointA;
            Vector3 sideAC = pointC - pointA;
            return Vector3.Cross(sideAB, sideAC).normalized;

        }

        public void FinalizeMesh()
        {
            if (useFlatShading)
            {
                FlatShading();
            }
            else
            {
                BakeNormals();
            }
        }

        private void BakeNormals()
        {
            bakedNormals = CalculateNormals();
        }

        private void FlatShading()
        {
            Vector3[] flatShadedVertices = new Vector3[triangles.Length];
            Vector2[] flatShadedUVs = new Vector2[triangles.Length];

            for (int i = 0; i < triangles.Length; i++)
            {
                flatShadedVertices[i] = vertices[triangles[i]];
                flatShadedUVs[i] = uvs[triangles[i]];
                triangles[i] = i;
            }

            vertices = flatShadedVertices;
            uvs = flatShadedUVs;
        }
    }
}