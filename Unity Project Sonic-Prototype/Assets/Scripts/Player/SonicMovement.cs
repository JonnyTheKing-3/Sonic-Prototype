using System.Collections;
using UnityEngine;
using TMPro;
using Unity.Cinemachine;
using UnityEngine.Splines;
using Unity.VisualScripting;
using UnityEngine.Rendering;
using System.Security.Cryptography;
using System;

public class SonicMovement : MonoBehaviour
{
    [Header("INPUTS")]
    public KeyCode jumpKey = KeyCode.Space;
    public KeyCode homingAttackKey = KeyCode.P;
    public KeyCode SpindashKey = KeyCode.O;
    public KeyCode BoostKey = KeyCode.I;
    public KeyCode StompKey = KeyCode.L;
    public KeyCode SlideKey = KeyCode.K;
    

    [Header("VALUES FOR MOVEMENT")]
    public float speed;
    public float GoingDownHillSpeed;
    public float GoingUpHillSpeed;
    [Range(0,2)]public float airSpeedMultiplier;
    public float acceleration;
    public float deceleration;
    public float turnSpeed; // How fast the player can change direction while running
    
    
    [Header("SPINDASH")] 
    public float SpindDashSpeed;
    public float SpinDashDownHillSpeed;
    public float SpinDashDeceleration;
    public float SpinDashDecelerationUpSlope;
    public float SpinDashDownHillAcceleration;
    public float SpinDashTurnSpeed;
    public float ChargePower; // How fast speed charges
    public float ChargedSpeed; // How much speed has been charged since we started charging spin dash. Gets reset to zero when we exit the spindash
    public bool StartingSpinDash = false;
    public bool SpinDashStartTime = false;
    public float waitToResetStartTime = .3f;
    public float InitialImpulseIfMoving = 100f;


    [Header("BOOST")] 
    public float BoostSpeed;
    public float BoostTurnSpeed;
    public float BoostConsumption;
    [Tooltip("O = empty, .5 = half full, 1 = full, and so on")] 
    [Range(0,1)] public float BoostMeter;
    
    
    [Header("JUMP RELATED")]
    public float jumpForce;
    public float shortHopForce;
    public float gravity;
    public float TimeAllowedToPerformShortHop;  //.058 (2-5 frames) is around the average time for most games
    [Range(0,1)]public float blendFactorJumpingUpHill = .5f;
    [Range(0,1)]public float blendFactorJumpingDownHill = .3f;
    public float jumpCooldown; // time to reset readyToJump
    [Space] // below is for jump locking
    public float jumpIgnoreDuration = 0.15f;
    private float jumpStartTime;
    private Vector3 jumpNormal;
    public bool TouchingWallBeforeJump = false;

    
    [Header("GROUND")]
    public float surfaceHitRay;
    public float groundStickingDistance;
    [Space]
    public LayerMask whatIsGround;
    public float GroundStickingOffset = 1f;
    public RaycastHit surfaceHit;

    
    [Header("HOMING ATTACK")]
    
    [Tooltip("Homing speed is really fast. This slows it down... Or speeds it up if the value is below zero")] public float hommingSpeedLimiter;
    public float NoTargethomingSpeed;
    public float ImpulseAfterAttack;
    public float ImpulseAfterAttackStrongMomentum;
    public float ImpulseAfterAttackWeakMomentum;
    public float homingAttackDistance;
    [SerializeField] private LayerMask HomingAttackLayer;
    [SerializeField] private float PlayerDistWeight = 1f;       // How strongly the distance from the player affects it's standing in being the target for a homing attack
    [SerializeField] private float CameraCenterDistWeight = 1f; // How strongly the distance from the cener of the camera affects it's standing in being the target for a homing attack
    [SerializeField] private Transform Target;  // Homing Attack target

    
    [Header("STOMP")] 
    [SerializeField] private float StompSpeed = 20f;
    public float AfterStompWaitTime = .6f;
    public bool InStompWaitTime = false;

    
    [Header("SLIDING")]
    [SerializeField] private float TopSlideSpeed;
    [SerializeField] private float SlideDeceleration;
    [SerializeField] private float SlideUpHillDeceleration;
    [SerializeField] private float desiredSlideDeceleration;
    [SerializeField] private float SlideTurnSpeed;


    [Header("RAIL GRINDINNG")] 
    public float RailStartSpeed = 0f;
    public float desiredRailAcceleration;
    [Space]
    public float railDeceleration;
    public float railUpDeceleration;
    public float railDownAcceleration;
    public Vector3 lastRailDirection;
    [Space]
    public CinemachineSplineCart CurrentCart;
    public float RailOffsetPos;
    public LayerMask whatIsRail;
    public bool TowardsEndPoint;
    public float rotateSmoothFactor;

    
    public enum SurfaceState { Flat, GoingUpHill, GoingDownHill, Air }
    public enum MovementState { Regular, HomingAttacking, Spindashing, Boosting, Stomp, Sliding, RailGrinding }

    
    [Header("STATUS")]
    public bool grounded;
    public bool rayHit;
    public bool ShortHopping = false;
    public bool readyToJump;
    public bool inIgnoreGroundJumpTime = false;
    public bool CanHomingAttack = false;
    public bool SomethingInFront = false;
    public float distancePlayerToGround;
    public float horizontalInput;
    public float verticalInput;
    public Vector3 moveDirection;
    private Vector3 horizontalVelocity;
    public float spindashDesiredAcceleration;
    public float DesiredSpeed;
    public float CurrentSpeedMagnitude;
    public Vector3 LastSpeedDirection; // Used for homing attack
    public SurfaceState surfaceState;
    private SurfaceState lastSurfaceState;
    public MovementState movementState;
    public MovementState LastMovementState;

    
    [Header("REFERENCES")]
    public Transform orientation;
    public Rigidbody rb;
    public CapsuleCollider triggerColliderForJumpTime;  // During ignore grounding while jumping, if the ground touches this collider, turn grounding back on
    public Camera cam;
    public GameObject GFX;
    public GameObject SpinBallCharge;
    public GameObject SpinBallForm;
    public GameObject BoostForm;
    public AnimationsManager animManager;
    [SerializeField] private TMP_Text speedText;

    
    [Header("EXTRA")] 
    [SerializeField] private bool ShowSpeed = true;
    [SerializeField] private KeyCode ShowSpeedKey;

    private void Start()
    {
        // getting references
        rb = GetComponent<Rigidbody>();

        // initiating values
        movementState = MovementState.Regular;
        cam = Camera.main;
        CanHomingAttack = true;
        ShortHopping = false;
        readyToJump = true;
        triggerColliderForJumpTime = GetComponent<CapsuleCollider>();
        triggerColliderForJumpTime.enabled = false;
        jumpStartTime = -jumpIgnoreDuration;
        StartingSpinDash = false;
    }
    
    // Mainly used for detecting surface during JumpTime where we ignore grounded. Used so player doesn't bounce off in case they reach a surface before the timer ends
    private void OnTriggerEnter(Collider other)
    {
        // Make sure that if we're touching a wall before the jump, the player doesn't activate sticking to the ground. Turned on in playerboxtrigger script
        if (TouchingWallBeforeJump) { return; }

        // Debug.Log("Touched something");
        triggerColliderForJumpTime.enabled = false;
        // Check if the colliding object's layer is in the whatIsGround LayerMask.
        // (1 << other.gameObject.layer) creates a bitmask for the object's layer.
        if ((whatIsGround.value & (1 << other.gameObject.layer)) != 0)
        {
           //  Debug.Log("Touched: " + other.name);
            // Debug.Log("Touched ground during jump time");
            jumpStartTime = jumpIgnoreDuration;
            readyToJump = true;
            rayHit = Physics.Raycast(transform.position, -transform.up, out surfaceHit, surfaceHitRay, whatIsGround);
            UpdateGroundedStatus();
        }
    }
    
    private void Update()
    {
        MyInput();
        
        // Show the right gfx. I'll switch this type of thing for a model with animations later
        if (movementState == MovementState.Regular) 
        { GFX.SetActive(true); SpinBallCharge.SetActive(false); SpinBallForm.SetActive(false); BoostForm.SetActive(false); }
        
        else if (StartingSpinDash) 
        { GFX.SetActive(false); SpinBallCharge.SetActive(true); SpinBallForm.SetActive(false); BoostForm.SetActive(false); }
        
        else if (movementState == MovementState.Spindashing && !StartingSpinDash)
        { GFX.SetActive(false); SpinBallCharge.SetActive(false); SpinBallForm.SetActive(true); BoostForm.SetActive(false);}
        
        else if (movementState == MovementState.Boosting)
        { GFX.SetActive(false); SpinBallCharge.SetActive(false); SpinBallForm.SetActive(false); BoostForm.SetActive(true); }
        
        // For show case purposes
        if (Input.GetKeyDown(ShowSpeedKey)) { ShowSpeed = !ShowSpeed;}
        if (ShowSpeed) { speedText.text = "Speed: " + CurrentSpeedMagnitude; }
        else { speedText.text = ""; }        
    }

public bool wasOnRail;
    private void MyInput()
    {
        // Don't accept any input during these moments. This makes it easy to avoid any potential interruptions
        if (movementState == MovementState.HomingAttacking || movementState == MovementState.Stomp || InStompWaitTime) { return;}
        
        // Get horizontal/vertical input
        horizontalInput = Input.GetAxisRaw("Horizontal");
        verticalInput = Input.GetAxisRaw("Vertical");
        
        if (movementState == MovementState.Spindashing && StartingSpinDash) { ChargeSpinDash(); return; }
        
        // Jump
        if (Input.GetKeyDown(jumpKey) && (readyToJump && grounded || movementState == MovementState.RailGrinding))
        {
            // If we're jumping off a rail, make cart null and ignore the rail for a bit so that we have enough time to get out
            if (CurrentCart != null)
            {
                CurrentCart.transform.parent.GetChild(0).GetComponentInChildren<SplineMeshCollider>().ignoreRail = true;
                CurrentCart = null;
            }
            
            InStompWaitTime = false;
            if (movementState == MovementState.Spindashing || movementState == MovementState.Sliding || movementState == MovementState.RailGrinding)
            {
                movementState = MovementState.Regular;
                
                // Keep momentum if we're exiting a rail
                if (LastMovementState == MovementState.RailGrinding) 
                {
                    rb.linearVelocity = lastRailDirection.normalized * RailStartSpeed; 
                    wasOnRail = true;
                }
            }
            StartCoroutine(JumpRoutine());
        }

        // Stomp
        if (Input.GetKeyDown(StompKey) && movementState == MovementState.Regular && surfaceState == SurfaceState.Air)
        {
            rb.linearVelocity = Vector3.zero;
            movementState = MovementState.Stomp;
            return;
        }

        // Spin-dash
        if (movementState == MovementState.Regular && Input.GetKeyDown(SpindashKey))
        {
            SpinDashStartTime = true;
            if (Mathf.Abs(rb.linearVelocity.x) < .1f && Mathf.Abs(rb.linearVelocity.z) < .1f && grounded)
            {
                // Debug.Log("Spindash Start");
                // Check if the player started from zero velocity and/or in the air
                StartingSpinDash = true;
                ChargedSpeed = 0f;
                movementState = MovementState.Spindashing;
                ChargeSpinDash();
            }
            else if (grounded && Mathf.Abs(rb.linearVelocity.x) > .1f && Mathf.Abs(rb.linearVelocity.z) > .1f)
            {
                StartingSpinDash = false;
                SpinDashStartTime = false;
                rb.AddForce(rb.linearVelocity.normalized * InitialImpulseIfMoving, ForceMode.Impulse);
                ChargedSpeed = rb.linearVelocity.magnitude > DesiredSpeed ? DesiredSpeed: rb.linearVelocity.magnitude;
                movementState = MovementState.Spindashing;
                return;
            }
        }
        
        // Spin-dash to Regular
        if (movementState == MovementState.Spindashing && Input.GetKeyDown(SpindashKey) && !StartingSpinDash)
        {
            // Debug.Log("STOPPED SPINDASH BY KEY PRESS");
            movementState = MovementState.Regular;
        }

        // homing attack
        if (!grounded && Input.GetKeyDown(homingAttackKey) && CanHomingAttack) // We shouldn't do a homing attack from the ground
        {
            // Freeze the player to make sure homing attack starts without any forces attached
            rb.linearVelocity = Vector3.zero;
            
            Target = GetTargetForHomingAttack();
            movementState = MovementState.HomingAttacking;
            Debug.Log("Started Homing");
            animManager.animator.SetTrigger("StartedHomingAttack");
        }

        // Boost
        if (Input.GetKeyDown(BoostKey) && BoostMeter > 0f && movementState != MovementState.RailGrinding) { movementState = MovementState.Boosting; }
        else if (Input.GetKeyUp(BoostKey) || (BoostMeter <= 0f && movementState == MovementState.Boosting))
        {
            // If we're on rail, don't go to normal
            if (movementState == MovementState.RailGrinding) { return;}
            
            movementState = MovementState.Regular;
        } // exit bossting
        
        // Sliding
        if (movementState != MovementState.Boosting && grounded && readyToJump && Input.GetKeyDown(SlideKey))
        {
            // Debug.Log("Start Slide");
            // Make sure players speed caps as soon as they slide. Kinda like a negative side effect to prevent spamming
            if (rb.linearVelocity.magnitude > TopSlideSpeed) { rb.linearVelocity = TopSlideSpeed * LastSpeedDirection; }
            movementState = MovementState.Sliding;
        }
        else if (movementState == MovementState.Sliding && Input.GetKeyUp(SlideKey))
        {
            // Debug.Log("Stop Slide");
            movementState = MovementState.Regular;
        }
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
        Jump(ShortHopping);     // Call jump knowing if we're gonna full hop or short hop
    
        // Wait for jump cooldown before allowing the next jump.
        yield return new WaitForSeconds(jumpCooldown);
        ResetJump();
    }

    private Vector3 jumpOrigin;
    private void Jump(bool shortHop)
    {
        jumpOrigin = transform.position;
        float forceToUse = shortHop ? shortHopForce : jumpForce;
        
        // Record the ground normal at the moment of jump
        jumpNormal = surfaceHit.normal;
        
        // If we jumped from a rail, jump from transform.up. Might change jumpNormal to be transform.up always for simplicty. I'll have to see if that works though
        if (wasOnRail) { jumpNormal = _N.normalized;}
        
        // Debug.Log(jumpNormal);
        jumpStartTime = Time.time;

        // Remove any velocity component in the jump direction.
        // This prevents any unwanted buildup from the ground movement.
        if (!wasOnRail)
        {
            Vector3 velocityWithoutJumpComponent = Vector3.ProjectOnPlane(rb.linearVelocity, jumpNormal);
            rb.linearVelocity = velocityWithoutJumpComponent;
        }


        // mark as not grounded so that the ground adjustments in FixedUpdate won’t interfere with the jump.
        grounded = false;
        readyToJump = false;
        triggerColliderForJumpTime.enabled = true;
        
        // If moving upward (e.g. running uphill), blend the jump direction toward world up to be able to make jumps bigger and cooler
        Vector3 jumpDirection = jumpNormal;
        if (surfaceState == SurfaceState.GoingUpHill)
        {
            // Adjust the blend factor (0.5f) to taste.
            jumpDirection = Vector3.Lerp(jumpNormal, Vector3.up, blendFactorJumpingUpHill);
        }
        else if (surfaceState == SurfaceState.GoingDownHill)
        {
            // Adjust the blend factor (0.5f) to taste.
            jumpDirection = Vector3.Lerp(jumpNormal, Vector3.up, blendFactorJumpingDownHill);
        }
        
        animManager.TriggerJumpAnimation();

        if (wasOnRail) { rb.AddForce(jumpNormal * forceToUse, ForceMode.Impulse); }
        else { rb.AddForce(jumpDirection * forceToUse, ForceMode.Impulse); }

    }

    private void ResetJump()
    {
        readyToJump = true;
        wasOnRail = false;
    }
    
    private void ChargeSpinDash()
    {
        if (Input.GetKeyUp(SpindashKey))
        {
            // spin dash start
            if (LastSpeedDirection == Vector3.zero) { LastSpeedDirection = transform.forward;}
            
            Vector3 dashDireciton = Vector3.ProjectOnPlane(LastSpeedDirection, surfaceHit.normal).normalized;
            // Debug.Log("DashDirection: " + dashDireciton);
            
            rb.linearVelocity += ChargedSpeed * dashDireciton;
            // Debug.Log("velocity: " + rb.velocity.magnitude);
            
            StartingSpinDash = false;
            StartCoroutine(SpinStartTime());
        }
        else
        {
            // Calculate move direction based on input and camera orientation
            moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;
            ChargedSpeed = Mathf.Lerp(ChargedSpeed, SpindDashSpeed, ChargePower * Time.deltaTime);
        }
    }

    IEnumerator SpinStartTime()
    {
        //Debug.Log("Start spin reset");
        yield return new WaitForSeconds(waitToResetStartTime);
        //Debug.Log("Reset Succesful");
        SpinDashStartTime = false;
    }

    private Transform GetTargetForHomingAttack()
    {
        // Check if there is any object in homing range
        Collider[] colliders = Physics.OverlapSphere(transform.position, homingAttackDistance, HomingAttackLayer);
        
        Transform bestTarget = null;
        float bestScore = float.MaxValue;
        Vector2 viewportCenter = new Vector2(0.5f, 0.5f);

        // Of the ones that were found, check if they are within camera view,
        // and if they are, choose the one closest to the center of the camera as well as to the player
        foreach (Collider col in colliders)
        {
            // 1: Convert the object's position to viewport coordinates (coordinates relative to camera view)
            Vector3 viewportPos = cam.WorldToViewportPoint(col.transform.position);

            // 2: Check if the object is in front of the camera (z > 0) and within the viewport bounds ((0,0) is bottom left corner, (1,1) is top right corner)
            if (viewportPos.z > 0 && viewportPos.x >= 0 && viewportPos.x <= 1 && viewportPos.y >= 0 && viewportPos.y <= 1)
            {
                Vector2 objectViewportPos = new Vector2(viewportPos.x, viewportPos.y);
                
                // 3: Calculate how far the object is from the center of the viewport and world distance
                float distanceFromCenter = Vector2.Distance(viewportCenter, objectViewportPos);
                float distFromPlayer = Vector3.Distance(transform.position, col.transform.position); // Use regular coords because we want distance from player not camera
                
                float score = (distanceFromCenter * CameraCenterDistWeight) + (distFromPlayer * PlayerDistWeight); // tweak weight as needed

                // 4: Lower score means closer to the center and player
                if (score < bestScore)
                {
                    bestScore = score;
                    bestTarget = col.transform;
                }
            }
        }

        if (bestTarget != null)
        {
            // Debug.Log("Selected: " + bestTarget.name);
            return bestTarget;
        }

        return null; // If no target could be chosen, return null
    }
    
    // Let's me see how big the selection radius is for homing attack
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, homingAttackDistance);
    }

    public Vector3 LastJumpDir; // for debugging
    private void FixedUpdate()
    {
        // Ignore everything except state functions if we're homing attacking to avoid any interruptions
        if (movementState != MovementState.HomingAttacking)
        {
            /*
             * With this check, we make sure that when the player jumps,
             * the vertical velocity in move-player doesn't interfere with the jump.
             * We keep ground at false briefly, so the vertical velocity
             * doesn't get affected by the few frames the character remains in grounded after jumping
            */
            if (Time.time - jumpStartTime >= jumpIgnoreDuration)
            {
                // Get ground info and status
                inIgnoreGroundJumpTime = false;
                triggerColliderForJumpTime.enabled = false;
                
                // Get grounding info
                if (movementState != MovementState.RailGrinding)
                {
                    rayHit = Physics.Raycast(transform.position, -transform.up, out surfaceHit, surfaceHitRay, whatIsGround);
                    UpdateGroundedStatus();
                }
            }
            else { inIgnoreGroundJumpTime = true; }
    
            transform.up = grounded ? surfaceHit.normal : Vector3.up;
    
            // reset short hopping and stick the player to the ground if they are in the ground
            if (grounded && readyToJump)
            {
                ShortHopping = false;

                // Make sure that the player has a bit of time after they leave jumping off of touching the wall to not automatically stick to the ground if they are moving forwward
                if (PlayerBoxTrigger.inDelay && rb.linearVelocity.y > 0f) { return;}
                StickPlayerToGround();
            }

            // Turn on homing attack if we hit the ground after not being able to homing attack
            if (!CanHomingAttack && grounded) { CanHomingAttack = true; }

        }

        // Move the player based on the player state
        switch (movementState)
        {
            case MovementState.Regular:
                // If the player crashes with something, make velocity zero
                MovePlayer();
                break;
            
            case MovementState.HomingAttacking:
                HomingAttack();
                break;
            
            case MovementState.Spindashing:
                // If we're in the startup, charge up. Otherwise, continue with regular spindash movemennt
                if (!StartingSpinDash) {SpindashMovement();}

                // If the speed is too low or we leave the ground, go back to normal
                if (rb.linearVelocity.magnitude < 5f && !StartingSpinDash && !SpinDashStartTime || !grounded && !StartingSpinDash)
                {
                    // if (!grounded) { Debug.Log("STOPPED SPINDASH BECAUSE WE'RE NOT GROUNDED"); }
                    // else {Debug.Log("STOPPED SPINDASH FOR LOW SPEED");}
                    
                    movementState = MovementState.Regular;
                }

                break; // StartingSpinDash logic (Charging the spindash) is handled in MyInput and ChargeSpinDash
            
            case MovementState.Boosting:
                BoostMovement();
                break;
            
            case MovementState.Stomp:
                // Do nothing. Just wait until control is passed over to regular
                if (InStompWaitTime) { return; }
                Stomp();
                break;
            
            case MovementState.Sliding:
                // If we go below a certain speed, go back to regular
                if (rb.linearVelocity.magnitude < 5f || !grounded) { movementState = MovementState.Regular;}
                Slide();
                break;
        
            case MovementState.RailGrinding:
                RailGrinding();
                break;
        }

        // Keep track of speed, direction and last movement state
        CurrentSpeedMagnitude = rb.linearVelocity.magnitude;
        if (moveDirection != Vector3.zero) { LastSpeedDirection = new Vector3(moveDirection.x, 0f, moveDirection.z); }
        LastMovementState = movementState;
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

    private bool DetectedSomethingInVelocityDirection(Vector3 horizontalVel)
    {
        // I'm using this variable in case I ever want to debug
        bool passValue = false;

        // Calculate the next spot
        float mag = rb.linearVelocity.magnitude < .1f ? .1f : rb.linearVelocity.magnitude; // first get an appropriate magnitude

        // If we're boosting, the first frames magnitude will be really low in comparison to all the rest. So we make sure the first frame's check has enough magnitude
        if (LastMovementState != MovementState.Boosting && movementState == MovementState.Boosting) 
        {
           mag = BoostSpeed;
        }

        Vector3 Velocity = horizontalVel.normalized * mag;
        Vector3 startPos = transform.position + Vector3.up * .5f; // I want the ray to be slightly above the midpoint of the player so that it doesn't collide with small objects
        Vector3 nextSpot = startPos + Velocity * Time.fixedDeltaTime; 
        
        // If the next spot hit's somthing, and it was angled perpendicular or towards the player, then we detected something. Otherwise, we didn't
        if (Physics.Linecast(startPos, nextSpot, out RaycastHit hit, whatIsGround))
        {
            // Dot = 0: Perpendicular, Dot = -1: Towards the player, Dot = 1; Away from player
            float dot = Vector3.Dot(transform.up.normalized, hit.normal.normalized);
            if (dot <= 0)   
            {
                passValue = true;
                animManager.animator.speed = .75f;    // reset animation speed so that it doesn't stay in the magnitude it previously was            
            }
        }

        // Draw ray
        // if (passValue) {Debug.DrawLine(startPos, nextSpot, Color.green);}
        // else { Debug.DrawLine(startPos, nextSpot, Color.red);}

        // if that next spot collides with something, 
        return passValue;
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
        horizontalVelocity = Vector3.RotateTowards(Vector3.ProjectOnPlane(rb.linearVelocity, Surface), targetVelocity, rad, 
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
                horizontalVelocity.Normalize();
                horizontalVelocity *= Mathf.Max(prevSpeed, currentSpeed);
            }
        }
        
        // Preserve vertical velocity
        float verticalVelocity = rb.linearVelocity.y;

        // If grounded, reset vertical velocity. Otherwise, apply gravity to it
        if (grounded && readyToJump)  { verticalVelocity = 0f; }
        else { verticalVelocity += -gravity * Time.fixedDeltaTime;}

        // Update last surface
        lastSurfaceState = surfaceState;
        
        // If something is in front, clash with it
        if (DetectedSomethingInVelocityDirection(horizontalVelocity) && grounded && readyToJump)
        {
            rb.linearVelocity = Vector3.zero;
            horizontalVelocity = Vector3.zero;
        }
        else
        {
            // Combine horizontal and vertical velocity
            rb.linearVelocity = horizontalVelocity + transform.up * verticalVelocity;
            // Debug.DrawRay(transform.position, rb.linearVelocity.normalized *3f, Color.red);
            // Debug.DrawRay(transform.position, horizontalVelocity.normalized *3f, Color.green);
        }

    }

    // Returns the surface the player is standing on based on player rotation, as well as updates DesiredSpeed
    private SurfaceState SurfacePlayerIsStandingOn()
    {
        if (!grounded && movementState != MovementState.RailGrinding) { DesiredSpeed = speed; return SurfaceState.Air; }

        // Check the angle of the surface. 0 = flat surface, > 0 = slope
        float angle = Vector3.Angle(transform.up, Vector3.up);

        switch (movementState)
        {
            case MovementState.Regular:
                switch (angle)
                {
                    case 0:
                        DesiredSpeed = speed;
                        return SurfaceState.Flat;
            
                    case > 0:
                        // Check the direction the player is running in to see how they are running in the slope
                        if (rb.linearVelocity.y > .01f) { DesiredSpeed = GoingUpHillSpeed; return SurfaceState.GoingUpHill; }
                        if (rb.linearVelocity.y < -.01f) { DesiredSpeed = GoingDownHillSpeed; return SurfaceState.GoingDownHill; }
                        break;
                }
        
                return SurfaceState.Flat;
            
            case MovementState.Spindashing:
                switch (angle)
                {
                    case 0:
                        DesiredSpeed = 1;
                        spindashDesiredAcceleration = SpinDashDeceleration;
                        return SurfaceState.Flat;
            
                    case > 0:
                        // Check the direction the player is running in to see how they are running in the slope
                        if (rb.linearVelocity.y > .01f) { DesiredSpeed = 1;
                            spindashDesiredAcceleration = SpinDashDecelerationUpSlope; return SurfaceState.GoingUpHill; }
                        
                        if (rb.linearVelocity.y < -.01f) { DesiredSpeed = SpinDashDownHillSpeed; 
                            spindashDesiredAcceleration = SpinDashDownHillAcceleration; return SurfaceState.GoingDownHill; }
                        break;
                }
        
                return SurfaceState.Flat;
            
            case MovementState.Sliding: 
                switch (angle)
                {
                    case 0:
                        DesiredSpeed = 1;
                        desiredSlideDeceleration = SlideDeceleration;
                        return SurfaceState.Flat;
            
                    case > 0:
                        if (rb.linearVelocity.y > .01f) { DesiredSpeed = 1;
                            desiredSlideDeceleration = SlideUpHillDeceleration; return SurfaceState.GoingUpHill; }
                        
                        if (rb.linearVelocity.y < -.01f) { DesiredSpeed = 1; 
                            desiredSlideDeceleration = SlideDeceleration; return SurfaceState.GoingDownHill; }
                        break;
                }
        
                return SurfaceState.Flat;
        }

        return SurfaceState.Flat;
    }

    private void HomingAttack()
    {
        // If we have a target, move towards the target and once we reach it, "bounce off". Then transition back to regular movement
        if (Target != null)
        {
            // transform.position = Vector3.MoveTowards(transform.position, Target.position, homingSpeed * Time.deltaTime);
            Vector3 targetPosition = Target.transform.position;
            Vector3 direction = (targetPosition - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, targetPosition);

            // Apply movement force to Rigidbody for a smoother transition
            rb.linearVelocity = direction * (distance / Time.deltaTime) / hommingSpeedLimiter;
            
            // If we reached the target, "bounce" and transition back to regular movement state
            if (distance < 1f)
            {
                /*
                // Replace .5f with ImpulseAfterAttackWeakMomentum or ImpulseAfterAttackWeakMomentum (depending on how much momentum you want to keep)
                // to keep momentum for the direction the player last inputted.
                // Replacing .5f by 0 makes it so the player shoots upwards only
                // All of the options mentioned above will be useful for future uses depending on whether it's speed section, or platforming, or light combat, or anything else
                */
                rb.linearVelocity = Vector3.zero;
                rb.AddForce((Vector3.up + (LastSpeedDirection * ImpulseAfterAttackWeakMomentum)) * ImpulseAfterAttack, ForceMode.Impulse);
                animManager.TriggerHomingAttackTrickAnimation();
                Debug.Log("Do trick");
                movementState = MovementState.Regular;
            }
        }
        // Otherwise, homing attack towards direction player is facing then transition to regular movement
        else
        {
            // Debug.Log("No Target selected");
            rb.AddForce(LastSpeedDirection.normalized * NoTargethomingSpeed, ForceMode.Impulse);
            CanHomingAttack = false; // If the player homing attacked and didn't hit anything, don't allow another homing attack until the player retouches the ground
            movementState = MovementState.Regular;
        }
    }

    // A modified version of MovePlayer. To anyone wanting to understand this function better, take a look a moveplayer :)
    private void SpindashMovement()
    {
        // Calculate move direction based on input and camera orientation
        moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;
        
        // If the player doesn't input anything, keep going in the direction last used. In other words, keep the ball rolling in the same direction
        if (moveDirection == Vector3.zero) { moveDirection = LastSpeedDirection; }

        // Apply movement on the slope by projecting onto the surface. In other words, get the move direction considering the surface the player is on
        Vector3 SurfaceAppliedDirection = Vector3.ProjectOnPlane(moveDirection, surfaceHit.normal);

        // Get the current surface, which also updates the desired speed (in the ground for now. Later I'll adjust air movement)
        surfaceState = SurfacePlayerIsStandingOn();
        
        // Calculate target velocity
        Vector3 targetVelocity = SurfaceAppliedDirection.normalized * DesiredSpeed;

        // Smoothly rotate towards target velocity and apply acceleration or deceleration
        float rad = SpinDashTurnSpeed * Mathf.PI * Time.deltaTime;

        float prevSpeed = horizontalVelocity.magnitude; // Store previous velocity
        // Move our current velocity towards our desired velocity
        horizontalVelocity = Vector3.RotateTowards(Vector3.ProjectOnPlane(rb.linearVelocity, surfaceHit.normal), targetVelocity, rad, 
            spindashDesiredAcceleration * Time.deltaTime);
        float currentSpeed = horizontalVelocity.magnitude; // Store current velocity
        
        if (surfaceState == lastSurfaceState && (surfaceState == SurfaceState.Flat || surfaceState == SurfaceState.GoingUpHill))
        {
            if (currentSpeed > DesiredSpeed)
            {
                horizontalVelocity.Normalize();
                horizontalVelocity *= currentSpeed;
            }
        }
        else
        {
            horizontalVelocity.Normalize();
            horizontalVelocity *= Mathf.Max(prevSpeed, currentSpeed);
        }
        
        // Update last surface
        lastSurfaceState = surfaceState;
        
        // If something is in front, clash with it
        if (DetectedSomethingInVelocityDirection(horizontalVelocity))
        {
            rb.linearVelocity = Vector3.zero;
            horizontalVelocity = Vector3.zero;
        }
        else
        {
            // Combine horizontal
            rb.linearVelocity = horizontalVelocity;
        }
    }

    private void BoostMovement()
    {
        // Calculate move direction based on input and camera orientation
        moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;
        Vector3 proofDir = moveDirection != Vector3.zero ? moveDirection : LastSpeedDirection;

        // Get surface normal based on grounding status
        Vector3 Surface = grounded ? surfaceHit.normal : Vector3.up;
    
        // Apply movement on the slope by projecting onto the surface. In other words, get the move direction considering the surface the player is on
        Vector3 SurfaceAppliedDirection = Vector3.ProjectOnPlane(proofDir, Surface);
        
        // Calculate target velocity
        Vector3 targetVelocity = SurfaceAppliedDirection.normalized * BoostSpeed;

        // Smoothly rotate towards target velocity and apply acceleration or deceleration
        float rad = BoostTurnSpeed * Mathf.PI * Time.deltaTime;
        
        // Move our current velocity towards our desired velocity
        horizontalVelocity = Vector3.RotateTowards(Vector3.ProjectOnPlane(rb.linearVelocity, Surface), targetVelocity, rad, 1f * Time.deltaTime);
        
        // Keep the velocity always at boost-speed while boosting
        horizontalVelocity.Normalize();
        horizontalVelocity *= BoostSpeed;
        
        // Preserve vertical velocity
        float verticalVelocity = rb.linearVelocity.y;

        // If grounded, reset vertical velocity. Otherwise, apply gravity to it
        if (grounded && readyToJump)  { verticalVelocity = 0f; }
        else { verticalVelocity += -gravity * Time.fixedDeltaTime;}

        // Update last surface
        lastSurfaceState = surfaceState;
        
        // Decrease boost meter
        BoostMeter = Mathf.MoveTowards(BoostMeter, 0f, (BoostConsumption/100) * Time.deltaTime);

        // If something is in front, clash with it
        if (DetectedSomethingInVelocityDirection(horizontalVelocity))
        {
            rb.linearVelocity = Vector3.zero;
            horizontalVelocity = Vector3.zero;
            movementState = MovementState.Regular;
        }
        else
        {
            // Combine horizontal and vertical velocity
            rb.linearVelocity = horizontalVelocity + transform.up * verticalVelocity;
        }
        
        
        // Go back to normal if the meter is empty
        if (BoostMeter <= 0f) { movementState = MovementState.Regular; }
    }

    private void Stomp()
    {
        rb.linearVelocity = Vector3.down * StompSpeed;
        
        // Rail takes priority
        DetectRail();
        
        if (grounded)
        {
            rb.linearVelocity = Vector3.zero;
            InStompWaitTime = true;
            StartCoroutine(AfterStompWait());
        }
    }
    
    private void DetectRail()
    {
        /*
         * The player travels very fast when stomping, so the collision between rail and player might be missed
         * because of the frame by frame window, even if the player's collision detection is continuous/continuous dynamic
         * So instead, we do our own simulation. Instead of waiting for the next frame, we predict the spot the player is going to be on the next frame
         * And if on our way to that spot we hit a rail, get on the rail. It works better because we don't have to wait for the next frame
        */
        Vector3 nextSpot = transform.position + rb.linearVelocity * Time.fixedDeltaTime;
        if (Physics.Linecast(transform.position, nextSpot, out RaycastHit stompRay, whatIsRail))
        { stompRay.transform.gameObject.GetComponent<NewRailMoveDetection>().SetupBeforeRailGrinding(this); }
    }

    IEnumerator AfterStompWait()
    {
        horizontalVelocity = Vector3.zero;
        yield return new WaitForSeconds(AfterStompWaitTime);
        InStompWaitTime = false;
        movementState = MovementState.Regular;
    }

    public void Slide()
    {
        // Calculate move direction based on input and camera orientation
        // and if the player doesn't input anything, keep going in the direction last used
        moveDirection = (orientation.forward * verticalInput + orientation.right * horizontalInput).normalized;
        if (moveDirection == Vector3.zero) { moveDirection = LastSpeedDirection; }

        // Apply movement on the slope by projecting onto the surface. In other words, get the move direction considering the surface the player is on
        Vector3 SurfaceAppliedDirection = Vector3.ProjectOnPlane(moveDirection, surfaceHit.normal);

        // Get the current surface, which also updates the desired speed
        surfaceState = SurfacePlayerIsStandingOn();
        
        Vector3 targetVelocity = SurfaceAppliedDirection.normalized * DesiredSpeed;

        // Smoothly rotate towards target velocity and apply acceleration or deceleration
        float rad = SlideTurnSpeed * Mathf.PI * Time.deltaTime;

        float prevSpeed = horizontalVelocity.magnitude;
        horizontalVelocity = Vector3.RotateTowards(Vector3.ProjectOnPlane(rb.linearVelocity, surfaceHit.normal), targetVelocity, rad, 
            desiredSlideDeceleration * Time.deltaTime);
        float currentSpeed = horizontalVelocity.magnitude;
        
        if (surfaceState == lastSurfaceState && (surfaceState == SurfaceState.Flat || surfaceState == SurfaceState.GoingUpHill))
        {
            if (currentSpeed > DesiredSpeed)
            {
                horizontalVelocity.Normalize();
                horizontalVelocity *= currentSpeed;
            }
        }
        else
        {
            horizontalVelocity.Normalize();
            horizontalVelocity *= Mathf.Max(prevSpeed, currentSpeed);
        }
        
        // Update last surface
        lastSurfaceState = surfaceState;
        
         // If something is in front, clash with it
        if (DetectedSomethingInVelocityDirection(horizontalVelocity))
        {
            rb.linearVelocity = Vector3.zero;
            horizontalVelocity = Vector3.zero;
        }
        else
        {
            // Combine horizontal
            rb.linearVelocity = horizontalVelocity;
        }
    }

    private float lastY;
    public void RailGrinding()
    {
        // Get the current surface, which also updates the desired speed and rail acceleration
        UpdateRotationForRailMovement();
        RailSpeedUpdate();
        lastY = transform.position.y;

        // Put desired speed in the right direction after magnitude has been chosen
        DesiredSpeed *= TowardsEndPoint ? 1 : -1;

        // Move rail speed towards appropriate speed
        RailStartSpeed = Mathf.MoveTowards(RailStartSpeed, DesiredSpeed, desiredRailAcceleration * Time.deltaTime);

        // Move cart
        CurrentCart.SplinePosition += RailStartSpeed * Time.deltaTime;
        
        // Move player based on cart if there's more rail left. Otherwise, get off cart with momentum
        float newPos = CurrentCart.SplinePosition;
        if (newPos < CurrentCart.Spline.CalculateLength() && newPos > 1f)
        {
            Vector3 targetPosition = CurrentCart.transform.position + (transform.up * RailOffsetPos);
            Vector3 direction = (targetPosition - transform.position).normalized;
            float distance = Vector3.Distance(transform.position, targetPosition);

            // Apply movement force to Rigidbody for a smoother transition
            rb.linearVelocity = direction * (distance / Time.deltaTime);
            lastRailDirection = GetNormalizedTangentOfRailPointBasedOnDirection();
        }
        else
        {
            CurrentCart.transform.parent.GetChild(0).GetComponentInChildren<SplineMeshCollider>().ignoreRail = true;
            CurrentCart = null;
            movementState = MovementState.Regular;
            rb.linearVelocity = lastRailDirection * RailStartSpeed;
        }
        
    }

    public Vector3 _N;
    public Vector3 _T;
    public Vector3 _right;


    void UpdateRotationForRailMovement()
    {
        // 1) Evaluate tangents
        float delta = 0.001f;

        CurrentCart.PositionUnits = PathIndexUnit.Normalized;
        Vector3 T1 = CurrentCart.Spline.EvaluateTangent(CurrentCart.SplinePosition);
        Vector3 T2 = CurrentCart.Spline.EvaluateTangent(CurrentCart.SplinePosition + delta);
        T1.Normalize();
        T2.Normalize();
        CurrentCart.PositionUnits = PathIndexUnit.Distance;

        // 2) Approx derivative
        Vector3 dT = (T2 - T1) / delta;
        // 3) If nearly zero curvature, pick a fallback up
        Vector3 N = (dT.sqrMagnitude < .01) ? Vector3.up : dT.normalized;

        // 4) Now get the actual tangent (already normalized) for the current position
        Vector3 T = T1.normalized;
        //    (flip T if needed)
        T *= TowardsEndPoint ? 1 : -1;

        // 5) Compute binormal and re-orthonormalize
        Vector3 B = Vector3.Cross(T, N).normalized;
        
        N = Vector3.Cross(B, T).normalized;
        Vector3 right = Vector3.Cross(N, T).normalized;

        // 6) Finally, rotate
        // T is forward, N is up, right is right
        transform.rotation = Quaternion.LookRotation(T, N);

        _N = N;
        _T = T;
        _right = right;
        
        // Debug.DrawRay(transform.position, right * 3f, Color.red);
    }

    Vector3 GetNormalizedTangentOfRailPointBasedOnDirection()
    {
        // Normalized position units are needed to get the tangent
        CurrentCart.PositionUnits = PathIndexUnit.Normalized;
        
        // Get tangent and normalize it
        Vector3 tangent = CurrentCart.Spline.EvaluateTangent(CurrentCart.SplinePosition);
        tangent.Normalize();
        
        // Go back to regular units when rail moving
        CurrentCart.PositionUnits = PathIndexUnit.Distance;
        return tangent;
    }

    void RailSpeedUpdate()
    {
        // Compare the y pos of the current and last frame to see if we're going up, down, or flat
        switch (transform.position.y - lastY)
        {
            case 0f:
                //Debug.Log("FLAT");
                DesiredSpeed = 0f;
                desiredRailAcceleration = railDeceleration;
                break;
            case > 0f:
                //Debug.Log("UP-HILL");
                DesiredSpeed = 0f; 
                desiredRailAcceleration = railUpDeceleration; 
                break;
                    
            case < 0f:
                //Debug.Log("DOWN-HILL");
                DesiredSpeed = GoingDownHillSpeed; 
                desiredRailAcceleration = railDownAcceleration; 
                break;
        }
        
        // If we're on a rail, simulate boost
        if (Input.GetKey(BoostKey) && BoostMeter > 0f)
        {
            DesiredSpeed = BoostSpeed;
            RailStartSpeed = BoostSpeed;
            RailStartSpeed *= TowardsEndPoint ? 1 : -1;
            BoostMeter = Mathf.MoveTowards(BoostMeter, 0f, (BoostConsumption/100) * Time.deltaTime);
        }
    }
}