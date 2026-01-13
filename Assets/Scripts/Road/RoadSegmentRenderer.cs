using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Splines;

namespace Road
{
    [ExecuteAlways]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
    public class RoadSegmentRenderer : MonoBehaviour
    {
        [Header("Components")] public RoadSegment roadSegment;
        public SplineContainer splineContainer;

        [Header("Materials")] public Material roadBaseMaterial;
        public Material solidLaneMaterial;
        public Material dottedLaneMaterial;

        [Header("Vertical Offsets")] public float roadYOffset = 0f;
        public float laneYOffset = 0.01f;

        [Header("Dotted Lane Geometry")]
        public float dashLength = 0.5f; // meters
        public float gapLength = 0.5f;  // meters
        public float dashScale = 1.0f;

        public void Generate()
        {
            ClearChildren();

            if (roadSegment == null || splineContainer == null)
                return;

            GenerateRoadBase();
            GenerateOuterLanes();

            if (!roadSegment.isOneWay)
                GenerateInnerLane();
        }

        private void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                if (Application.isEditor)
                    DestroyImmediate(transform.GetChild(i).gameObject);
                else
                    Destroy(transform.GetChild(i).gameObject);
            }
        }

        private void GenerateOuterLanes()
        {
            float half = roadSegment.roadWidth * 0.5f;

            CreateLane(half, roadSegment.outerLaneWidth, roadSegment.outerLaneColor, roadSegment.outerLaneType,
                "Outer_Right");
            CreateLane(-half, roadSegment.outerLaneWidth, roadSegment.outerLaneColor, roadSegment.outerLaneType,
                "Outer_Left");
        }

        private void GenerateInnerLane()
        {
            CreateLane(0f, roadSegment.innerLaneWidth, roadSegment.innerLaneColor, roadSegment.innerLaneType, "Inner");
        }

        private void GenerateRoadBase()
        {
            CreateStrip(
                offset: 0f,
                width: roadSegment.roadWidth,
                material: roadBaseMaterial,
                name: "RoadBase",
                yOffset: roadYOffset
            );
        }

        private void CreateLane(
            float offset,
            float width,
            Color color,
            LaneType type,
            string name
        )
        {
            if (type == LaneType.Solid)
            {
                Material mat = Instantiate(solidLaneMaterial);
                mat.color = color;

                CreateStrip(offset, width, mat, name, laneYOffset);
            }
            else
            {
                GenerateDottedLane(offset, width, color, name);
            }
        }

        private void GenerateDottedLane(
            float offset,
            float width,
            Color color,
            string name
        )
        {
            float totalLength = GetLength();
            float step = dashLength + gapLength;

            Material mat = Instantiate(dottedLaneMaterial);
            mat.color = color;

            int dashIndex = 0;

            for (float s = 0; s < totalLength; s += step)
            {
                float dashStart = s;
                float dashEnd = Mathf.Min(s + dashLength, totalLength);

                CreateDashSegment(
                    dashStart,
                    dashEnd,
                    offset,
                    width,
                    mat,
                    $"{name}_Dash_{dashIndex++}"
                );
            }
        }

        private void CreateDashSegment(
            float startDist,
            float endDist,
            float offset,
            float width,
            Material material,
            string name
        )
        {
            GameObject go = new GameObject(name);
            go.transform.parent = transform;

            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = material;

            mf.sharedMesh = BuildDashMesh(startDist, endDist, offset, width);
        }

        private Mesh BuildDashMesh(
            float startDist,
            float endDist,
            float offset,
            float width
        )
        {
            Mesh mesh = new Mesh();
            mesh.name = "DashMesh";

            List<Vector3> verts = new();
            List<int> tris = new();

            int samples = 4; // increase for sharper curves
            float length = endDist - startDist;

            for (int i = 0; i <= samples; i++)
            {
                float d = Mathf.Lerp(startDist, endDist, i / (float)samples);
                float t = DistanceToSplineT(d);

                Vector3 pos = splineContainer.Spline.EvaluatePosition(t);
                Vector3 tangent = splineContainer.Spline.EvaluateTangent(t);
                Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

                Vector3 center = pos + right * offset + Vector3.up * laneYOffset;

                verts.Add(center + right * (width * 0.5f));
                verts.Add(center - right * (width * 0.5f));

                if (i < samples)
                {
                    int idx = i * 2;
                    tris.Add(idx);
                    tris.Add(idx + 1);
                    tris.Add(idx + 2);

                    tris.Add(idx + 2);
                    tris.Add(idx + 1);
                    tris.Add(idx + 3);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        private float DistanceToSplineT(float targetDistance)
        {
            var spline = splineContainer.Spline;
            int steps = spline.Count * roadSegment.roundingResolution;

            float accumulated = 0f;
            Vector3 prev = spline.EvaluatePosition(0f);

            for (int i = 1; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector3 p = spline.EvaluatePosition(t);
                float seg = Vector3.Distance(prev, p);

                if (accumulated + seg >= targetDistance)
                    return Mathf.Lerp(
                        (i - 1) / (float)steps,
                        t,
                        (targetDistance - accumulated) / seg
                    );

                accumulated += seg;
                prev = p;
            }

            return 1f;
        }


        private void CreateStrip(
            float offset,
            float width,
            Material material,
            string name,
            float yOffset
        )
        {
            GameObject go = new GameObject(name);
            go.transform.parent = transform;

            MeshFilter mf = go.AddComponent<MeshFilter>();
            MeshRenderer mr = go.AddComponent<MeshRenderer>();
            mr.sharedMaterial = material;

            mf.sharedMesh = BuildStripMesh(offset, width, yOffset);
        }

        private Mesh BuildStripMesh(float offset, float width, float yOffset)
        {
            Mesh mesh = new Mesh();
            mesh.name = "RoadStrip";

            var spline = splineContainer.Spline;
            int steps = spline.Count * roadSegment.roundingResolution;

            List<Vector3> verts = new();
            List<Vector2> uvs = new();
            List<int> tris = new();

            float length = 0f;
            Vector3 prev = spline.EvaluatePosition(0f);

            for (int i = 0; i <= steps; i++)
            {
                float t = i / (float)steps;

                Vector3 pos = spline.EvaluatePosition(t);
                Vector3 tangent = spline.EvaluateTangent(t);

                Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;

                if (i > 0)
                    length += Vector3.Distance(prev, pos);
                prev = pos;

                Vector3 center = pos + right * offset + Vector3.up * yOffset;

                verts.Add(center + right * (width * 0.5f));
                verts.Add(center - right * (width * 0.5f));

                uvs.Add(new Vector2(length, 1));
                uvs.Add(new Vector2(length, 0));

                if (i < steps)
                {
                    int idx = i * 2;

                    // ✅ CORRECT WINDING (FIX)
                    tris.Add(idx);
                    tris.Add(idx + 1);
                    tris.Add(idx + 2);

                    tris.Add(idx + 2);
                    tris.Add(idx + 1);
                    tris.Add(idx + 3);
                }
            }

            mesh.SetVertices(verts);
            mesh.SetUVs(0, uvs);
            mesh.SetTriangles(tris, 0);
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            return mesh;
        }

        public float GetLength(int samplesPerKnot = 20)
        {
            if (splineContainer == null)
                return 0f;

            var spline = splineContainer.Spline;
            int steps = spline.Count * samplesPerKnot;

            float length = 0f;
            Vector3 prev = spline.EvaluatePosition(0f);

            for (int i = 1; i <= steps; i++)
            {
                float t = i / (float)steps;
                Vector3 p = spline.EvaluatePosition(t);
                length += Vector3.Distance(prev, p);
                prev = p;
            }

            if (roadSegment.isClosedLoop)
                length += Vector3.Distance(prev, spline.EvaluatePosition(0f));

            return length;
        }
    }
}