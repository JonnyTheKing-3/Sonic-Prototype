using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimationsManager : MonoBehaviour
{
    [Header("MODEL SETTINGS")]
    public Vector3 offset = Vector3.zero;
    
    [Header("ANIMATION SPEEDS")]
    public float RotationSmoothingFactor = 10f;
    public float BoostAnimSpeed = 2f;

    [Header("ANIMATION SPEED")] 
    public float SpeedDivider = 60f;
    
    [Header("REFERENCES")] 
    public Animator animator;
    
    public SonicMovement player;
    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<SonicMovement>();
        animator = GetComponent<Animator>();
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
        
        switch (player.movementState)
        {
            case SonicMovement.MovementState.Regular:
                animator.SetBool("Boosting", false);
                animator.SetBool("SpinDashing", false);
                animator.SetBool("StompWait", false);
                
                // blend tree is from 0-1, so what we want to pass to the animator is the percentage of how close we are to reaching the speed value
                float speedVal = player.CurrentSpeedMagnitude / 100f;
                animator.SetFloat("CurrentSpeed", speedVal);
        
                // this part makes it look cooler because it makes the animation move at speeds relative to players actual speeds
                if (player.CurrentSpeedMagnitude > SpeedDivider) { animator.speed = player.CurrentSpeedMagnitude / SpeedDivider; } 
                
                animator.speed = player.grounded && player.readyToJump ? animator.speed : 1f;
                
                animator.SetBool("grounded", player.grounded && player.readyToJump);
                break;

            case SonicMovement.MovementState.Spindashing:
                animator.SetBool("Boosting", false);
                animator.SetBool("SpinDashing", true);
                
                animator.speed = 1f;
                break;
            
            case SonicMovement.MovementState.Boosting:
                animator.SetBool("Boosting", true);
                animator.SetBool("SpinDashing", false);
                
                animator.SetBool("grounded", player.grounded && player.readyToJump);
                animator.speed = player.grounded && player.readyToJump ? BoostAnimSpeed : 1f; // Boost animation is a bit slow out of box, but air-boost is a decent speed
                break;
            
            case SonicMovement.MovementState.HomingAttacking:
                animator.SetBool("Boosting", false);
                animator.SetBool("SpinDashing", false);
                
                animator.speed = 1f;
                break;
            
            case SonicMovement.MovementState.Stomp:
                animator.SetBool("Stomping", true);

                if (player.InStompWaitTime)
                {
                    animator.SetBool("Stomping", false);
                    animator.SetBool("StompWait", true);
                }
                break;
        }
        
        
    }

    public void TriggerJumpAnimation()
    {
        animator.SetTrigger("Jump");
    }
    public void TriggerHomingAttackTrickAnimation()
    {
        animator.SetTrigger("HomingAttackTrick");
    }
}
