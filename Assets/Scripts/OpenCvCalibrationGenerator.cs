using System;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class OpenCvCalibrationGenerator : MonoBehaviour
{
    [Header("Chessboard Settings")]
    [Tooltip("Number of squares horizontally")]
    public int squaresX = 9;

    [Tooltip("Number of squares vertically")]
    public int squaresY = 6;

    [Tooltip("Size of one square in meters (e.g. 0.024)")]
    public float squareSize = 0.024f;

    void Awake()
    {
        Generate();
    }

    private void OnValidate()
    {
        Generate();
    }

    private void Generate()
    {
        var mesh = new Mesh
        {
            name = "OpenCV Chessboard"
        };

        var vertices = new List<Vector3>();
        var triangles = new List<int>();
        var colors = new List<Color>();
        var vertIndex = 0;

        for (var y = 0; y < squaresY; y++)
        {
            for (var x = 0; x < squaresX; x++)
            {
                var x0 = x * squareSize;
                var x1 = (x + 1) * squareSize;
                var y0 = y * squareSize;
                var y1 = (y + 1) * squareSize;

                // Quad vertices (XZ plane)
                vertices.Add(new Vector3(x0, 0, y0));
                vertices.Add(new Vector3(x1, 0, y0));
                vertices.Add(new Vector3(x1, 0, y1));
                vertices.Add(new Vector3(x0, 0, y1));

                // Two triangles
                triangles.Add(vertIndex + 0);
                triangles.Add(vertIndex + 2);
                triangles.Add(vertIndex + 1);

                triangles.Add(vertIndex + 0);
                triangles.Add(vertIndex + 3);
                triangles.Add(vertIndex + 2);

                // Alternating color
                var isWhite = (x + y) % 2 == 0;
                var c = isWhite ? Color.white : Color.black;

                colors.Add(c);
                colors.Add(c);
                colors.Add(c);
                colors.Add(c);

                vertIndex += 4;
            }
        }

        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.SetColors(colors);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;
    }
}