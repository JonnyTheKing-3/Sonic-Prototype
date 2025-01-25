using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting.ReorderableList;
using UnityEngine.UIElements;

public class SonicMovement : MonoBehaviour
{
    [Header("INPUTS")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("VALUES FOR MOVEMENT")]
    public float moveSpeed;
    public float acceleration;
    public float deceleration;
    public float jumpForce;
    
    [Header("JUMP RELATED")]
    public float jumpCooldown;
    bool readyToJump;
    
    [Header("GROUND CHECK")]
    public float playerHeight;
    public LayerMask whatIsGround;

    [Header("REFERENCES")]
    public Transform orientation;
    public Rigidbody rb;
    public ConstantForce cf;
    
    [Header("STATUS")]
    public bool grounded;
    public float horizontalInput;
    public float verticalInput;
    public Vector3 moveDirection;

    private void Start()
    {
        // getting references
        rb = GetComponent<Rigidbody>();
        cf = GetComponent<ConstantForce>();

        // initiating values
        readyToJump = true;
    }

    private void Update()
    {
        // Status on player
        grounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, whatIsGround);

        MyInput();
        
        cf.enabled = !grounded;
    }
    
    private void MyInput()
    {
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // when to jump
        if(Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            readyToJump = false;

            Jump();

            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }
    
    private void Jump()
    {
        // reset y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);
    }
    
    private void ResetJump()
    {
        readyToJump = true;
    }
    
    private void FixedUpdate()
    {
        MovePlayer();
    }
    
    private void MovePlayer()
    {
        moveDirection = orientation.forward * verticalInput + orientation.right * horizontalInput;

        moveDirection = orientation.forward * verticalInput * CalculateMovementZ() + orientation.right * horizontalInput * CalculateMovementX();
        
        rb.AddForce(moveDirection);
    }

    private float CalculateMovementX()
    {
        // Speed of player
        float targetSpeed = horizontalInput * moveSpeed;
        
        // Calculate the difference of the speed the player wants to go by
        // how fast the player is already going
        float speedDif = targetSpeed - rb.velocity.x;
        
        // If the player is going the same direction as the one they pressed, accelerate. Else, decelerate
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        
        // Applied acceleration to speed difference, AND THEN, raised to something so that
        // speed increases and decreases depending on the current movement status of the player.
        // Lastly, preserve the direction
        return Mathf.Pow(Mathf.Abs(speedDif) * accelRate, .9f) * Math.Sign(speedDif);
    }
    private float CalculateMovementZ()
    {
        // Speed of player
        float targetSpeed = verticalInput * moveSpeed;
        
        // Calculate the difference of the speed the player wants to go by
        // how fast the player is already going
        float speedDif = targetSpeed - rb.velocity.z;
        
        // If the player is going the same direction as the one they pressed, accelerate. Else, decelerate
        float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;
        
        // Applied acceleration to speed difference, AND THEN, raised to something so that
        // speed increases and decreases depending on the current movement status of the player.
        // Lastly, preserve the direction
        return Mathf.Pow(Mathf.Abs(speedDif) * accelRate, .9f) * Math.Sign(speedDif);
    }

    // private bool OnSlope()
    // {
    //     if(Physics.Raycast(transform.position, Vector3.down, out slopeHit, playerHeight * 0.5f + 0.3f))
    //     {
    //         angle = Vector3.Angle(Vector3.up, slopeHit.normal);
    //         return angle < maxSlopeAngle && angle != 0;
    //     }
    //
    //     return false;
    // }
    //
    // private Vector3 GetSlopeMoveDirection()
    // {
    //     return Vector3.ProjectOnPlane(moveDirection, slopeHit.normal).normalized;
    // }
}