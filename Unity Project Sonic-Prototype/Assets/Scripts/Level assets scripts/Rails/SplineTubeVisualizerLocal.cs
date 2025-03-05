using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

[RequireComponent(typeof(SplineContainer))]
[RequireComponent(typeof(MeshFilter))]
[RequireComponent(typeof(MeshRenderer))]
public class SplineTubeVisualizerLocal : MonoBehaviour
{
    [Header("Tube Settings")]
    public float tubeRadius = 0.5f;    // Radius of the tube
    public int pathSegments = 50;      // Number of samples along the spline
    public int radialSegments = 8;     // Number of vertices in each circular ring
    public Material tubeMaterial;      // Optional material

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private SplineContainer splineContainer;

    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        splineContainer = GetComponent<SplineContainer>();

        if (tubeMaterial != null)
        {
            meshRenderer.material = tubeMaterial;
        }

        GenerateTubeMeshLocal();
    }

    /// <summary>
    /// Generates a continuous "tube" mesh along the spline, in local space.
    /// This ensures the mesh lines up exactly with the spline, even if the
    /// GameObject (or its parent) is moved/rotated in the scene.
    /// </summary>
    void GenerateTubeMeshLocal()
    {
        if (!splineContainer)
        {
            Debug.LogError("No SplineContainer found on this GameObject!");
            return;
        }

        // Prepare lists for mesh data
        List<Vector3> vertices  = new List<Vector3>();
        List<Vector3> normals   = new List<Vector3>();
        List<Vector2> uvs       = new List<Vector2>();
        List<int> triangles     = new List<int>();

        // We'll create (pathSegments + 1) rings along the spline
        for (int i = 0; i <= pathSegments; i++)
        {
            float t = i / (float)pathSegments;

            // Evaluate the spline in WORLD space
            Vector3 worldPos     = splineContainer.EvaluatePosition(t);
            Vector3 worldTangent = splineContainer.EvaluateTangent(t);

            // Convert to local space relative to THIS transform
            Vector3 localPos     = transform.InverseTransformPoint(worldPos);
            Vector3 localTangent = transform.InverseTransformDirection(worldTangent).normalized;

            // Pick a "binormal" by crossing with up (or a fallback if nearly parallel)
            Vector3 binormal = Vector3.Cross(localTangent, Vector3.up).normalized;
            if (binormal.sqrMagnitude < 0.0001f)
            {
                // Fallback if the spline tangent is almost vertical
                binormal = Vector3.Cross(localTangent, Vector3.right).normalized;
            }

            // Then another perpendicular is the "normal"
            Vector3 normal = Vector3.Cross(binormal, localTangent).normalized;

            // Build the ring around localPos
            for (int r = 0; r < radialSegments; r++)
            {
                float frac  = r / (float)radialSegments;
                float angle = frac * Mathf.PI * 2f;
                float sin   = Mathf.Sin(angle);
                float cos   = Mathf.Cos(angle);

                // Circular offset in local space
                Vector3 offset    = binormal * cos * tubeRadius + normal * sin * tubeRadius;
                Vector3 vertexPos = localPos + offset;

                vertices.Add(vertexPos);

                // Normal is outward from the tube's center
                normals.Add(offset.normalized);

                // Simple UV: (u = ring fraction, v = spline fraction)
                uvs.Add(new Vector2(frac, t));
            }
        }

        // Create triangles between adjacent rings
        for (int i = 0; i < pathSegments; i++)
        {
            int ringStart     = i * radialSegments;
            int nextRingStart = (i + 1) * radialSegments;

            for (int r = 0; r < radialSegments; r++)
            {
                int current         = ringStart + r;
                int next            = ringStart + (r + 1) % radialSegments;
                int currentNextRing = nextRingStart + r;
                int nextNextRing    = nextRingStart + (r + 1) % radialSegments;

                // Two triangles per "quad"
                triangles.Add(current);
                triangles.Add(next);
                triangles.Add(currentNextRing);

                triangles.Add(currentNextRing);
                triangles.Add(next);
                triangles.Add(nextNextRing);
            }
        }

        // Build the final mesh
        Mesh tubeMesh = new Mesh();
        tubeMesh.indexFormat = UnityEngine.Rendering.IndexFormat.UInt32; // In case vertex count exceeds 65k
        tubeMesh.SetVertices(vertices);
        tubeMesh.SetNormals(normals);
        tubeMesh.SetUVs(0, uvs);
        tubeMesh.SetTriangles(triangles, 0);
        tubeMesh.RecalculateBounds();
        // If you need tangents for normal maps, you can do: tubeMesh.RecalculateTangents();

        // Assign to the MeshFilter
        meshFilter.mesh = tubeMesh;
    }
}
