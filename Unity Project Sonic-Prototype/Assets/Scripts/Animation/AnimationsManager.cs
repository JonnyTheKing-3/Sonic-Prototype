using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class AnimationsManager : MonoBehaviour
{
    [Header("MODEL SETTINGS")]
    public float groundOffset = 0f;
    public float bumperZOffset = -1f; // Used for when hitting a bumper

    [Header("BUMPER SETTINGS")]
    public float rotationTime;
    public float scalar;
    
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
        if (player.movementState == SonicMovement.MovementState.OnBumperInertia) 
        {
            transform.position = player.transform.position + (player.transform.forward * bumperZOffset);
        }
        else
        {
            transform.position = player.transform.position + (player.transform.up * groundOffset);
        }   
    
        // Determine the raw forward direction:
        Vector3 rawForward = (player.moveDirection.sqrMagnitude > 0 ? player.moveDirection : player.LastSpeedDirection).normalized;
    
        // Project the raw forward vector onto the plane defined by player.transform.up.
        Vector3 forward = Vector3.ProjectOnPlane(rawForward, player.transform.up).normalized;
    
        // If we have a valid forward direction, compute the target rotation.
        if (forward != Vector3.zero)
        {

            if (player.movementState == SonicMovement.MovementState.RailGrinding)
            {
                // Construct a rotation matrix manually
                Matrix4x4 rotationMatrix = new Matrix4x4();
                rotationMatrix.SetColumn(0, -player._T);      // Z-axis (forward)
                rotationMatrix.SetColumn(1, player._N);      // Y-axis (up)
                rotationMatrix.SetColumn(2, player._right);  // X-axis (right)
                rotationMatrix.SetColumn(3, new Vector4(0, 0, 0, 1)); // Homogeneous coordinate.
            
                // Extract quaternion from rotation matrix
                if (player._N == Vector3.zero || player._T == Vector3.zero ||player._right == Vector3.zero) {return;}
                Quaternion _targetRotation = Quaternion.LookRotation(rotationMatrix.GetColumn(2), rotationMatrix.GetColumn(1));
            
                // Apply to character
                transform.rotation = _targetRotation;
                return;
            }

            else if (player.AllowInput)
            {   
                Quaternion targetRotation = Quaternion.LookRotation(forward, player.transform.up);

                // Smoothly interpolate from the current rotation to the target rotation.
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, RotationSmoothingFactor * Time.deltaTime);

            }
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
                animator.SetBool("Sliding", false);
                animator.SetBool("RailGrinding", false);
                animator.SetBool("OnBumperForce", false);
                
                // blend tree is from 0-1, so what we want to pass to the animator is the percentage of how close we are to reaching the speed value
                float speedVal = player.CurrentSpeedMagnitude / 100f;
                animator.SetFloat("CurrentSpeed", speedVal);
        
                // this part makes it look cooler because it makes the animation move at speeds relative to players actual speeds
                if (player.CurrentSpeedMagnitude > SpeedDivider) { animator.speed = player.CurrentSpeedMagnitude / SpeedDivider; } 
                
                animator.speed = player.grounded && player.readyToJump ? animator.speed : .75f;
                //Debug.Log(animator.speed);
                
                animator.SetBool("grounded", player.grounded && player.readyToJump);
                animator.SetBool("OnDelay", PlayerBoxTrigger.inDelay);
                break;

            case SonicMovement.MovementState.Spindashing:
                animator.SetBool("Boosting", false);
                animator.SetBool("SpinDashing", true);
                animator.SetBool("OnBumperForce", false);
                animator.SetBool("OnDelay", PlayerBoxTrigger.inDelay);
                
                animator.speed = .75f;
                break;
            
            case SonicMovement.MovementState.Boosting:
                animator.SetBool("Boosting", true);
                animator.SetBool("SpinDashing", false);
                animator.SetBool("OnBumperForce", false);
                animator.SetBool("OnDelay", PlayerBoxTrigger.inDelay);
                
                animator.SetBool("grounded", player.grounded && player.readyToJump);
                animator.speed = player.grounded && player.readyToJump ? BoostAnimSpeed : 1f; // Boost animation is a bit slow out of box, but air-boost is a decent speed
                break;
            
            case SonicMovement.MovementState.HomingAttacking:
                animator.SetBool("Boosting", false);
                animator.SetBool("SpinDashing", false);
                animator.SetBool("OnDelay", false);
                animator.SetBool("OnBumperForce", false);
                
                animator.speed = 1f;
                break;
            
            case SonicMovement.MovementState.Stomp:
                animator.SetBool("Stomping", true);
                animator.SetBool("OnDelay", false);
                animator.SetBool("OnBumperForce", false);

                if (player.InStompWaitTime)
                {
                    animator.SetBool("Stomping", false);
                    animator.SetBool("StompWait", true);
                }
                break;
            
            case SonicMovement.MovementState.Sliding:
                animator.SetBool("Sliding", true);
                animator.SetBool("SpinDashing", false);
                animator.SetBool("OnBumperForce", false);
                animator.SetBool("grounded", player.grounded && player.readyToJump);
                animator.SetBool("OnDelay", PlayerBoxTrigger.inDelay);
                animator.speed = 1f;
                break;
            
            case SonicMovement.MovementState.RailGrinding:
                animator.SetBool("SpinDashing", false);
                animator.SetBool("StompWait", false);
                animator.SetBool("Sliding", false);
                animator.SetBool("Stomping", false);
                animator.SetBool("OnDelay", false);
                animator.SetBool("RailGrinding", true);
                animator.SetBool("OnBumperForce", false);
                break;
        
            case SonicMovement.MovementState.OnBumperInertia:
                animator.SetBool("grounded", false);
                animator.SetBool("SpinDashing", false);
                animator.SetBool("StompWait", false);
                animator.SetBool("Sliding", false);
                animator.SetBool("Stomping", false);
                animator.SetBool("OnDelay", false);
                animator.SetBool("RailGrinding", false);
                animator.SetBool("OnBumperForce", true);
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

    public void RotationSpeedUpCaller()
    {
        StartCoroutine(RotationSpeedUp());
    }
    
    IEnumerator RotationSpeedUp()
    {
        RotationSmoothingFactor *= scalar;
        yield return new WaitForSeconds(rotationTime);
        RotationSmoothingFactor /= scalar;
    }

}
