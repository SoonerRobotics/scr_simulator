using UnityEngine;

public enum LaneType
{
    Solid,
    Dotted
}

[CreateAssetMenu(menuName = "IGVC/Road Segment")]
public class RoadSegment : ScriptableObject
{
    [Header("Road Geometry")]
    public float roadWidth = 4.0f;
    public bool isOneWay = false;
    public bool isClosedLoop = false;
    public int roundingResolution = 16;

    [Header("Outer Lane")]
    public Color outerLaneColor = Color.white;
    public LaneType outerLaneType = LaneType.Solid;
    public float outerLaneWidth = 0.15f;

    [Header("Inner Lane")]
    public Color innerLaneColor = Color.yellow;
    public LaneType innerLaneType = LaneType.Dotted;
    public float innerLaneWidth = 0.15f;
}