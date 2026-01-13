using Road;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(RoadSegmentRenderer))]
public class RoadSegmentRendererEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        RoadSegmentRenderer r = (RoadSegmentRenderer)target;

        GUILayout.Space(10);

        if (GUILayout.Button("Regenerate Road Segment"))
        {
            r.Generate();
        }

        if (r.roadSegment != null)
        {
            GUILayout.Label($"Segment Length: {r.GetLength():F2} m");
        }
    }

    private void OnSceneGUI()
    {
        if (GUI.changed)
        {
            ((RoadSegmentRenderer)target).Generate();
        }
    }
}