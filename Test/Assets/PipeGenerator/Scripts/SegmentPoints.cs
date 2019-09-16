using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SegmentPoints
{
    public readonly List<Vector3> SegmentPointsOutside;
    public readonly List<Vector3> SegmentPointsInside;

    public SegmentPoints(List<Vector3> outside, List<Vector3> inside)
    {
        SegmentPointsOutside = outside;
        SegmentPointsInside = inside;
    }
}
