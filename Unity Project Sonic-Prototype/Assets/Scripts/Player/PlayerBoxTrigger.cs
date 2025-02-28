using System.Collections;
using UnityEngine;

public class PlayerBoxTrigger : MonoBehaviour
{
    public SonicMovement player;
    public LayerMask whatIsGround;

    public float delay = .5f;
    public static bool inDelay = false;

    void Start()
    {
        player = transform.root.GetComponentInChildren<SonicMovement>();
    }

    void OnTriggerStay(Collider other)
    {
        // Make sure that wall doesn't stop player from jumping
        if ((whatIsGround & (1 << other.gameObject.layer)) != 0 && player.grounded && player.readyToJump)
        {
            // Debug.Log("Touched");
            player.TouchingWallBeforeJump = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        // Make sure that wall doesn't stop player from jumping
        if ((whatIsGround & (1 << other.gameObject.layer)) != 0)
        {
            player.TouchingWallBeforeJump = false;
            if (!(player.grounded && player.readyToJump))
            {
                inDelay = true;
                // Debug.Log("Exit");
                StartCoroutine(resetWallTouching());
            }
        }
    }

    IEnumerator resetWallTouching()
    {
        yield return new WaitForSeconds(delay);
        inDelay = false;
    }
}
