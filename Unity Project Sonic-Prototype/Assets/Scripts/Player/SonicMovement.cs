using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime.Tree;
using Unity.VisualScripting.ReorderableList;
using UnityEngine.PlayerLoop;
using UnityEngine.UIElements;

public class SonicMovement : MonoBehaviour
{
    [Header("INPUTS")]
    public KeyCode jumpKey = KeyCode.Space;

    [Header("VALUES FOR MOVEMENT")]
    public float speed;
    public float GoingDownHillSpeed;
    public float GoingUpHillSpeed;
    [Range(0,1)]public float airSpeedMultiplier;
    public float acceleration;
    public float deceleration;
    public float turnSpeed; // How fast the player can change direction while running

    [Header("JUMP RELATED")]
    public float jumpForce;
    public float shortHopForce;
    public float TimeAllowedToPerformShortHop;  //.058 (2-5 frames) is around the average time for most games
    public float jumpCooldown; // time to reset readyToJump
    public float gravity;
    
    [Header("GROUND")]
    public float surfaceHitRay;
    public float groundStickingDistance;
    [Space]
    public LayerMask whatIsGround;
    public float GroundStickingOffset = 1f;
    public RaycastHit surfaceHit;

    public enum SurfaceState { Flat, GoingUpHill, GoingDownHill, Air }
    
    [Header("STATUS")]
    public bool grounded;
    public bool rayHit;
    public bool ShortHopping = false;
    private bool readyToJump;
    public float distancePlayerToGround;
    public float horizontalInput;
    public float verticalInput;
    private Vector3 moveDirection;
    private Vector3 horizontalVelocity;
    public float DesiredSpeed;
    public float CurrentSpeedMagnitude;
    public SurfaceState surfaceState;
    public SurfaceState lastSurfaceState;

    [Header("REFERENCES")]
    public Transform orientation;
    public Rigidbody rb;
    
    private void Start()
    {
        // getting references
        rb = GetComponent<Rigidbody>();

        // initiating values
        readyToJump = true;
        ShortHopping = false;
    }
    
    private void Update()
    {
        MyInput();
    }

    private void MyInput()
    {
        // Get horizontal/vertical input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");

        // Jump when the player is on the ground and presses the jump key
        if(Input.GetKeyDown(jumpKey) && readyToJump && grounded) { StartCoroutine(JumpRoutine()); }
    }
    
    private IEnumerator JumpRoutine()
    {
        // Record the start time.
        float startTime = Time.time;
        ShortHopping = false; // reset short hopping just in case
    
        // Run a timer. If the player releases the jump key before the timer is done, they want to short hop. Otherwise, they want to full hop
        while (Time.time - startTime < TimeAllowedToPerformShortHop)
        {
            if (Input.GetKeyUp(jumpKey))
            {
                ShortHopping = true;
                break;
            }
            yield return null;
        }
    
        /*
            * The ground check is still true for a few frames after the jump
            * so this readyToJump check makes sure the player can't get another jump
            * a few moments after the first
           */
        readyToJump = false;
        Jump(ShortHopping);     // Call jump knowing if we're gonna full hop or short hop
    
        // Wait for jump cooldown before allowing the next jump.
        yield return new WaitForSeconds(jumpCooldown);
        ResetJump();
    }
    
    private void Jump(bool shortHop)
    {
        float forceToUse = shortHop ? shortHopForce : jumpForce;
        
        // reset y velocity and apply upward impulse
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(transform.up * forceToUse, ForceMode.Impulse);
    }
    
    private void ResetJump()
    {
        readyToJump = true;
    }

    private void FixedUpdate()
    {
        // Shoot a ray downwards and get the info of the surface it hits, assuming it hits a suface and update ground state and distance from player to ground
        rayHit = Physics.Raycast(transform.position, -transform.up, out surfaceHit, surfaceHitRay, whatIsGround);
        UpdateGroundedStatus();
        transform.up = grounded ? surfaceHit.normal : Vector3.up; // rotate the player towards the surface
        
        // reset short hopping and stick the player to the ground if they are in the ground
        if (grounded && readyToJump) { ShortHopping = false; StickPlayerToGround(); }
        
        MovePlayer();
        CurrentSpeedMagnitude = rb.velocity.magnitude;
    }

    private void UpdateGroundedStatus()
    {
        if (rayHit)
        {
            // Calculate the distance to the ground, and determine if the player is grounded
            distancePlayerToGround = Vector3.Distance(transform.position, surfaceHit.point);
            grounded = distancePlayerToGround <= groundStickingDistance;
        }
        else
        {
            grounded = false;
        }
    }
    
    private void StickPlayerToGround()
    {
        // What's below works BUT REMEMBER THAT IN SLOPES, the offset can look a bit bigger in than in the ground. So when I put the model in, make sure it's good on slopes
        // If it's not, just make sure to scale the offset by an accurate 
        
        // Get the target position, which is right above the surface the player is standing on, stick the player to that position
        Vector3 targetPosition = surfaceHit.point + (surfaceHit.normal * GroundStickingOffset);

        transform.position = targetPosition;
    }
    
    private void MovePlayer()
    {
        // Calculate move direction based on input and camera orientation
        moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;

        // Get surface normal based on grounding status
        Vector3 Surface = grounded ? surfaceHit.normal : Vector3.up;
    
        // Apply movement on the slope by projecting onto the surface. In other words, get the move direction considering the surface the player is on
        Vector3 SurfaceAppliedDirection = Vector3.ProjectOnPlane(moveDirection, Surface);

        // Get the current surface, which also updates the desired speed (in the ground for now. Later I'll adjust air movement)
        surfaceState = SurfacePlayerIsStandingOn();

        // If the player is in the air, slow down the speed. Also, reward the player if they short hopped by letting them keep there speed
        DesiredSpeed = grounded || ShortHopping ? DesiredSpeed : DesiredSpeed * airSpeedMultiplier;
        
        // Calculate target velocity
        Vector3 targetVelocity = SurfaceAppliedDirection.normalized * DesiredSpeed;

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
            if (surfaceState == lastSurfaceState)
            {
                switch (surfaceState)
                {
                    
                    case SurfaceState.Flat:
                        // If our current speed is greater than our surface desired speed, simply move towards it normally. Otherwise, always keep the max speed
                        if (currentSpeed > DesiredSpeed)
                        {
                            horizontalVelocity.Normalize();
                            horizontalVelocity *= currentSpeed;
                        }
                        else
                        {
                            horizontalVelocity.Normalize();
                            horizontalVelocity *= Mathf.Max(prevSpeed, currentSpeed);
                        }
                        break;
                
                    case SurfaceState.GoingUpHill:
                        if (currentSpeed > DesiredSpeed)
                        {
                            horizontalVelocity.Normalize();
                            horizontalVelocity *= currentSpeed;
                        }
                        else
                        {
                            horizontalVelocity.Normalize();
                            horizontalVelocity *= Mathf.Max(prevSpeed, currentSpeed);
                        }
                        break;
                
                    case SurfaceState.GoingDownHill:
                        horizontalVelocity.Normalize();
                        horizontalVelocity *= Mathf.Max(prevSpeed, currentSpeed);
                        break;
                }   
            }
            else
            {
                // Debug.Log("LAST: " + lastSurfaceState + " --- CURRENT: " + surfaceState);
                horizontalVelocity.Normalize();
                horizontalVelocity *= Mathf.Max(prevSpeed, currentSpeed);
            }
        }
        
        // Preserve vertical velocity
        float verticalVelocity = rb.velocity.y;

        // If grounded, reset vertical velocity. Otherwise, apply gravity to it
        if (grounded && readyToJump) { verticalVelocity = 0f; }
        else { verticalVelocity += -gravity * Time.fixedDeltaTime;}

        // Update last surface
        lastSurfaceState = surfaceState;
        
        // Combine horizontal and vertical velocity
        rb.velocity = horizontalVelocity + Vector3.up * verticalVelocity;
    }

    // Returns the surface the player is standing on based on surface and speed direction, as well as updates DesiredSpeed
    private SurfaceState SurfacePlayerIsStandingOn()
    {
        if (!grounded) { DesiredSpeed = speed; return SurfaceState.Air; }

        // Check the angle of the surface. 0 = flat surface, > 0 = slope
        float angle = Vector3.Angle(transform.up, Vector3.up);
        switch (angle)
        {
            case 0:
                DesiredSpeed = speed;
                return SurfaceState.Flat;
            
            case > 0:
                // Check the direction the player is running in to see how they are running in the slope
                if (rb.velocity.y > .01f) { DesiredSpeed = GoingUpHillSpeed; return SurfaceState.GoingUpHill; }
                if (rb.velocity.y < -.01f) { DesiredSpeed = GoingDownHillSpeed; return SurfaceState.GoingDownHill; }
                return SurfaceState.Flat;
        }
        
        return SurfaceState.Flat;
    }
}