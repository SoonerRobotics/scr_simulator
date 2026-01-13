using UnityEngine;
using TMPro;
using System.Collections.Generic;

[ExecuteAlways]
public class StopSignGenerator : MonoBehaviour
{
    [Header("Sign Geometry")]
    [Tooltip("Radius from center to vertex (meters)")]
    public float signRadius = 0.35f;

    [Tooltip("Thickness of the sign (meters)")]
    public float signThickness = 0.01f;

    [Tooltip("Distance from bottom of sign to ground (meters)")]
    public float signBottomHeight = 2.0f;

    [Header("Pole")]
    public float poleRadius = 0.03f;
    public float poleExtraHeight = 0.5f;

    [Header("Text")]
    public string signText = "STOP";
    public float textScale = 0.25f;
    public float textDepthOffset = 0.002f;

    [Header("Materials")]
    public Material signMaterial;
    public Material poleMaterial;
    public Material textMaterial;

    [ContextMenu("Generate Stop Sign")]
    public void Generate()
    {
        ClearChildren();

        float signCenterHeight = signBottomHeight + signRadius;

        GameObject sign = CreateOctagonSign(signCenterHeight);
        GameObject pole = CreatePole(signCenterHeight - signRadius);

        sign.transform.parent = transform;
        pole.transform.parent = transform;

        CreateText(sign.transform, signCenterHeight);
    }

    private GameObject CreateOctagonSign(float centerHeight)
    {
        GameObject go = new GameObject("StopSign");
        go.transform.localPosition = new Vector3(0, centerHeight, 0);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = signMaterial;

        mf.sharedMesh = BuildOctagonMesh(signRadius, signThickness);

        return go;
    }

    private GameObject CreatePole(float bottomOfSign)
    {
        GameObject go = new GameObject("Pole");

        float height = bottomOfSign + poleExtraHeight;
        go.transform.localPosition = new Vector3(0, height * 0.5f, 0);

        MeshFilter mf = go.AddComponent<MeshFilter>();
        MeshRenderer mr = go.AddComponent<MeshRenderer>();
        mr.sharedMaterial = poleMaterial;

        mf.sharedMesh = BuildCylinderMesh(poleRadius, height);

        return go;
    }

    private void CreateText(Transform signTransform, float centerHeight)
    {
        GameObject textObj = new GameObject("SignText");
        textObj.transform.parent = signTransform;

        textObj.transform.localPosition =
            Vector3.forward * (signThickness * 0.5f + textDepthOffset);
        textObj.transform.localRotation = Quaternion.identity;
        textObj.transform.localScale = Vector3.one * textScale;

        TextMeshPro tm = textObj.AddComponent<TextMeshPro>();
        tm.text = signText;
        tm.alignment = TextAlignmentOptions.Center;
        tm.fontSize = 10;
        tm.color = Color.white;
        tm.enableAutoSizing = true;

        if (textMaterial != null)
            tm.fontMaterial = textMaterial;
    }

    private Mesh BuildOctagonMesh(float radius, float thickness)
    {
        Mesh mesh = new Mesh();
        mesh.name = "Octagon";

        List<Vector3> verts = new();
        List<int> tris = new();

        int sides = 8;
        float halfZ = thickness * 0.5f;

        verts.Add(Vector3.forward * halfZ);
        for (int i = 0; i < sides; i++)
        {
            float a = Mathf.Deg2Rad * (45f * i + 22.5f);
            verts.Add(new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, halfZ));
        }

        int backStart = verts.Count;
        verts.Add(Vector3.back * halfZ);
        for (int i = 0; i < sides; i++)
        {
            float a = Mathf.Deg2Rad * (45f * i + 22.5f);
            verts.Add(new Vector3(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius, -halfZ));
        }

        for (int i = 1; i <= sides; i++)
        {
            int next = i < sides ? i + 1 : 1;
            tris.Add(0);
            tris.Add(i);
            tris.Add(next);
        }

        for (int i = 1; i <= sides; i++)
        {
            int next = i < sides ? i + 1 : 1;
            tris.Add(backStart);
            tris.Add(backStart + next);
            tris.Add(backStart + i);
        }

        for (int i = 1; i <= sides; i++)
        {
            int next = i < sides ? i + 1 : 1;

            int f0 = i;
            int f1 = next;
            int b0 = backStart + i;
            int b1 = backStart + next;

            tris.Add(f0);
            tris.Add(f1);
            tris.Add(b1);

            tris.Add(f0);
            tris.Add(b1);
            tris.Add(b0);
        }

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    private Mesh BuildCylinderMesh(float radius, float height, int sides = 16)
    {
        Mesh mesh = new Mesh();
        mesh.name = "Pole";

        List<Vector3> verts = new();
        List<int> tris = new();

        for (int i = 0; i <= sides; i++)
        {
            float a = (i / (float)sides) * Mathf.PI * 2f;
            float x = Mathf.Cos(a) * radius;
            float z = Mathf.Sin(a) * radius;

            verts.Add(new Vector3(x, -height * 0.5f, z));
            verts.Add(new Vector3(x, height * 0.5f, z));

            if (i < sides)
            {
                int idx = i * 2;
                tris.Add(idx);
                tris.Add(idx + 1);
                tris.Add(idx + 3);

                tris.Add(idx);
                tris.Add(idx + 3);
                tris.Add(idx + 2);
            }
        }

        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
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
}
