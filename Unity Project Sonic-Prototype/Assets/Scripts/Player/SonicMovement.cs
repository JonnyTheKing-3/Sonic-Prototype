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
    public RaycastHit surfaceHit;
    public float threshold;

    [Header("STATUS")]
    public bool grounded;
    public float horizontalInput;
    public float verticalInput;
    public Vector3 moveDirection;

    [Header("REFERENCES")]
    public Transform orientation;
    public Rigidbody rb;
    public ConstantForce cf;
    private Quaternion defaultRotation;

    [Header("DEBUG RAYS")] 
    [SerializeField] private bool FixedUpdateRay;
    [SerializeField] private bool UpdateRay;
    [SerializeField] private bool targetVelocityRay;
    [SerializeField] private bool ProjectRay;
    [SerializeField] private bool ProjectOnPlaneRay;


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
        defaultRotation = transform.rotation;
    }
    
    private void Update()
    {
        grounded = Physics.Raycast(transform.position, -transform.up, out surfaceHit, playerHeight, whatIsGround);
        StickPlayerToGround();
        ApplyGravity();
        MyInput();
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

        if (Input.GetKeyDown(KeyCode.Keypad1))
        {
            FixedUpdateRay = !FixedUpdateRay;
        }
        if (Input.GetKeyDown(KeyCode.Keypad2))
        {  
            UpdateRay = !UpdateRay;
        }
        if (Input.GetKeyDown(KeyCode.Keypad3))
        {  
            targetVelocityRay = !targetVelocityRay;
        }
        if (Input.GetKeyDown(KeyCode.Keypad4))
        {  
            ProjectRay = !ProjectRay;
        }
        if (Input.GetKeyDown(KeyCode.Keypad5))
        {  
            ProjectOnPlaneRay = !ProjectOnPlaneRay;
        }
    }
    
    private void ApplyGravity()
    {
        cf.enabled = !grounded;
        
        // Update the gravity in case we change it during play
        if (cf.enabled) { cf.force = Vector3.down * gravity; }
    }
    
    private void StickPlayerToGround()
    {
        if (!grounded) {return;}

        Vector3 targetPosition = new Vector3(transform.position.x, surfaceHit.point.y + playerHeight -.1f, transform.position.z);
        transform.position = targetPosition;
        
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
        // Calculate move direction based on input and camera orientation
        moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;

        Vector3 Surface = grounded ? surfaceHit.normal : transform.up;
        // Get the correct force to apply according to the surface to player is in
        Vector3 SurfaceAppliedDirection = Vector3.ProjectOnPlane(moveDirection, Surface);

        Vector3 targetVelocity = SurfaceAppliedDirection.normalized * speed;
        
        // preparing specs for moving the character. Turn speed is to make sure when massively changing directions, the player loses speed
        float rad = turnSpeed * Mathf.PI * Time.deltaTime;
        float appropriateAcceleration = moveDirection != Vector3.zero ? acceleration : deceleration;
     
        // move character
        rb.velocity += Vector3.RotateTowards(Vector3.ProjectOnPlane(rb.velocity, Surface), targetVelocity, rad,
                    appropriateAcceleration * Time.deltaTime) + Vector3.Project(rb.velocity, Surface);
        
        if (ProjectRay) {Debug.DrawRay(transform.position, Vector3.Project(rb.velocity, Surface), Color.blue);}
    }
}