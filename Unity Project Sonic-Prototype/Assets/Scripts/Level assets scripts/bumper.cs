using System;
using System.Collections;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Rendering;

public class bumper : MonoBehaviour
{
    public float bumperForce;
    public float speedPlayerThresholdBeforePlayerMoves;
    private SonicMovement player;

    void Start()
    {
        player = GameObject.FindWithTag("Player").GetComponent<SonicMovement>();
    }


    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player Trigger Collider"))
        {   
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
        }
    }

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