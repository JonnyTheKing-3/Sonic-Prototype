using System;
using Unity.Cinemachine;
using UnityEngine;

public class IfPlayerTouchesRailGrind : MonoBehaviour
{
    public CinemachineSplineCart cart;

    private void Start()
    {
        cart = transform.parent.GetComponentInChildren<CinemachineSplineCart>();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("RAIL TRIGGER");
        if (other.CompareTag("Player"))
        {
            // Allow time for player to leave rail if they jump
            if (other.GetComponent<SonicMovement>().inIgnoreGroundJumpTime) { return; }
            
            other.GetComponent<SonicMovement>().rb.linearVelocity = Vector3.zero;
            other.GetComponent<SonicMovement>().CurrentCart = cart;
            other.GetComponent<SonicMovement>().movementState = SonicMovement.MovementState.RailGrinding;
            
            cart.GetComponent<RailMovement>().playerIsOnRail = true;
        }
    }
}
