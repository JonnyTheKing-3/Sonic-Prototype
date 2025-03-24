using UnityEngine;

public class dashpanel : MonoBehaviour
{
    public bool switchCameraAngle = false;
    [Tooltip("If unchecked, the speed pased from the panel will be the player's GoingDownHillSpeed")]
    public bool CustomSpeedForThisPanel = false;

    [Tooltip("SpeedGiven is the custom speed passed from the panel, assuming CustomSpeedForThisPanel is checked on")]
    public float speedGiven;
    public float timerToKeepInertia;

    public float TimePanelWasTouched;
    private SonicMovement player;


    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<SonicMovement>();
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player Trigger Collider"))
        {   
            // FMODbanks.Instance.PlayDashPanelSFX(gameObject);
            // Debug.Log("touched player");
            player.CurrentDashPanel = this;
            TimePanelWasTouched = Time.time;
            
            // Setup player so bumper time can function smoothly
            player.transform.position = transform.position;
            player.grounded = true;
            player.readyToJump = true;
            player.animManager.animator.SetTrigger("HitDashPannel");
            player.movementState = SonicMovement.MovementState.Regular;

            // make input 0 so that moveplayer in sonicmovement doesn't interfere
            player.horizontalInput = 0;
            player.verticalInput = 1;
            
            // Speed given
            float panelSpeed = CustomSpeedForThisPanel ? speedGiven : player.GoingDownHillSpeed;
            player.rb.linearVelocity = transform.forward * panelSpeed;
            player.animManager.transform.forward = transform.forward;

            // Change camera angle if switchCameraAngle is true
            if (switchCameraAngle)
            {   
                // Debug.Log("switch: " + switchCameraAngle);
                CameraFollow cameraFollow = Camera.main.transform.parent.GetComponent<CameraFollow>();
                if (cameraFollow != null)
                {
                    // Debug.Log("Set new Angle");
                    cameraFollow.SetTemporaryCameraDirection(Quaternion.LookRotation(transform.forward), 1.5f);
                }
            }
        }
    }

     private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow; // Choose any color
        Gizmos.DrawRay(transform.position, transform.forward * 4f); // Adjust length as needed
    }
}
