using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.ReorderableList;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public class SonicMovement : MonoBehaviour
{
    [Header("INPUTS")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("VALUES FOR MOVEMENT")]
    public float speed;
    public float acceleration;
    public float deceleration;
    public float turnSpeed;
    public float jumpForce;

    [Header("JUMP RELATED")]
    public float jumpCooldown;
    private bool readyToJump;
    public float gravity;
    
    [Header("GROUND")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public float GroundStickingOffset = 1f;
    public RaycastHit surfaceHit;
    public RaycastHit AlignmentHit;

    [Header("STATUS")]
    public bool grounded;
    public float horizontalInput;
    public float verticalInput;
    private Vector3 moveDirection;

    [Header("REFERENCES")]
    public Transform orientation;
    public Rigidbody rb;
    

    private void Start()
    {
        // getting references
        rb = GetComponent<Rigidbody>();

        // initiating values
        readyToJump = true;
    }
    
    private void Update()
    {
        grounded = Physics.Raycast(transform.position, -transform.up, out surfaceHit, playerHeight, whatIsGround);
        transform.up = surfaceHit.normal;
        
        MyInput();
        StickPlayerToGround();
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
    
    private void StickPlayerToGround()
    {
        // Only stick player to the ground when the player is on the ground
        if (!grounded || !readyToJump) {return;}

        // This works BUT REMEMBER THAT IN SLOPES, the offset can look a bit bigger in slopes than in the ground. So when I put the model in, make sure it's good on slopes
        Vector3 targetPosition = surfaceHit.point + (surfaceHit.normal * GroundStickingOffset);
        transform.position = targetPosition;
    }
    
    private void FixedUpdate()
    {
        MovePlayer();
    }
    
    private void MovePlayer()
    {
        // Calculate move direction based on input and camera orientation
        moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;

        // Get surface normal based on grounding status
        Vector3 Surface = grounded ? surfaceHit.normal : Vector3.up;
    
        // Apply movement on the slope by projecting onto the surface
        Vector3 SurfaceAppliedDirection = Vector3.ProjectOnPlane(moveDirection, Surface);

        // Calculate target velocity
        Vector3 targetVelocity = SurfaceAppliedDirection.normalized * speed;

        // Smoothly rotate towards target velocity and apply acceleration or deceleration
        float rad = turnSpeed * Mathf.PI * Time.deltaTime;
        float appropriateAcceleration = moveDirection != Vector3.zero ? acceleration : deceleration;
        
        // Move our current velocity towards our desired velocity
        Vector3 horizontalVelocity = Vector3.RotateTowards(Vector3.ProjectOnPlane(rb.velocity, Surface), targetVelocity, rad,
            appropriateAcceleration * Time.deltaTime);

        // Preserve vertical velocity
        float verticalVelocity = rb.velocity.y;

        // If grounded, reset vertical velocity. Otherwise, apply gravity to it
        if (grounded && readyToJump) { verticalVelocity = 0f; }
        else { verticalVelocity += -gravity * Time.fixedDeltaTime;}

        // Combine horizontal and vertical velocity
        rb.velocity = horizontalVelocity + Vector3.up * verticalVelocity;
    }
}