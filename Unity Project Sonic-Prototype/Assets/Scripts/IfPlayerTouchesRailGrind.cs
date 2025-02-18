using System;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Splines;

public class IfPlayerTouchesRailGrind : MonoBehaviour
{
    public CinemachineSplineCart cart;
    public SplineContainer railPath;
    public RailMovement railMovement;
    
    
    private void Start()
    {
        GameObject parent = transform.parent.gameObject;
        
        cart = parent.GetComponentInChildren<CinemachineSplineCart>();
        railMovement = cart.GetComponent<RailMovement>();
        railPath = parent.GetComponentInChildren<SplineContainer>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("RAIL TRIGGER");
        if (other.CompareTag("Player"))
        {
            // Allow time for player to leave rail if they jump
            if (other.GetComponent<SonicMovement>().inIgnoreGroundJumpTime) { return; }
            
            SetupBeforeRailGrinding(other.GetComponent<SonicMovement>());
        }
    }

    public void SetupBeforeRailGrinding(SonicMovement other)
    {
        // Stop player
        other.rb.linearVelocity = Vector3.zero;
            
        // move cart towards where player landed
        float newCartPos = GetClosestPointOnTrack(other.transform.position);
        cart.SplinePosition = newCartPos;
            
        // Attach player to cart
        other.CurrentCart = cart;
        other.movementState = SonicMovement.MovementState.RailGrinding;
        railMovement.playerIsOnRail = true;
    }
    
    
    private float GetClosestPointOnTrack(Vector3 position)
    {
        float closestDistance = Mathf.Infinity;
        float closestPoint = 0f;

        // Step through the track and find the closest point
        for (float i = 0f; i < 1f; i += 0.05f) // Small increments for accuracy
        {
            Vector3 pathPosition = railPath.EvaluatePosition(i);
            float distance = Vector3.Distance(position, pathPosition);
    
            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestPoint = i;
            }
        }
    
        return closestPoint;
    }
}
