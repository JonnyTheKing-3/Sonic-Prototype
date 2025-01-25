using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SurfaceAlignment : MonoBehaviour
{
    public LayerMask whatIsGround;
    public float rotationRayLength;
    public float rotationSpeed = 5f;
    public float rotationDamping = 0.1f;

    private Quaternion defaultRotation;

    void Start()
    {
        defaultRotation = transform.rotation;
    }

    void FixedUpdate()
    {
        AlignWithSurface();
    }

    private void AlignWithSurface()
    {
        RaycastHit hit;
        if (Physics.Raycast(transform.position, Vector3.down, out hit, rotationRayLength, whatIsGround))
        {
            // Calculate the target rotation based on the ground normal
            Quaternion targetRotation = Quaternion.FromToRotation(transform.up, hit.normal) * transform.rotation;

            // Gradually rotate the player towards the target rotation with damping
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);
        }
        else
        {
            // If not on the ground, smoothly return to default rotation
            transform.rotation = Quaternion.Slerp(transform.rotation, defaultRotation, rotationDamping);
        }
    }
}
