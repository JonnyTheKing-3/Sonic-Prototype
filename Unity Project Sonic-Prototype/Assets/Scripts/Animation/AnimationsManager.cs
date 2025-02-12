using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimationsManager : MonoBehaviour
{
    [Header("MODEL SETTINGS")]
    public Vector3 offset = Vector3.zero;
    public float RotationSmoothingFactor = 10f;
    
    [Header("REFERENCES")]
    
    public SonicMovement player;
    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<SonicMovement>();
    }
    
    void SetupModelPositionAndRotation()
    {
        // Update position with offset.
        transform.position = player.transform.position + offset;
    
        // Determine the raw forward direction:
        Vector3 rawForward = (player.moveDirection.sqrMagnitude > 0 ? player.moveDirection : player.LastSpeedDirection).normalized;
    
        // Project the raw forward vector onto the plane defined by player.transform.up.
        Vector3 forward = Vector3.ProjectOnPlane(rawForward, player.transform.up).normalized;
    
        // If we have a valid forward direction, compute the target rotation.
        if (forward != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(forward, player.transform.up);
        
            // Smoothly interpolate from the current rotation to the target rotation.
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSmoothingFactor * Time.deltaTime);
        }
    }
    
    void Update()
    {
        SetupModelPositionAndRotation();
    }
}
