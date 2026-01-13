using System.Collections.Generic;
using Road;
using UnityEngine;

[CreateAssetMenu(menuName = "IGVC/Course")]
public class Course : ScriptableObject
{
    public List<RoadSegmentRenderer> roadRenderers;

    public float GetTotalLength()
    {
        float total = 0f;
        foreach (var r in roadRenderers)
            if (r != null)
                total += r.GetLength();

        return total;
    }
}