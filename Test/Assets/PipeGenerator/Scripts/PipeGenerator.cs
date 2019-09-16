using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(BezierSpline))]
public class PipeGenerator : MonoBehaviour
{
    private BezierSpline spline;
    private Mesh mesh;
    private MeshRenderer renderer;
    private GameObject segment;
    private PointCalculatorCircle calculator;

    private Vector3[] vertices;
    private int[] triangles;
    private Vector2[] uv;

    [SerializeField]
    private int frequency;
    [SerializeField]
    private int pipeSegmentCount;
    [SerializeField]
    private float pipeRadius;
    [SerializeField]
    private float pipeWidth;
    [SerializeField]
    private int vMult;

    public Texture Texture;


    public List<SegmentPoints> Points = new List<SegmentPoints>();


    private void Awake()
    {
        spline = this.GetComponent<BezierSpline>();
        renderer = this.GetComponent<MeshRenderer>();
        mesh = this.GetComponent<MeshFilter>().mesh;
        calculator = new PointCalculatorCircle();

        mesh.name = "Pipe";

        GenerateMaterial();
        GeneratePipe();
    }

    private void GenerateMaterial()
    {

        Material tubeMaterial = new Material(Shader.Find("Standard"));
        tubeMaterial.SetTexture("_MainTex", Texture);

        renderer.material = tubeMaterial;
    }

    private void GeneratePipe()
    {
        mesh.Clear();
        vertices = new Vector3[pipeSegmentCount * frequency * 8];
        triangles = new int[pipeSegmentCount * frequency * 24];
        uv = new Vector2[vertices.Length];

        segment = new GameObject("Segment");
        segment.transform.SetParent(this.transform);

        float stepSize = 1f / (frequency);
        for (int p = 0, f = 0; f < frequency; f++, p++)
        {
            Vector3 position = spline.GetPoint(p * stepSize);
            segment.transform.position = position;
            segment.transform.LookAt(position + spline.GetDirection(p * stepSize));

            Points.Add(calculator.CalculatePoints(segment.transform, pipeSegmentCount, pipeRadius, pipeWidth));
        }

        SetVertices();
        SetTriangles();
        SetUV();
        mesh.RecalculateNormals();
        Destroy(segment);
    }

    private void SetVertices()
    {
        int iDelta = pipeSegmentCount * 4;
        for (int u = 1, i = iDelta; u <= Points.Count - 1; u++, i += iDelta)
        {
            SegmentPoints First = Points[u];
            SegmentPoints Second = Points[u - 1];

            List<Vector3> dotsFirst = First.SegmentPointsOutside;
            List<Vector3> dotsSecond = Second.SegmentPointsOutside;
            List<Vector3> dotsFirstInside = First.SegmentPointsInside;
            List<Vector3> dotsSecondInside = Second.SegmentPointsInside;

            Vector3 vertexA =  dotsFirst[dotsFirst.Count - 1];
            Vector3 vertexB = dotsSecond[dotsSecond.Count - 1];
            Vector3 vertexAInside = dotsFirstInside[dotsFirstInside.Count - 1];
            Vector3 vertexBInside = dotsSecondInside[dotsSecondInside.Count - 1];

            // Caps
            if (u == 1)
            {
                for (int v = 0, t = 0; v < pipeSegmentCount; v++, t += 4)
                {
                    vertices[t] = vertexB;
                    vertices[t + 1] = vertexB = dotsSecond[v];
                    vertices[t + 2] = vertexBInside;
                    vertices[t + 3] = vertexBInside = dotsSecondInside[v];
                }
            }

            if (u == Points.Count - 1)
            {
                for (int v = 0, t = 0; v < pipeSegmentCount; v++, t += 4)
                {
                    var innerTubeStart = vertices.Length / 2 + t;
                    vertices[innerTubeStart] = vertexAInside;
                    vertices[innerTubeStart + 1] = vertexAInside = dotsFirstInside[v];
                    vertices[innerTubeStart + 2] = vertexA;
                    vertices[innerTubeStart + 3] = vertexA = dotsFirst[v];
                }
            }

            // Pipe
            for (int v = 0, t = i; v < pipeSegmentCount; v++, t += 4)
            {
                vertices[t] = vertexA;
                vertices[t + 1] = vertexA = dotsFirst[v];
                vertices[t + 2] = vertexB;
                vertices[t + 3] = vertexB = dotsSecond[v];

                var innerTubeStart = vertices.Length / 2 + t;
                vertices[innerTubeStart] = vertexBInside;
                vertices[innerTubeStart + 1] = vertexBInside = dotsSecondInside[v];
                vertices[innerTubeStart + 2] = vertexAInside;
                vertices[innerTubeStart + 3] = vertexAInside = dotsFirstInside[v];
            }
        }
        mesh.vertices = vertices;
    }

    private void SetTriangles()
    {
        for (int t = 0, i = 0; t < triangles.Length / 2; t += 6, i += 4)
        {
            triangles[t] = i;
            triangles[t + 1] = triangles[t + 4] = i + 1;
            triangles[t + 2] = triangles[t + 3] = i + 2;
            triangles[t + 5] = i + 3;

            var innerTubeStart = triangles.Length / 2 + t;
            triangles[innerTubeStart] = i;
            triangles[innerTubeStart + 1] = triangles[innerTubeStart + 4] = i + 1;
            triangles[innerTubeStart + 2] = triangles[innerTubeStart + 3] = i + 2;
            triangles[innerTubeStart + 5] = i + 3;
        }

        mesh.triangles = triangles;
    }

    private void SetUV()
    {
        // uv.x
        for (int i = 0; i < vertices.Length;)
        {
            for (int n = 0; n < pipeSegmentCount; n++)
            {
                uv[i].x = (float)(pipeSegmentCount - (pipeSegmentCount - n)) / pipeSegmentCount;
                uv[i + 1].x = (float)1 / (pipeSegmentCount) * (n + 1);
                uv[i + 2].x = (float)(pipeSegmentCount - (pipeSegmentCount - n)) / pipeSegmentCount;
                uv[i + 3].x = (float)1 / (pipeSegmentCount) * (n + 1);

                i += 4;
            }
        }

        // uv.y
        for (int i = 0; i < vertices.Length; i += 4)
        {
            if (uv[i].y == 0)
            {
                uv[i].y = (float)1 / (vMult);
                uv[i + 1].y = (float)1 / (vMult);
                uv[i + 2].y = 0;
                uv[i + 3].y = 0;

                if (vMult > 1)
                {
                    for (int n = 1; n < vMult; n++)
                    {
                        var offset = 4 * pipeSegmentCount * n;
                        if (i + 3 + offset < vertices.Length)
                        {
                            uv[i + offset].y = (float)1 / (vMult) * (n + 1);
                            uv[i + 1 + offset].y = (float)1 / (vMult) * (n + 1);
                            uv[i + 2 + offset].y = (float)1 / (vMult) * (n);
                            uv[i + 3 + offset].y = (float)1 / (vMult) * (n);
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }

        mesh.uv = uv;
    }
}
