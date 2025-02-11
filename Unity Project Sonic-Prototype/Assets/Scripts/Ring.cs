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
        if (other.CompareTag("Player"))
        {
            other.GetComponent<SonicMovement>().BoostMeter += ringRefill;
            Destroy(gameObject);
            // Add to ring counter later
        }
    }
}
