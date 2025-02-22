using UnityEngine;
using UnityEngine.Splines;
using System.Collections.Generic;

[RequireComponent(typeof(SplineContainer))]
public class SplineMeshCollider : MonoBehaviour
{
    public float radius = 0.5f; // Collider thickness
    public int splineSegments = 50; // Number of colliders along the spline
    public float capsuleLengthFactor = 1.2f; // Capsule length multiplier

    public bool ignoreRail;

    private void Start()
    {
        GenerateCollidersAlongSpline();
    }

    void GenerateCollidersAlongSpline()
    {
        SplineContainer splineContainer = GetComponent<SplineContainer>();
        if (splineContainer == null)
        {
            Debug.LogError("No SplineContainer found on " + gameObject.name);
            return;
        }

        float step = 1f / splineSegments;

        for (int i = 0; i < splineSegments; i++)
        {
            float t1 = i * step;
            float t2 = (i + 1) * step;

            Vector3 pos1 = splineContainer.EvaluatePosition(t1);
            Vector3 pos2 = splineContainer.EvaluatePosition(t2);

            if (pos1 == pos2)
            {
                Debug.LogWarning("Skipping collider: Two consecutive points are identical.");
                continue;
            }

            // Create GameObject for CapsuleCollider
            GameObject capsuleObj = new GameObject($"SplineCollider_{i}");
            capsuleObj.transform.SetParent(transform);
            capsuleObj.layer = 7; // layer 7 is the rail layer

            CapsuleCollider capsule = capsuleObj.AddComponent<CapsuleCollider>();

            // Position the collider at the midpoint
            Vector3 midPoint = (pos1 + pos2) / 2f;
            capsuleObj.transform.position = midPoint;

            // Rotate to align with the spline direction
            Vector3 direction = (pos2 - pos1).normalized;
            capsuleObj.transform.rotation = Quaternion.LookRotation(direction);

            // Set collider properties
            capsule.radius = radius;
            capsule.height = Vector3.Distance(pos1, pos2) * capsuleLengthFactor;
            capsule.direction = 2; // Z-axis
            capsule.isTrigger = true;

            // Debug.Log($"Placed collider at {midPoint} with height {capsule.height}");
            NewRailMoveDetection railScript = capsuleObj.AddComponent<NewRailMoveDetection>();
            railScript.cartIterations = 25; // Adjust default values if needed
            railScript.roughIterations = 0.04f;
            railScript.ignoreWaitTime = 0.2f;
        }
    }

    // Draw colliders for debugging
    private void OnDrawGizmos()
    {
        SplineContainer splineContainer = GetComponent<SplineContainer>();
        if (splineContainer == null) return;

        Gizmos.color = Color.green;
        float step = 1f / splineSegments;

        for (int i = 0; i < splineSegments; i++)
        {
            float t1 = i * step;
            float t2 = (i + 1) * step;

            Vector3 pos1 = splineContainer.EvaluatePosition(t1);
            Vector3 pos2 = splineContainer.EvaluatePosition(t2);

            Gizmos.DrawLine(pos1, pos2);
            Gizmos.DrawWireSphere(pos1, radius);
        }
    }
}
