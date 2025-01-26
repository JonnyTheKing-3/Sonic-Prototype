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
    public float acceleration;
    public float deceleration;
    public float jumpForce;
    public float NormaleSpeedCap;

    [Header("JUMP RELATED")]
    public float jumpCooldown;
    private bool readyToJump;
    public float gravity;
    
    [Header("GROUND")]
    public float playerHeight;
    public LayerMask whatIsGround;
    public RaycastHit surfaceHit;

    [Header("STATUS")]
    public bool grounded;
    public float horizontalInput;
    public float verticalInput;
    public Vector3 moveDirection;

    [Header("REFERENCES")]
    public Transform orientation;
    public Rigidbody rb;
    public ConstantForce cf;
    
    
    private void Start()
    {
        // getting references
        rb = GetComponent<Rigidbody>();
        cf = GetComponent<ConstantForce>();

        // initiating values
        readyToJump = true;
        cf.enabled = true;
        cf.force = Vector3.down * gravity;
        cf.enabled = false;
    }

    private void Update()
    {
        grounded = Physics.Raycast(transform.position, Vector3.down, out surfaceHit, playerHeight * 0.5f + 0.2f, whatIsGround);
        MyInput();
        ApplyGravity();
    }

    private void ApplyGravity()
    {
        cf.enabled = !grounded;
        
        // Update the gravity in case we change it during play
        if (cf.enabled) { cf.force = Vector3.down * gravity; }
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
        CapSpeed(NormaleSpeedCap);
        Debug.Log(rb.velocity.sqrMagnitude);
    }
    
    private void MovePlayer()
    {
        // Calculate move direction based on input
        moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;

        // Get current velocity
        Vector3 currentVelocity = rb.velocity;

        // Project the current velocity onto the move direction
        Vector3 velocityInMoveDirection = Vector3.Project(currentVelocity, moveDirection);

        // Calculate the force required to align the velocity with the move direction
        Vector3 alignmentForce = (moveDirection * NormaleSpeedCap - velocityInMoveDirection);

        // Apply force for sharp directional change
        if (moveDirection.magnitude > 0)
        {
            rb.AddForce(alignmentForce * acceleration, ForceMode.Acceleration);
        }

        // Apply deceleration when no input and grounded
        if (grounded && moveDirection.magnitude == 0 && currentVelocity.magnitude > 0.1f)
        {
            Vector3 decelerationForce = -currentVelocity.normalized * deceleration;
            rb.AddForce(decelerationForce, ForceMode.Acceleration);
        }
    }


    
    private void CapSpeed(float limit)
    {
        
        if (rb.velocity.sqrMagnitude > limit && grounded)
        {
           rb.velocity = Vector3.ClampMagnitude(rb.velocity, limit);
        }
    }
    
    
}