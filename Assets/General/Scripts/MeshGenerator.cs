using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshGenerator : MonoBehaviour
{
    [SerializeField] MeshRenderer meshRenderer;
    [SerializeField] MeshFilter meshFilter;
    
    Mesh mesh;

    List<Vector3> points = new List<Vector3>();
    List<int> triangles = new List<int>();
    List<Vector2> uvs = new List<Vector2>();

    public Material Material
    {
        get
        {
            return meshRenderer.material;
        }
    }

    public void Generate(int width, int height)
    {
        mesh = new Mesh();
        mesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32;

        GeneratePointsAndUVs(width, height);
        GenerateTRiangles(width, height);

        meshFilter.mesh = mesh;
        mesh.RecalculateBounds();
    }

    void GeneratePointsAndUVs(int width, int height)
    {
        float aspectRatio = (float)width / height;
        for (int h = 0; h <= height; h++)
        {
            for (int w = 0; w <= width; w++)
            {
                float wi = (float)w / width;
                float he = (float)h / height;

                Vector2 uv = new Vector2(wi, 1 - he);
                uvs.Add(uv);

                if (aspectRatio > 1)
                    he = (he - 0.5f) / aspectRatio + 0.5f;
                else
                    wi = aspectRatio * (wi - 0.5f) + 0.5f;

                Vector3 point = new Vector3(wi - 0.5f, he - 0.5f, 0);
                points.Add(point);
            }
        }

        mesh.SetVertices(points);
        mesh.SetUVs(0, uvs);
    }

    void GenerateTRiangles(int width, int height)
    {
        int TotalWidth = width + 1;
        for (int h = 0; h < height; h++)
        {
            for (int w= 0; w < width; w++)
            {
                triangles.Add(TotalWidth * h + w);
                triangles.Add(TotalWidth * (h + 1) + w);
                triangles.Add(TotalWidth * (h + 1) + (w + 1));

                triangles.Add(TotalWidth * (h + 1) + (w + 1));
                triangles.Add(TotalWidth * h + (w + 1));
                triangles.Add(TotalWidth * h + w);
            }
        }

        mesh.SetTriangles(triangles, 0);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0)), transform.TransformPoint(new Vector3(0.5f, 0.5f, 0)));
        Gizmos.DrawLine(transform.TransformPoint(new Vector3(0.5f, 0.5f, 0)), transform.TransformPoint(new Vector3(0.5f, -0.5f, 0)));
        Gizmos.DrawLine(transform.TransformPoint(new Vector3(0.5f, -0.5f, 0)), transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0)));
        Gizmos.DrawLine(transform.TransformPoint(new Vector3(-0.5f, -0.5f, 0)), transform.TransformPoint(new Vector3(-0.5f, 0.5f, 0)));
    }
}
