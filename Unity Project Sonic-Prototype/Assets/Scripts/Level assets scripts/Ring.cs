using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ring : MonoBehaviour
{
    public float rotationSpeed = 10f; // Adjust speed as needed
    [Range(0, 1)] public float ringRefill;

    void Update()
    {
        Vector3 rotationDir = Vector3.up * rotationSpeed;
        transform.Rotate(rotationDir * Time.deltaTime,  Space.Self);
    }

    // If the player touches the ring, refill boost meter a bit and destroy ring
    private void OnTriggerEnter(Collider other)
    {
        FMODbanks.Instance.PlayHomingLockOnSFX(gameObject);
        //Debug.Log("Tounched something: " + other.tag);
        if (other.CompareTag("Player") || other.CompareTag("Player Trigger Collider"))
        {
            SonicMovement player = other.transform.root.GetComponentInChildren<SonicMovement>();

            // refill boost and limit boost meter refill
            player.BoostMeter += ringRefill;
            if (player.BoostMeter > 1) { player.BoostMeter = 1;} 

            Destroy(gameObject);

            // ADD TO RING COUNTER UI
        }
    }
}
