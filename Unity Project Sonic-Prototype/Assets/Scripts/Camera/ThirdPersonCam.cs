using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCam : MonoBehaviour
{
    [Header("References")]
    public Transform orientation;
    public Transform player;
    public SonicMovement playerMoveScript;
    public Transform playerObj;

    public float rotationSpeed;

    void Start()
    {
        playerMoveScript = player.GetComponent<SonicMovement>();
    }

    private void FixedUpdate()
    {
        // We want the orientation of the player to be based on the loop if on the loop
        if (playerMoveScript.OnLoopDeLoop) { return ;}

        // rotate orientation
        Vector3 viewDir = player.position - new Vector3(transform.position.x, transform.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;
    }
}