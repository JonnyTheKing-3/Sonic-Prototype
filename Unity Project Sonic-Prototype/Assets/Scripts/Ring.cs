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
        Debug.Log("Tounched something: " + other.tag);
        if (other.CompareTag("Player") || other.CompareTag("Player Trigger Collider"))
        {
            other.transform.root.GetComponentInChildren<SonicMovement>().BoostMeter += ringRefill;
            Debug.Log("Ring fill: " + other.transform.root.GetComponentInChildren<SonicMovement>().BoostMeter);
            if (other.transform.root.GetComponentInChildren<SonicMovement>().BoostMeter > 1)  // limit boost meter refill
            { other.transform.root.GetComponentInChildren<SonicMovement>().BoostMeter = 1;}
            Debug.Log("After mod: " + other.transform.root.GetComponentInChildren<SonicMovement>().BoostMeter);
            Destroy(gameObject);
            // Add to ring counter later
        }
    }
}
