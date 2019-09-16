using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointCalculatorCircle
{
    public SegmentPoints CalculatePoints(Transform _transform, int _segmentCount, float _radius, float _width)
    {
        List<Vector3> outside = new List<Vector3>();
        List<Vector3> inside = new List<Vector3>();

        float vStep = (2f * Mathf.PI) / _segmentCount;
        Quaternion rotation = Quaternion.Euler(_transform.rotation.x, _transform.rotation.y, _transform.rotation.z);

        for (int v = 0; v < _segmentCount; v++)
        {

            var pointOnCircle = GetPointOnTorus(0, v * vStep, _radius);
            var point = rotation * _transform.TransformPoint(pointOnCircle);
            outside.Add(point);

            var pointOnCircleInside = GetPointOnTorus(0, v * vStep, _width);
            var pointInside = rotation * _transform.TransformPoint(pointOnCircleInside);
            inside.Add(pointInside);
        }

        SegmentPoints points = new SegmentPoints(outside, inside);

        return points;
    }

    private Vector3 GetPointOnTorus(float u, float v, float pipeRadius)
    {
        Vector3 p;
        float r = (pipeRadius * Mathf.Cos(v));
        p.x = pipeRadius * Mathf.Sin(v);
        p.y = r;
        p.z = u;
        return p;
    }
}
