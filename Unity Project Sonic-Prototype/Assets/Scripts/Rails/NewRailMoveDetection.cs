using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Splines;

public class NewRailMoveDetection : MonoBehaviour
{
    [SerializeField, Min(0)] public int cartIterations; // higher = accuracy in landing on rail, lower = performance
    [SerializeField, Range(0.000000001f,1)] public float roughIterations;
    [SerializeField] public float ignoreWaitTime = .2f;
    
    public CinemachineSplineCart cart;
    public SplineContainer railPath;

    private void Start()
    {
        GameObject parent = transform.parent.gameObject;
        
        cart = parent.transform.parent.GetComponentInChildren<CinemachineSplineCart>();
        railPath = parent.GetComponent<SplineContainer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Allow time for player to leave rail if they jump
            if (other.GetComponent<SonicMovement>().inIgnoreGroundJumpTime || 
                other.GetComponent<SonicMovement>().movementState == SonicMovement.MovementState.RailGrinding ||
                transform.parent.GetComponent<SplineMeshCollider>().ignoreRail)
            { return; }
            
            SetupBeforeRailGrinding(other.GetComponent<SonicMovement>());
        }
    }

    public void SetupBeforeRailGrinding(SonicMovement other)
    {
        // Pass the in which the player entered the rail
        other.RailStartSpeed = other.CurrentSpeedMagnitude;
        Vector3 vel = other.rb.linearVelocity.normalized;
        
        // Stop player to make sure the don't bounce on the rail for trying to move. Remember, we want to move the cart which moves sonic, not sonic himself
        other.rb.linearVelocity = Vector3.zero;
            
        // move cart towards where player landed
        float newCartPos = GetClosestPointOnTrack(other.transform.position);
        cart.SplinePosition = newCartPos;
        
        // Determines what direction the player should go
        Vector3 positionVector = cart.Spline.EvaluateTangent(cart.SplinePosition);
        positionVector.Normalize();
        float angle = Vector3.Dot(vel,positionVector);
        other.RailStartSpeed *= angle; // change direction in relation to the angle
        other.TowardsEndPoint = angle > 0f; 
        
        // Attach player to cart
        other.CurrentCart = cart;
        cart.PositionUnits = PathIndexUnit.Distance;
        other.movementState = SonicMovement.MovementState.RailGrinding;
    }
    
    private float GetClosestPointOnTrack(Vector3 position)
    {
        cart.PositionUnits = PathIndexUnit.Normalized; 

        float roughStep = roughIterations; // Rough initial search step size
        float closestPoint = 0f;
        float closestDistance = Mathf.Infinity;

        // **1. Rough Search** (Quickly find an approximate closest point)
        for (float i = 0f; i <= 1f; i += roughStep)
        {
            Vector3 pointOnSpline = railPath.EvaluatePosition(i);
            float distance = Vector3.Distance(position, pointOnSpline);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = i;
            }
        }

        // **2. Binary Search for Precision**
        float left = Mathf.Max(closestPoint - roughStep, 0f);
        float right = Mathf.Min(closestPoint + roughStep, 1f);
    
        int iterations = 20; // Binary search refinement iterations

        for (int i = 0; i < iterations; i++)
        {
            float mid1 = left + (right - left) / 3f;
            float mid2 = right - (right - left) / 3f;

            Vector3 pos1 = railPath.EvaluatePosition(mid1);
            Vector3 pos2 = railPath.EvaluatePosition(mid2);

            float dist1 = Vector3.Distance(position, pos1);
            float dist2 = Vector3.Distance(position, pos2);

            if (dist1 < dist2)
            {
                right = mid2;
            }
            else
            {
                left = mid1;
            }

            if (dist1 < closestDistance)
            {
                closestDistance = dist1;
                closestPoint = mid1;
            }
            if (dist2 < closestDistance)
            {
                closestDistance = dist2;
                closestPoint = mid2;
            }
        }

        return closestPoint;
    }

    private void OnTriggerExit(Collider other)
    {
        StartCoroutine(ResetRail()); }

    IEnumerator ResetRail()
    {
        yield return new WaitForSeconds(ignoreWaitTime);
        transform.parent.GetComponent<SplineMeshCollider>().ignoreRail = false;
    }


}
