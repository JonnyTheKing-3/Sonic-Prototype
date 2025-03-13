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

    private CameraFollow cam;

    public float rotationSpeed;

    void Start()
    {
        playerMoveScript = player.GetComponent<SonicMovement>();
        cam = Camera.main.transform.parent.GetComponent<CameraFollow>();
    }

    private void FixedUpdate()
    {
        // We want the orientation of the player to be based on the loop if on the loop
        if (playerMoveScript.OnLoopDeLoop || cam.cameraState == CameraFollow.CameraState.TransitioningOutOfLoop) { return; }

        // rotate orientation
        Vector3 viewDir = player.position - new Vector3(transform.position.x, transform.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;
    }
}