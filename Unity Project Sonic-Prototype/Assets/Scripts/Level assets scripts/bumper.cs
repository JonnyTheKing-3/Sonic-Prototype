using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class bumper : MonoBehaviour
{
    public float bumperForce;

    [Tooltip("Once player decreases speed to this point, player is allowed to move. If you don't want to restrict the players movement, just make this number really large")]
    public float speedPlayerThresholdBeforePlayerMoves;
    private SonicMovement player;

    public enum CameraAngleChange { none, FaceBumperDirection, FaceDownwards }

    [Tooltip("Which direction the camera switches to after hitting the bumper. None makes the camera not be affected")]
    public CameraAngleChange cameraAngleChange = CameraAngleChange.none;


    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<SonicMovement>();
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player Trigger Collider"))
        {   
            FMODbanks.Instance.PlayBumperSFX(gameObject);
            player.CurrentDashPanel = null;
            player.CurrentBumper = this;
            
            // Setup player so bumper time can function smoothly
            player.transform.position = transform.position;
            // make input 0 so that moveplayer in sonicmovement doesn't interfere
            player.horizontalInput = 0;
            player.verticalInput = 0;
            player.grounded = false;
            player.readyToJump = false;
            player.movementState = SonicMovement.MovementState.OnBumperInertia;
            player.rb.linearVelocity = Vector3.zero;
            player.horizontalVelocity = Vector3.zero;

            // 'bounce' the player in the direction of the bumpers up rotation
            player.rb.AddForce(transform.up * bumperForce, ForceMode.Impulse);

            StartCoroutine(delayBeforeResetingReadyToJump());
            // setup animatior
            player.animManager.transform.forward = transform.up;

            // Switch camera angle if desired
            switch (cameraAngleChange)
            {
                case CameraAngleChange.none:
                    break;

                default:
                    CameraFollow cameraFollow = Camera.main.transform.parent.GetComponent<CameraFollow>();

                    Vector3 dir = cameraAngleChange == CameraAngleChange.FaceBumperDirection ? transform.up : Vector3.down;
                    
                    cameraFollow.SetTemporaryCameraDirection(Quaternion.LookRotation(dir), 1.5f);
                    break;
            }
        }
    }

    // Used in order to not trigger animations and gravity early
    IEnumerator delayBeforeResetingReadyToJump()
    {
        yield return new WaitForSeconds(player.jumpCooldown);
        player.readyToJump = true;
    }

     private void OnDrawGizmos()
    {
        Gizmos.color = Color.green; // Choose any color
        Gizmos.DrawRay(transform.position, transform.up * 2f); // Adjust length as needed
    }
}