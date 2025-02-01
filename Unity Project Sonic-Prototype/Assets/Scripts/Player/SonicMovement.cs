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
    public float turnSpeed; // How fast the player can change direction while running
    public float jumpForce;

    [Header("JUMP RELATED")]
    public float jumpCooldown; // time to reset readyToJump
    private bool readyToJump;
    public float gravity;
    
    [Header("GROUND")]
    public LayerMask whatIsGround;
    public float playerHeight;
    public float GroundStickingOffset = 1f;
    public RaycastHit surfaceHit;

    [Header("STATUS")]
    public bool grounded;
    public float horizontalInput;
    public float verticalInput;
    private Vector3 moveDirection;
    public Vector3 horizontalVelocity;

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
        transform.up = surfaceHit.normal; // rotate player so it matches the surface normal
        
        MyInput();
        StickPlayerToGround();
    }

    private void MyInput()
    {
        // Get horizontal/vertical input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jump when the player is on the ground and presses the jump key
        if(Input.GetKeyDown(jumpKey) && readyToJump && grounded)
        {
            // The ground check is still true for a few frames after the jump, so this readyToJump check makes sure the player can't get another jump right after the first
            readyToJump = false;
            Jump();
            Invoke(nameof(ResetJump), jumpCooldown);
        }
    }

    private void Jump()
    {
        // reset y velocity and apply upward impulse
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

        // What's below works BUT REMEMBER THAT IN SLOPES, the offset can look a bit bigger in than in the ground. So when I put the model in, make sure it's good on slopes
        // If it's not, just make sure to scale the offset by an accurate 
        
        // Get the target position, which is right above the surface the player is standing on, stick the player to that positionn
        Vector3 targetPosition = surfaceHit.point + (surfaceHit.normal * GroundStickingOffset);

        transform.position = targetPosition;
    }
    
    private void FixedUpdate()
    {
        MovePlayer();
        // Debug.Log(rb.velocity.magnitude);
    }
    
    private void MovePlayer()
    {
        // Calculate move direction based on input and camera orientation
        moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;

        // Get surface normal based on grounding status
        Vector3 Surface = grounded ? surfaceHit.normal : Vector3.up;
    
        // Apply movement on the slope by projecting onto the surface. In other words, get the move direction considering the surface the player is on
        Vector3 SurfaceAppliedDirection = Vector3.ProjectOnPlane(moveDirection, Surface);

        // Calculate target velocity
        Vector3 targetVelocity = SurfaceAppliedDirection.normalized * speed;

        // Smoothly rotate towards target velocity and apply acceleration or deceleration
        float rad = turnSpeed * Mathf.PI * Time.deltaTime;
        float appropriateAcceleration = moveDirection != Vector3.zero ? acceleration : deceleration;

        float prevSpeed = horizontalVelocity.magnitude; // Store previous velocity
        // Move our current velocity towards our desired velocity
        horizontalVelocity = Vector3.RotateTowards(Vector3.ProjectOnPlane(rb.velocity, Surface), targetVelocity, rad, 
            appropriateAcceleration * Time.deltaTime);
        float currentSpeed = horizontalVelocity.magnitude; // Store current velocity
        
        // If we want to move, make sure the magnitude of the speed doesn't abruptly change. When entering different surfaces, the transition hindered the magnitude
        // This check makes sure the speed is kept at where it's supposed to be
        if (moveDirection != Vector3.zero)
        {
            horizontalVelocity.Normalize();
            horizontalVelocity *= Mathf.Max(prevSpeed, currentSpeed);
        }
        
        // Preserve vertical velocity
        float verticalVelocity = rb.velocity.y;

        // If grounded, reset vertical velocity. Otherwise, apply gravity to it
        if (grounded && readyToJump) { verticalVelocity = 0f; }
        else { verticalVelocity += -gravity * Time.fixedDeltaTime;}

        // Combine horizontal and vertical velocity
        rb.velocity = horizontalVelocity + Vector3.up * verticalVelocity;
    }
}